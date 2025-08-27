namespace Auth;

public class JwtOptions
{
    public bool ValidateIssuer { get; set; } = false;
    public bool ValidateAudience { get; set; } = false;
    public bool ValidateIssuerSigningKey  { get; set; } = false;
    public bool ValidateLifetime  { get; set; } = true;
    public string[] ValidIssuers { get; set; } = [];
    public string[] ValidAudiences { get; set; } = [];
}
