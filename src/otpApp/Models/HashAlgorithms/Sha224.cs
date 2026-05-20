using System.Security.Cryptography;

namespace otpApp.Models.HashAlgorithms;

public sealed class Sha224 : HashAlgorithm
{
    private const int BlockSize = 64;

    private static readonly uint[] Iv =
    [
        0xc1059ed8, 0x367cd507, 0x3070dd17, 0xf70e5939,
        0xffc00b31, 0x68581511, 0x64f98fa7, 0xbefa4fa4
    ];

    private static readonly uint[] K =
    [
        0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5,
        0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
        0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3,
        0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
        0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc,
        0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
        0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7,
        0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
        0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13,
        0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
        0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3,
        0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
        0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5,
        0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
        0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208,
        0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
    ];

    private readonly uint[] _state;
    private readonly byte[] _buffer;
    private int _bufferLength;
    private long _bitsProcessed;

    public Sha224()
    {
        _state = new uint[8];
        _buffer = new byte[BlockSize];
        _bufferLength = 0;
        _bitsProcessed = 0;
        HashSizeValue = 224;
        Initialize();
    }

    public override void Initialize()
    {
        Array.Copy(Iv, _state, 8);
        Array.Clear(_buffer, 0, BlockSize);
        _bufferLength = 0;
        _bitsProcessed = 0;
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize)
    {
        _bitsProcessed += cbSize * 8L;

        var offset = ibStart;
        var remaining = cbSize;

        if (_bufferLength > 0)
        {
            var copy = Math.Min(BlockSize - _bufferLength, remaining);
            Buffer.BlockCopy(array, offset, _buffer, _bufferLength, copy);
            _bufferLength += copy;
            offset += copy;
            remaining -= copy;

            if (_bufferLength == BlockSize)
            {
                ProcessBlock(_buffer, 0);
                _bufferLength = 0;
            }
        }

        while (remaining >= BlockSize)
        {
            ProcessBlock(array, offset);
            offset += BlockSize;
            remaining -= BlockSize;
        }

        if (remaining > 0)
        {
            Buffer.BlockCopy(array, offset, _buffer, 0, remaining);
            _bufferLength = remaining;
        }
    }

    protected override byte[] HashFinal()
    {
        var bits = _bitsProcessed;
        _buffer[_bufferLength++] = 0x80;

        if (_bufferLength > 56)
        {
            while (_bufferLength < BlockSize)
                _buffer[_bufferLength++] = 0;
            ProcessBlock(_buffer, 0);
            _bufferLength = 0;
        }

        while (_bufferLength < 56)
            _buffer[_bufferLength++] = 0;

        var lengthBytes = BitConverter.GetBytes(bits);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(lengthBytes);
        Buffer.BlockCopy(lengthBytes, 0, _buffer, 56, 8);

        ProcessBlock(_buffer, 0);

        var result = new byte[28];
        for (var i = 0; i < 7; i++)
        {
            var val = BitConverter.GetBytes(_state[i]);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(val);
            Buffer.BlockCopy(val, 0, result, i * 4, 4);
        }

        return result;
    }

    private void ProcessBlock(byte[] block, int offset)
    {
        Span<uint> w = stackalloc uint[64];

        for (var t = 0; t < 16; t++)
        {
            var i = offset + t * 4;
            w[t] = (uint)(block[i] << 24 | block[i + 1] << 16 | block[i + 2] << 8 | block[i + 3]);
        }

        for (var t = 16; t < 64; t++)
        {
            var s0 = RotR(w[t - 15], 7) ^ RotR(w[t - 15], 18) ^ (w[t - 15] >> 3);
            var s1 = RotR(w[t - 2], 17) ^ RotR(w[t - 2], 19) ^ (w[t - 2] >> 10);
            w[t] = w[t - 16] + s0 + w[t - 7] + s1;
        }

        var a = _state[0];
        var b = _state[1];
        var c = _state[2];
        var d = _state[3];
        var e = _state[4];
        var f = _state[5];
        var g = _state[6];
        var h = _state[7];

        for (var t = 0; t < 64; t++)
        {
            var s1 = RotR(e, 6) ^ RotR(e, 11) ^ RotR(e, 25);
            var ch = (e & f) ^ (~e & g);
            var temp1 = h + s1 + ch + K[t] + w[t];
            var s0 = RotR(a, 2) ^ RotR(a, 13) ^ RotR(a, 22);
            var maj = (a & b) ^ (a & c) ^ (b & c);
            var temp2 = s0 + maj;

            h = g;
            g = f;
            f = e;
            e = d + temp1;
            d = c;
            c = b;
            b = a;
            a = temp1 + temp2;
        }

        _state[0] += a;
        _state[1] += b;
        _state[2] += c;
        _state[3] += d;
        _state[4] += e;
        _state[5] += f;
        _state[6] += g;
        _state[7] += h;
    }

    private static uint RotR(uint value, int bits) => (value >> bits) | (value << (32 - bits));
}
