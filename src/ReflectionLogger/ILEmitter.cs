using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ReflectionLogger
{
    class ILEmitter
    {
        public void EmitMethodBody(MethodBodyInfo methodBodyInfo)
        {
            var il = methodBodyInfo.NewMethodInfo.GetILGenerator();

            if (!IsProcedure(methodBodyInfo))
                EmitFunctionBody(methodBodyInfo, il);
            else EmitProcedureBody(methodBodyInfo, il);            
        }

        private bool IsProcedure(MethodBodyInfo methodBodyInfo)
        {
            return methodBodyInfo.NewMethodInfo.ReturnType == typeof (void);
        }

        private void EmitFunctionBody(MethodBodyInfo methodBodyInfo, ILGenerator il)
        {
            var local = EmitCreateLocal(il, methodBodyInfo.NewMethodInfo);
            EmitLogMessage(il, methodBodyInfo.LogMessagesBuilder.BuildStartLogMessage(methodBodyInfo.VirtualMethodInfo.Name),
                methodBodyInfo.LoggerField);
            EmitLogMethodParameters(methodBodyInfo.Parameters, il, methodBodyInfo.LoggerField);
            EmitCallBaseMethod(il, methodBodyInfo.VirtualMethodInfo);
            EmitSaveReturnToLocal(il, local);
            EmitLogReturn(il, methodBodyInfo.VirtualMethodInfo, local, methodBodyInfo.LoggerField);
            EmitLogMessage(il, methodBodyInfo.LogMessagesBuilder.BuildEndLogMessage(methodBodyInfo.VirtualMethodInfo.Name),
                methodBodyInfo.LoggerField);
            EmitReturnMethod(il, methodBodyInfo.VirtualMethodInfo, local);
        }

        private void EmitProcedureBody(MethodBodyInfo methodBodyInfo, ILGenerator il)
        {
            EmitLogMessage(il, methodBodyInfo.LogMessagesBuilder.BuildStartLogMessage(methodBodyInfo.VirtualMethodInfo.Name),
                methodBodyInfo.LoggerField);
            EmitLogMethodParameters(methodBodyInfo.Parameters, il, methodBodyInfo.LoggerField);
            EmitCallBaseMethod(il, methodBodyInfo.VirtualMethodInfo);
            EmitLogMessage(il, methodBodyInfo.LogMessagesBuilder.BuildEndLogMessage(methodBodyInfo.VirtualMethodInfo.Name),
                methodBodyInfo.LoggerField);
        }

        private void EmitSaveReturnToLocal(ILGenerator il, LocalBuilder local)
        {
            il.Emit(OpCodes.Stloc_S, local);            
        }

        private LocalBuilder EmitCreateLocal(ILGenerator il, MethodBuilder newMethodInfo)
        {
            return il.DeclareLocal(newMethodInfo.ReturnType);
        }

        private void EmitReturnMethod(ILGenerator il, MethodInfo methodInfo, LocalBuilder local)
        {            
            il.Emit(OpCodes.Ldloc_S, local);
            il.Emit(OpCodes.Ret);
        }

        private void EmitCallBaseMethod(ILGenerator il, MethodInfo virtualMethodInfo)
        {
            ushort index = 0;
            while (index < virtualMethodInfo.GetParameters().Length + 1)
                il.Emit(OpCodes.Ldarg, index++);

            il.Emit(OpCodes.Call, virtualMethodInfo);
        }

        private void EmitLogMethodParameters(Type[] parameters, ILGenerator il, FieldBuilder loggerField)
        {                        
            ushort index = 1;
            while (index < parameters.Length + 1)
            {
                int i = index;
                EmitCallLogger(il,
                () => il.Emit(OpCodes.Ldarg, i),
                loggerField,
                typeof(IMethodLogger).GetMethod("LogParameter")
                    .MakeGenericMethod(new[] {parameters[index - 1]}));
                index++;
            }
        }

        private void EmitLogReturn(ILGenerator il, MethodInfo methodInfo, LocalBuilder local, FieldBuilder loggerField)
        {
            EmitCallLogger(il,
                () => il.Emit(OpCodes.Ldloc_S, local),
                loggerField, typeof(IMethodLogger).GetMethod("LogReturn")
                    .MakeGenericMethod(new[] { methodInfo.ReturnType }));
        }

        private void EmitLogMessage(ILGenerator il, string message, FieldBuilder loggerField)
        {            
            EmitCallLogger(il,
                () => il.Emit(OpCodes.Ldstr, message),
                loggerField,
                typeof (IMethodLogger).GetMethod("Log", new[] {typeof (string)}));
        }

        private void EmitCallLogger(ILGenerator il, Action emitLogArgument, FieldBuilder loggerField, MethodInfo loggerMethod)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, loggerField);
            emitLogArgument.Invoke();
            il.Emit(OpCodes.Call, loggerMethod);
        }
    }
}