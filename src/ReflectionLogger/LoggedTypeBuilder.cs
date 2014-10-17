using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ReflectionLogger
{
    public class LoggedTypeBuilder
    {
        private readonly AssemblyName _loggerFactoryAssembly = new AssemblyName("LoggerFactory");
        private readonly AppDomain _appDomain = System.Threading.Thread.GetDomain();
        private readonly ModuleBuilder _moduleBuilder;
        private readonly VirtualMethodsBuilder _virtualMethodsBuilder;
        private readonly InterfaceMethodsBuilder _interfaceMethodsBuilder;

        public LoggedTypeBuilder()
        {
            AssemblyBuilder assemblyBuilder = _appDomain.DefineDynamicAssembly(_loggerFactoryAssembly,
                AssemblyBuilderAccess.Run);
            _moduleBuilder = assemblyBuilder.DefineDynamicModule(_loggerFactoryAssembly.Name);
            var ilEmitter = new ILEmitter();
            _interfaceMethodsBuilder = new InterfaceMethodsBuilder(ilEmitter);
            _virtualMethodsBuilder = new VirtualMethodsBuilder(ilEmitter);
        }

        public T GetLoggedClass<T>(IMethodLogger logger) where T : class
        {
            var builder = DefineType<T>();

            var field = builder.DefineField("_internalMethodLogger_" + Guid.NewGuid(), typeof (IMethodLogger), FieldAttributes.Private);
            DefineConstructor(builder, field);

            _virtualMethodsBuilder.DefineOverrideMethods(logger.LogMessagesBuilder, builder, typeof(T), field);
            _interfaceMethodsBuilder.DefineInterfaceMethods(logger.LogMessagesBuilder, builder, typeof(T), field);

            var type = CreateType(builder);                
            return (T)Activator.CreateInstance(type, new object[]{logger});            
        }

        private void DefineConstructor(TypeBuilder builder, FieldBuilder field)
        {
            Type objType = Type.GetType("System.Object");
            ConstructorInfo objCtor = objType.GetConstructor(new Type[0]);
            ConstructorBuilder pointCtor = builder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new[] {typeof (IMethodLogger)});
            ILGenerator ctorIL = pointCtor.GetILGenerator();
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Call, objCtor);
            ctorIL.Emit(OpCodes.Ldarg_0);
            ctorIL.Emit(OpCodes.Ldarg_1);
            ctorIL.Emit(OpCodes.Stfld, field);
            ctorIL.Emit(OpCodes.Ret);
        }

        private Type CreateType(TypeBuilder builder)
        {            
            var type = builder.CreateType();
            return type;
        }

        private TypeBuilder DefineType<T>() where T : class
        {
            return _moduleBuilder.DefineType("LoggedProxy_" + typeof(T).Name + "_" + Guid.NewGuid(),
                TypeAttributes.Sealed | TypeAttributes.Class | TypeAttributes.Public, typeof (T));
        }

    }
}
