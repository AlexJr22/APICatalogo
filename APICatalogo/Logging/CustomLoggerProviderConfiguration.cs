namespace APICatalogo.Logging;

/*

Essa é a classe base quem defini as configurações dos Logs

    -> proxima: CustomerLogger.cs

*/
public class CustomLoggerProviderConfiguration
{
    public LogLevel LogLevel { get; set; } = LogLevel.Warning;
    public int EventId { get; set; } = 0;
}
