namespace Busly.API.DTOs.PNR;

public class PnrLookupRequest
{
    public string Pnr { get; set; } = string.Empty;
    public string CaptchaToken { get; set; } = string.Empty;
    public string CaptchaInput { get; set; } = string.Empty;
}
