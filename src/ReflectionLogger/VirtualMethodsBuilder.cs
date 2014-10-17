using System;
using System.Collections.Generic;
using System.Linq;
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
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(q => q.IsVirtual && !q.IsFinal);
        }

        private Type[] GetMethodParametersTypes(MethodInfo virtualMethodInfo)
        {
            return virtualMethodInfo.GetParameters().Select(q => q.ParameterType).ToArray();
        }

        private MethodBuilder DefineNewVirtualMethod(TypeBuilder builder, MethodInfo virtualMethodInfo, Type[] parameters)
        {
            return builder.DefineMethod(virtualMethodInfo.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                virtualMethodInfo.ReturnType, parameters);
        }
    }
}