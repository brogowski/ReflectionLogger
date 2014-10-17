using System;
using NUnit.Framework;
using ReflectionLogger;
using Rhino.Mocks;

namespace ReflectionLoggerTests
{
    [TestFixture]
    class LoggedTypeBuilderTestsVirtualTypes
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
        public void CanLog_Virtual_VoidReturn_VoidParameter_Method()
        {           
            MockStartEndMessages();

            Test1Class loggedClassInstance = _loggedTypeBuilder.GetLoggedClass<Test1Class>(_mockMethodLogger);
            loggedClassInstance.Method();            
        }

        [Test]
        public void CanLog_Virtual_IntReturn_IntParameter_Method()
        {
            MockStartEndMessages();

            _mockMethodLogger.Expect(q => q.LogParameter(_value));
            _mockMethodLogger.Expect(q => q.LogReturn(_value));

            Test2Class loggedClassInstance = _loggedTypeBuilder.GetLoggedClass<Test2Class>(_mockMethodLogger);
            loggedClassInstance.Method(_value);            
        }

        [Test]
        public void CanLog_Virtual_IntReturn_VoidParameter_Get_Property()
        {
            MockStartEndMessages();

            _mockMethodLogger.Expect(q => q.LogReturn(_value));

            Test3Class loggedClassInstance = _loggedTypeBuilder.GetLoggedClass<Test3Class>(_mockMethodLogger);
            Assert.AreEqual(_value, loggedClassInstance.Property);
        }

        [Test]
        public void CanLog_Virtual_IntReturn_IntParameter_Get_Indexer()
        {
            MockStartEndMessages();

            _mockMethodLogger.Expect(q => q.LogParameter(_value));
            _mockMethodLogger.Expect(q => q.LogReturn(_value));

            Test4Class loggedClassInstance = _loggedTypeBuilder.GetLoggedClass<Test4Class>(_mockMethodLogger);
            var x = loggedClassInstance[_value];
        }

        [Test]
        public void CanLog_Virtual_voidReturn_Int_IntParameter_Set_Indexer()
        {
            MockStartEndMessages();

            const int value = _value;

            _mockMethodLogger.Expect(q => q.LogParameter(value)).Repeat.Twice();            

            Test5Class loggedClassInstance = _loggedTypeBuilder.GetLoggedClass<Test5Class>(_mockMethodLogger);
            loggedClassInstance[value] = value;
        }

        [Test]
        public void CanLog_Virtual_voidReturn_MethodParameter_Add_Event()
        {
            MockStartEndMessages();

            _mockMethodLogger.Expect(q => q.LogParameter<EventHandler>(MockEventMethod));

            Test6Class loggedClassInstance = _loggedTypeBuilder.GetLoggedClass<Test6Class>(_mockMethodLogger);
            loggedClassInstance.Event += MockEventMethod;
        }

        [Test]
        public void CanLog_Virtual_voidReturn_MethodParameter_Remove_Event()
        {
            MockStartEndMessages();

            _mockMethodLogger.Expect(q => q.LogParameter<EventHandler>(MockEventMethod));

            Test6Class loggedClassInstance = _loggedTypeBuilder.GetLoggedClass<Test6Class>(_mockMethodLogger);
            loggedClassInstance.Event -= MockEventMethod;
        }

        [Test]
        public void CanLog_WholeVirtualClass()
        {
            _mockMethodLogger.Expect(q => q.Log(_startLogMessage)).Repeat.Times(7);
            _mockMethodLogger.Expect(q => q.Log(_endLogMessage)).Repeat.Times(7);

            _mockMethodLogger.Expect(q => q.LogReturn(_value)).Repeat.Times(1+1+1);
            _mockMethodLogger.Expect(q => q.LogParameter(_value)).Repeat.Times(1+1+2);            

            _mockMethodLogger.Expect(q => q.LogParameter<EventHandler>(MockEventMethod)).Repeat.Twice();

            Test7Class loggedClassInstance = _loggedTypeBuilder.GetLoggedClass<Test7Class>(_mockMethodLogger);

            loggedClassInstance.IntMethod(_value);

            loggedClassInstance.IntProperty = 1;
            var i = loggedClassInstance.IntProperty;

            loggedClassInstance[_value] = _value;
            i = loggedClassInstance[_value];

            loggedClassInstance.Event += MockEventMethod;
            loggedClassInstance.Event -= MockEventMethod;
        }
    }

    public class Test1Class
    {
        public virtual void Method() { }
    }
    public class Test2Class
    {
        public virtual int Method(int a) { return a; }
    }    
    public class Test3Class
    {
        public virtual int Property { get { return 1; } }
    }    
    public class Test4Class
    {
        public virtual int this[int i]
        {
            get { return i; }
        }
    }
    public class Test5Class
    {
        public virtual int this[int i]
        {
            set { }
        }
    }
    public class Test6Class
    {
        public virtual event EventHandler Event;    
    }
    public class Test7Class
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
