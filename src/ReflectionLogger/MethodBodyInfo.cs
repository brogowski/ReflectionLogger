using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ReflectionLogger
{
    class MethodBodyInfo
    {
        public MethodBuilder NewMethodInfo;
        public MethodInfo VirtualMethodInfo;
        public Type[] Parameters;
        public FieldBuilder LoggerField;
        public ILogMessagesBuilder LogMessagesBuilder;
    }
}
