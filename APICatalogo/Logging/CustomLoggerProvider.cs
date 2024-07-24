using System.Collections.Concurrent;

namespace APICatalogo.Logging;

/*

Essa é a classe que chama as classes que geram os loggs
ou melhor ela é a class provedora dos loggs
 
*/
public class CustomLoggerProvider : ILoggerProvider
{
    readonly CustomLoggerProviderConfiguration loggerConfig;

    readonly ConcurrentDictionary<string, CustomerLogger> loggers =
               new ConcurrentDictionary<string, CustomerLogger>();

    public CustomLoggerProvider(CustomLoggerProviderConfiguration configurarion)
    {
        loggerConfig = configurarion;
    }
    public ILogger CreateLogger(string categoryName)
    {
        return loggers.GetOrAdd(categoryName, name =>
            new CustomerLogger(name, loggerConfig));
    }
    public void Dispose()
    {
        loggers.Clear();
    }
}
