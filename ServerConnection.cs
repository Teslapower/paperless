namespace First;

public class ServerConnection
{
    public string Url { get; set; } = String.Empty;
    public string Password { get; set; } = String.Empty;
    public int Port { get; set; }
    public string User { get; set; } = String.Empty;
    public Uri BaseAddress => new($"{Url}:{Port}/api/");

    public override string ToString()
    {
        return $"{Url}:{Port}, User: {User}, password=******";
    }
}