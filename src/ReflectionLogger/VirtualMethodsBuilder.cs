using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace ReflectionLogger
{
    class VirtualMethodsBuilder
    {
        private readonly ILEmitter _ilEmitter;

        public VirtualMethodsBuilder(ILEmitter ilEmitter)
        {
            _ilEmitter = ilEmitter;
        }

        public void DefineOverrideMethods(ILogMessagesBuilder logMessagesBuilder, TypeBuilder builder, Type type, FieldBuilder loggerField)
        {
            foreach (var virtualMethodInfo in GetPureVirtualMethods(type))
            {
                var parameters = GetMethodParametersTypes(virtualMethodInfo);
                var newMethodInfo = DefineNewVirtualMethod(builder, virtualMethodInfo, parameters);

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
        }

        private IEnumerable<MethodInfo> GetPureVirtualMethods(Type type)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (method.IsVirtual && !method.IsFinal)
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

        private MethodBuilder DefineNewVirtualMethod(TypeBuilder builder, MethodInfo virtualMethodInfo, Type[] parameters)
        {
            return builder.DefineMethod(virtualMethodInfo.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                virtualMethodInfo.ReturnType, parameters);
        }
    }
}