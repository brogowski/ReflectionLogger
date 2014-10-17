using System;
using NUnit.Framework;
using ReflectionLogger;
using Rhino.Mocks;

namespace ReflectionLoggerTests
{
    [TestFixture]
    class LoggedTypeBuilderTestsInterfaceTypes
    {
        private const int _value = 1;
        private LoggedTypeBuilder _loggedTypeBuilder;
        private ILogMessagesBuilder _mockLoggerInfo;
        private IMethodLogger _mockMethodLogger;        
        private string _endLogMessage = "END";
        private string _startLogMessage = "START";

        [SetUp]
        public void SetUp()
        {
            _mockLoggerInfo = MockRepository.GenerateStub<ILogMessagesBuilder>();
            _mockMethodLogger = MockRepository.GenerateMock<IMethodLogger>();            
            _loggedTypeBuilder = new LoggedTypeBuilder();
            MockLoggerInfo();
        }

        [TearDown]
        public void TearDown()
        {
            VerifyAllExpectations();
        }

        private void MockLoggerInfo()
        {
            _mockMethodLogger.Stub(q => q.LogMessagesBuilder).Return(_mockLoggerInfo);
            _mockLoggerInfo.Stub(q => q.BuildStartLogMessage("")).IgnoreArguments().Return(_startLogMessage);
            _mockLoggerInfo.Stub(q => q.BuildEndLogMessage("")).IgnoreArguments().Return(_endLogMessage);            
        }

        private void MockStartEndMessages()
        {
            _mockMethodLogger.Expect(q => q.Log(_startLogMessage));
            _mockMethodLogger.Expect(q => q.Log(_endLogMessage));
        }

        private void VerifyAllExpectations()
        {
            _mockLoggerInfo.VerifyAllExpectations();
            _mockMethodLogger.VerifyAllExpectations();
        }

        private void MockEventMethod(object sender, EventArgs eventArgs) { }

        [Test]
        public void CanLog_Interface_VoidReturn_VoidParameter_Method()
        {           
            MockStartEndMessages();

            Test1Interface loggedClassInstance = _loggedTypeBuilder.GetLoggedClass<Test1InterfaceImpl>(_mockMethodLogger);
            loggedClassInstance.Method();            
        }

        [Test]
        public void CanLog_Interface_IntReturn_IntParameter_Method()
        {
            MockStartEndMessages();

            _mockMethodLogger.Expect(q => q.LogParameter(_value));
            _mockMethodLogger.Expect(q => q.LogReturn(_value));

            Test2Interface loggedClassInstance = _loggedTypeBuilder.GetLoggedClass<Test2InterfaceImpl>(_mockMethodLogger);
            loggedClassInstance.Method(_value);            
        }

        [Test]
        public void CanLog_Interface_IntReturn_VoidParameter_Get_Property()
        {
            MockStartEndMessages();

            _mockMethodLogger.Expect(q => q.LogReturn(_value));

            Test3Interface loggedClassInstance = _loggedTypeBuilder.GetLoggedClass<Test3InterfaceImpl>(_mockMethodLogger);
            Assert.AreEqual(_value, loggedClassInstance.Property);
        }

        [Test]
        public void CanLog_Interface_IntReturn_IntParameter_Get_Indexer()
        {
            MockStartEndMessages();

            _mockMethodLogger.Expect(q => q.LogParameter(_value));
            _mockMethodLogger.Expect(q => q.LogReturn(_value));

            Test4Interface loggedClassInstance = _loggedTypeBuilder.GetLoggedClass<Test4InterfaceImpl>(_mockMethodLogger);
            var x = loggedClassInstance[_value];
        }

        [Test]
        public void CanLog_Interface_voidReturn_Int_IntParameter_Set_Indexer()
        {
            MockStartEndMessages();

            const int value = _value;

            _mockMethodLogger.Expect(q => q.LogParameter(value)).Repeat.Twice();

            Test5Interface loggedClassInstance = _loggedTypeBuilder.GetLoggedClass<Test5InterfaceImpl>(_mockMethodLogger);
            loggedClassInstance[value] = value;
        }

        [Test]
        public void CanLog_Interface_voidReturn_MethodParameter_Add_Event()
        {
            MockStartEndMessages();

            _mockMethodLogger.Expect(q => q.LogParameter<EventHandler>(MockEventMethod));

            Test6Interface loggedClassInstance = _loggedTypeBuilder.GetLoggedClass<Test6InterfaceImpl>(_mockMethodLogger);
            loggedClassInstance.Event += MockEventMethod;
        }

        [Test]
        public void CanLog_Interface_voidReturn_MethodParameter_Remove_Event()
        {
            MockStartEndMessages();

            _mockMethodLogger.Expect(q => q.LogParameter<EventHandler>(MockEventMethod));

            Test6Interface loggedClassInstance = _loggedTypeBuilder.GetLoggedClass<Test6InterfaceImpl>(_mockMethodLogger);
            loggedClassInstance.Event -= MockEventMethod;
        }

        [Test]
        public void CanLog_WholeInterfaceClass()
        {
            _mockMethodLogger.Expect(q => q.Log(_startLogMessage)).Repeat.Times(7);
            _mockMethodLogger.Expect(q => q.Log(_endLogMessage)).Repeat.Times(7);

            _mockMethodLogger.Expect(q => q.LogReturn(_value)).Repeat.Times(1+1+1);
            _mockMethodLogger.Expect(q => q.LogParameter(_value)).Repeat.Times(1+1+2);            

            _mockMethodLogger.Expect(q => q.LogParameter<EventHandler>(MockEventMethod)).Repeat.Twice();

            Test7Interface loggedClassInstance = _loggedTypeBuilder.GetLoggedClass<Test7InterfaceImpl>(_mockMethodLogger);

            loggedClassInstance.IntMethod(_value);

            loggedClassInstance.IntProperty = 1;
            var i = loggedClassInstance.IntProperty;

            loggedClassInstance[_value] = _value;
            i = loggedClassInstance[_value];

            loggedClassInstance.Event += MockEventMethod;
            loggedClassInstance.Event -= MockEventMethod;
        }
    }

    public interface Test1Interface
    {
        void Method();
    }
    public class Test1InterfaceImpl : Test1Interface
    {
        public virtual void Method() { }
    }
    public interface Test2Interface
    {
        int Method(int a);
    }
    public class Test2InterfaceImpl : Test2Interface
    {
        public virtual int Method(int a) { return a; }
    }
    public interface Test3Interface
    {
        int Property { get; }
    }
    public class Test3InterfaceImpl : Test3Interface
    {
        public virtual int Property { get { return 1; } }
    }
    public interface Test4Interface
    {
        int this[int i] { get; }
    }
    public class Test4InterfaceImpl : Test4Interface
    {
        public virtual int this[int i]
        {
            get { return i; }
        }
    }
    public interface Test5Interface
    {
        int this[int i] { set; }
    }
    public class Test5InterfaceImpl : Test5Interface
    {
        public virtual int this[int i]
        {
            set { }
        }
    }
    public interface Test6Interface
    {
        event EventHandler Event;
    }
    public class Test6InterfaceImpl : Test6Interface
    {
        public virtual event EventHandler Event;    
    }
    public interface Test7Interface
    {
        event EventHandler Event;
        int IntProperty { get; set; }
        int IntMethod(int i);
        int this[int i] { get; set; }
    }
    public class Test7InterfaceImpl : Test7Interface
    {
        public virtual event EventHandler Event;
        public virtual int IntProperty { get; set; }
        public virtual int IntMethod(int i) { return i; }
        public virtual int this[int i]
        {
            get { return i; }
            set {  }
        }
    }
}
