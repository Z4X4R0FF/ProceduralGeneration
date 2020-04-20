using NUnit.Framework;

namespace Unity.Build.Tests
{
    class ContextBaseTests : BuildTestsBase
    {
        class TestContextBase : ContextBase
        {
            public TestContextBase() : base() { }
            public TestContextBase(BuildPipeline pipeline, BuildConfiguration config) : base(pipeline, config) { }
        }

        class TestValueA { }
        class TestValueB { }

        [Test]
        public void HasValue()
        {
            var context = new TestContextBase();
            context.SetValue(new TestValueA());
            Assert.That(context.HasValue<TestValueA>(), Is.True);
            Assert.That(context.HasValue<TestValueB>(), Is.False);
        }

        [Test]
        public void GetValue()
        {
            var context = new TestContextBase();
            var value = new TestValueA();
            context.SetValue(value);
            Assert.That(context.GetValue<TestValueA>(), Is.EqualTo(value));
        }

        [Test]
        public void GetValue_WhenValueDoesNotExist_IsNull()
        {
            var context = new TestContextBase();
            Assert.That(context.GetValue<TestValueA>(), Is.Null);
        }

        [Test]
        public void GetOrCreateValue()
        {
            var context = new TestContextBase();
            Assert.That(context.GetOrCreateValue<TestValueA>(), Is.Not.Null);
            Assert.That(context.HasValue<TestValueA>(), Is.True);
            Assert.That(context.GetValue<TestValueA>(), Is.Not.Null);
            Assert.That(context.Values.Length, Is.EqualTo(1));
        }

        [Test]
        public void GetOrCreateValue_WhenValueExist_DoesNotThrow()
        {
            var context = new TestContextBase();
            context.SetValue(new TestValueA());
            Assert.DoesNotThrow(() => context.GetOrCreateValue<TestValueA>());
        }

        [Test]
        public void GetValueOrDefault()
        {
            var context = new TestContextBase();
            Assert.That(context.GetValueOrDefault<TestValueA>(), Is.Not.Null);
            Assert.That(context.HasValue<TestValueA>(), Is.False);
        }

        [Test]
        public void SetValue()
        {
            var context = new TestContextBase();
            context.SetValue(new TestValueA());
            context.SetValue<TestValueB>();
            Assert.That(context.HasValue<TestValueA>(), Is.True);
            Assert.That(context.GetValue<TestValueA>(), Is.Not.Null);
            Assert.That(context.HasValue<TestValueB>(), Is.True);
            Assert.That(context.GetValue<TestValueB>(), Is.Not.Null);
            Assert.That(context.Values.Length, Is.EqualTo(2));
        }

        [Test]
        public void SetValue_SkipObjectType()
        {
            var context = new TestContextBase();
            Assert.DoesNotThrow(() => context.SetValue(new object()));
            Assert.That(context.Values.Length, Is.Zero);
        }

        [Test]
        public void SetValue_SkipNullValues()
        {
            var context = new TestContextBase();
            Assert.DoesNotThrow(() => context.SetValue<object>(null));
            Assert.That(context.Values.Length, Is.Zero);
        }

        [Test]
        public void SetValue_WhenValueExist_OverrideValue()
        {
            var context = new TestContextBase();
            var instance1 = new TestValueA();
            var instance2 = new TestValueA();

            context.SetValue(instance1);
            Assert.That(context.Values, Is.EqualTo(new[] { instance1 }));

            context.SetValue(instance2);
            Assert.That(context.Values, Is.EqualTo(new[] { instance2 }));
        }

        [Test]
        public void RemoveValue()
        {
            var context = new TestContextBase();
            context.SetValue(new TestValueA());
            Assert.That(context.Values.Length, Is.EqualTo(1));
            Assert.That(context.RemoveValue<TestValueA>(), Is.True);
            Assert.That(context.Values.Length, Is.Zero);
        }
    }
}
