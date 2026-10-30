namespace IndustIoT.Api.ViewModels.Responses;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
}
