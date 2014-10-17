namespace ReflectionLogger
{
    public interface IMethodLogger
    {
        void Log(string log);
        void LogParameter<T>(T value);        
        void LogReturn<T>(T value);
        ILogMessagesBuilder LogMessagesBuilder { get; }
    }

    public interface ILogMessagesBuilder
    {
        string BuildStartLogMessage(string loggedClassName);
        string BuildEndLogMessage(string loggedClassName);
    }
}
