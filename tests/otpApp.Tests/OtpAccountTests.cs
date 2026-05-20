using otpApp.Models;

namespace otpApp.Tests;

public class OtpAccountTests
{
    [Fact]
    public void NormalizeSecret_RemovesSpacesDashesPadding_UpperCases()
    {
        var result = OtpAccount.NormalizeSecret( " jbs w y3dpe=hk3p-xp " );

        result.Should().Be( "JBSWY3DPEHK3PXP" );
    }
}
