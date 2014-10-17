using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace ReflectionLogger
{
    class InterfaceMethodsBuilder
    {
        private readonly ILEmitter _ilEmitter;

        public InterfaceMethodsBuilder(ILEmitter ilEmitter)
        {
            _ilEmitter = ilEmitter;
        }

        public void DefineInterfaceMethods(ILogMessagesBuilder logMessagesBuilder, TypeBuilder builder, Type type, FieldBuilder loggerField)
        {            
            foreach (var virtualMethodInfo in GetFinalVirtualMethods(type))
            {
                var parameters = GetMethodParametersTypes(virtualMethodInfo);
                var newMethodInfo = DefineNewInterfaceMethod(builder, virtualMethodInfo, parameters);

                var args = new MethodBodyInfo
                {
                    LogMessagesBuilder = logMessagesBuilder,
                    LoggerField = loggerField,
                    NewMethodInfo = newMethodInfo,
                    Parameters = parameters,
                    VirtualMethodInfo = virtualMethodInfo
                };
                
                _ilEmitter.EmitMethodBody(args);
            }
            
            foreach(var @interface in type.GetInterfaces())
                builder.AddInterfaceImplementation(@interface);
        }

        private IEnumerable<MethodInfo> GetFinalVirtualMethods(Type type)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (method.IsVirtual && method.IsFinal)
                    yield return method;
            }                        
        }

        private Type[] GetMethodParametersTypes(MethodInfo virtualMethodInfo)
        {
            var parameters = virtualMethodInfo.GetParameters();
            var types = new Type[parameters.Length];
            for (int index = 0; index < parameters.Length; index++)
            {
                types[index] = parameters[index].ParameterType;
            }
            return types;
        }

        private MethodBuilder DefineNewInterfaceMethod(TypeBuilder builder, MethodInfo virtualMethodInfo, Type[] parameters)
        {
            return builder.DefineMethod(virtualMethodInfo.Name,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                virtualMethodInfo.ReturnType, parameters);
        }
    }
}
