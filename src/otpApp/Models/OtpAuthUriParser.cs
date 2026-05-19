namespace otpApp.Models;

public static class OtpAuthUriParser
{
    public static OtpAccount? Parse(string input)
    {
        if (!Uri.TryCreate(input.Trim(), UriKind.Absolute, out var uri))
            return null;

        if (!string.Equals(uri.Scheme, "otpauth", StringComparison.OrdinalIgnoreCase))
            return null;

        var host = uri.Host;
        OtpType type;
        if (string.Equals(host, "totp", StringComparison.OrdinalIgnoreCase))
            type = OtpType.Totp;
        else if (string.Equals(host, "hotp", StringComparison.OrdinalIgnoreCase))
            type = OtpType.Hotp;
        else
            return null;

        var path = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
        var parts = path.Split(':', 2);
        var issuerFromPath = parts.Length > 1 ? parts[0] : "";
        var label = parts.Length > 1 ? parts[1] : parts[0];

        var query = ParseQueryString(uri.Query);
        var secret = query.GetValueOrDefault("secret") ?? "";
        if (string.IsNullOrWhiteSpace(secret))
            return null;

        var issuerFromQuery = query.GetValueOrDefault("issuer") ?? "";
        var issuer = !string.IsNullOrWhiteSpace(issuerFromQuery) ? issuerFromQuery : issuerFromPath;

        var algorithm = query.GetValueOrDefault("algorithm") ?? "SHA1";
        var digits = int.TryParse(query.GetValueOrDefault("digits"), out var d) ? d : 6;
        var period = int.TryParse(query.GetValueOrDefault("period"), out var p) ? p : 30;
        var counter = long.TryParse(query.GetValueOrDefault("counter"), out var c) ? c : 0;

        if (digits != 6 && digits != 8)
            digits = 6;

        var algo = algorithm.ToUpperInvariant() switch
        {
            "SHA256" => OtpAlgorithm.SHA256,
            "SHA512" => OtpAlgorithm.SHA512,
            _ => OtpAlgorithm.SHA1
        };

        return new OtpAccount
        {
            Type = type,
            Issuer = issuer.Trim(),
            Label = label.Trim(),
            SecretBase32 = secret.Trim().ToUpperInvariant(),
            Algorithm = algo,
            Digits = digits,
            Period = period,
            HotpCounter = counter,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static Dictionary<string, string> ParseQueryString(string query)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(query) || query.Length < 2)
            return dict;

        foreach (var part in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var eq = part.IndexOf('=');
            if (eq < 0)
                continue;

            var key = Uri.UnescapeDataString(part[..eq]);
            var value = Uri.UnescapeDataString(part[(eq + 1)..]);
            dict[key] = value;
        }

        return dict;
    }
}