using System.Security.Cryptography;

namespace otpApp.Models.HashAlgorithms;

public sealed class Sha3 : HashAlgorithm
{
    private readonly int _rate;
    private readonly int _outputSize;

    private readonly ulong[,] _state;
    private readonly byte[] _buffer;
    private int _bufferLength;

    private static readonly ulong[] Rc =
    [
        0x0000000000000001, 0x0000000000008082, 0x800000000000808A,
        0x8000000080008000, 0x000000000000808B, 0x0000000080000001,
        0x8000000080008081, 0x8000000000008009, 0x000000000000008A,
        0x0000000000000088, 0x0000000080008009, 0x000000008000000A,
        0x000000008000808B, 0x800000000000008B, 0x8000000000008089,
        0x8000000000008003, 0x8000000000008002, 0x8000000000000080,
        0x000000000000800A, 0x800000008000000A, 0x8000000080008081,
        0x8000000000008080, 0x0000000080000001, 0x8000000080008008
    ];

    private static readonly int[] RhoOffsets =
    [
        0, 1, 62, 28, 27, 36, 44, 6, 55, 20, 3, 10, 43,
        25, 39, 41, 45, 15, 21, 8, 18, 2, 61, 56, 14
    ];

    public Sha3(int hashBits)
    {
        _outputSize = hashBits / 8;
        _rate = (1600 - hashBits * 2) / 8;
        _state = new ulong[5, 5];
        _buffer = new byte[_rate];
        _bufferLength = 0;
        HashSizeValue = hashBits;
    }

    public override void Initialize()
    {
        Array.Clear(_state, 0, 25);
        Array.Clear(_buffer, 0, _rate);
        _bufferLength = 0;
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize)
    {
        var offset = ibStart;
        var remaining = cbSize;

        while (remaining > 0)
        {
            var space = _rate - _bufferLength;
            var copy = Math.Min(space, remaining);
            Buffer.BlockCopy(array, offset, _buffer, _bufferLength, copy);
            _bufferLength += copy;
            offset += copy;
            remaining -= copy;
            if (_bufferLength == _rate)
            {
                Absorb();
                _bufferLength = 0;
            }
        }
    }

    protected override byte[] HashFinal()
    {
        _buffer[_bufferLength++] = 0x06;

        if (_bufferLength == _rate)
        {
            Absorb();
            _bufferLength = 0;
        }

        Array.Clear(_buffer, _bufferLength, _rate - _bufferLength);
        _buffer[_rate - 1] = 0x80;
        _bufferLength = _rate;
        Absorb();

        var result = new byte[_outputSize];
        Squeeze(result);
        return result;
    }

    private void Absorb()
    {
        for (var i = 0; i < _rate / 8; i++)
        {
            var x = i % 5;
            var y = i / 5;
            var value = (ulong)_buffer[i * 8]
                      | (ulong)_buffer[i * 8 + 1] << 8
                      | (ulong)_buffer[i * 8 + 2] << 16
                      | (ulong)_buffer[i * 8 + 3] << 24
                      | (ulong)_buffer[i * 8 + 4] << 32
                      | (ulong)_buffer[i * 8 + 5] << 40
                      | (ulong)_buffer[i * 8 + 6] << 48
                      | (ulong)_buffer[i * 8 + 7] << 56;
            _state[x, y] ^= value;
        }

        KeccakF();
    }

    private void Squeeze(byte[] output)
    {
        var remaining = _outputSize;
        var off = 0;

        while (remaining > 0)
        {
            var copy = Math.Min(_rate, remaining);
            for (var i = 0; i < copy / 8 && i < _rate / 8; i++)
            {
                var x = i % 5;
                var y = i / 5;
                var value = _state[x, y];
                var pos = off + i * 8;
                output[pos] = (byte)(value);
                output[pos + 1] = (byte)(value >> 8);
                output[pos + 2] = (byte)(value >> 16);
                output[pos + 3] = (byte)(value >> 24);
                output[pos + 4] = (byte)(value >> 32);
                output[pos + 5] = (byte)(value >> 40);
                output[pos + 6] = (byte)(value >> 48);
                output[pos + 7] = (byte)(value >> 56);
            }
            off += copy;
            remaining -= copy;
            if (remaining > 0)
                KeccakF();
        }
    }

    private void KeccakF()
    {
        for (var round = 0; round < 24; round++)
        {
            Theta();
            Rho();
            Pi();
            Chi();
            Iota(round);
        }
    }

    private void Theta()
    {
        var c0 = _state[0, 0] ^ _state[0, 1] ^ _state[0, 2] ^ _state[0, 3] ^ _state[0, 4];
        var c1 = _state[1, 0] ^ _state[1, 1] ^ _state[1, 2] ^ _state[1, 3] ^ _state[1, 4];
        var c2 = _state[2, 0] ^ _state[2, 1] ^ _state[2, 2] ^ _state[2, 3] ^ _state[2, 4];
        var c3 = _state[3, 0] ^ _state[3, 1] ^ _state[3, 2] ^ _state[3, 3] ^ _state[3, 4];
        var c4 = _state[4, 0] ^ _state[4, 1] ^ _state[4, 2] ^ _state[4, 3] ^ _state[4, 4];

        var d0 = c4 ^ RotL64(c1, 1);
        var d1 = c0 ^ RotL64(c2, 1);
        var d2 = c1 ^ RotL64(c3, 1);
        var d3 = c2 ^ RotL64(c4, 1);
        var d4 = c3 ^ RotL64(c0, 1);

        for (var y = 0; y < 5; y++)
        {
            _state[0, y] ^= d0;
            _state[1, y] ^= d1;
            _state[2, y] ^= d2;
            _state[3, y] ^= d3;
            _state[4, y] ^= d4;
        }
    }

    private void Rho()
    {
        var index = 0;
        for (var x = 0; x < 5; x++)
            for (var y = 0; y < 5; y++)
                _state[x, y] = RotL64(_state[x, y], RhoOffsets[index++]);
    }

    private void Pi()
    {
        var temp = new ulong[5, 5];
        for (var x = 0; x < 5; x++)
            for (var y = 0; y < 5; y++)
                temp[y, (2 * x + 3 * y) % 5] = _state[x, y];

        for (var x = 0; x < 5; x++)
            for (var y = 0; y < 5; y++)
                _state[x, y] = temp[x, y];
    }

    private void Chi()
    {
        for (var y = 0; y < 5; y++)
        {
            var t0 = _state[0, y];
            var t1 = _state[1, y];
            var t2 = _state[2, y];
            var t3 = _state[3, y];
            var t4 = _state[4, y];

            _state[0, y] = t0 ^ (~t1 & t2);
            _state[1, y] = t1 ^ (~t2 & t3);
            _state[2, y] = t2 ^ (~t3 & t4);
            _state[3, y] = t3 ^ (~t4 & t0);
            _state[4, y] = t4 ^ (~t0 & t1);
        }
    }

    private void Iota(int round)
    {
        _state[0, 0] ^= Rc[round];
    }

    private static ulong RotL64(ulong value, int bits)
    {
        return (value << bits) | (value >> (64 - bits));
    }
}
