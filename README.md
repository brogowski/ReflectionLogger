ReflectionLogger
================

ReflectionLogger is a automatic class builder.
Class builded by ReflectionLogger have logged every virtual method and every interface implementation method.

ReflectionLogger builds classes on runtime based on type provided.

**Supported Framework: .NET 2.0+**

**Supported classes:**
-	Only non sealed and public classes.

**Supported methods:**

-	All virtual methods (event add/remove, property get/set, indexer get/set, normal methods) are overriden.
-	All interface implementation methods (event add/remove, property get/set, indexer get/set, normal methods) are hidden by 	logged implementation.

All new methods are invoking base implementations.

**Logging:**

Logging mechanism is based on interface IMethodLogger.

**Example:**

	IMethodLogger mylogger = new MyLogger(); //Simple implementation of IMethodLogger.  
	var builder = new LoggedTypeBuilder();
	var loggedClassInstance = builder.GetClass<MyClass>(mylogger);

**Interfaces WARNING!**
When using with interfaces cast result class to desired interface to invoke new methods otherwise base class imeplementation methods will be called directly.

*Sample MyClass:*


	public class MyClass : IMyInterface
	{
	    public int IMyInterface.GetStatus() { return 1; }
	    public virtual string GetInfo(int a) { return string.Empty + a; }
	}

*MyClass after building with ReflectionLogger:*

	public class LoggedProxy_MyClass_GuidNumber : MyClass, IMyInterface
	{
	  	private IMethodLogger _methodLogger;
	  	private string _startMessage;
	  	private string _endMessage;
      
    	public LoggedProxy_MyClass_GuidNumber(IMethodLogger logger)
    	{
    	    _methodLogger = logger;
    	    _startMessage = logger.LogMessagesBuilder.BuildStartLogMessage("LoggedProxy_MyClass_GuidNumber");
    	    _endMessage = logger.LogMessagesBuilder.BuildEndLogMessage("LoggedProxy_MyClass_GuidNumber");
    	}
      
    	public new int IMyInterface.GetStatus(int a)
    	{ 
    	    _methodLogger.Log(_startMessage);
    	    _methodLogger.LogParameter<int>(a);
    	    int temp = base.GetStatus(a);
    	    _methodLogger.LogReturn<int>(temp);
    	    _methodLogger.Log(_endMessage);
    	    return temp;
    	}
    	
    	public override string GetInfo()
    	{
    	    _methodLogger.Log(_startMessage);
    	    string temp = base.GetInfo();
    	    _methodLogger.LogReturn<string>(temp);
    	    _methodLogger.Log(_endMessage);
    	    return temp;
    	}
	}
