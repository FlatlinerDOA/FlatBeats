
namespace Flatliner.UnitTests
{
    using System;
    using System.Linq;
    using Flatliner.Functional;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public sealed class EnumerateSpec
    {
        [TestMethod]
        public void Return()
        {
            var set = Enumerate.Return(1)();
            Assert.AreEqual(1, set().Value);
            Assert.IsFalse(set().HasValue);
        }

        [TestMethod]
        public void Select()
        {
            var set = Enumerate.Return(1).Select(t => t + 1)();
            Assert.AreEqual(2, set().Value);
            Assert.IsFalse(set().HasValue);
        }

        [TestMethod]
        public void SelectNone()
        {
            var set = Enumerate.Empty<int>().Select(t => t)();
            Assert.IsFalse(set().HasValue);
        }

        [TestMethod]
        public void ToEnumerable()
        {
            var set = Enumerate.Return(1).ToEnumerable();
            Assert.AreEqual(1, set.Single());
        }

        [TestMethod]
        public void ToFunctional()
        {
            var array = new[] { 1, 2, 3 };
            var set = array.ToFunctional()();
            Assert.AreEqual(1, set().Value);
            Assert.AreEqual(2, set().Value);
            Assert.AreEqual(3, set().Value);
            Assert.IsFalse(set().HasValue);
        }

        [TestMethod]
        public void Range()
        {
            var set = Enumerate.Range(2, 3)();
            Assert.AreEqual(2, set().Value);
            Assert.AreEqual(3, set().Value);
            Assert.AreEqual(4, set().Value);
            Assert.IsFalse(set().HasValue);
        }

        [TestMethod]
        public void Throw()
        {
            var set = Enumerate.Throw<int>(new InvalidOperationException("Epic Fail!"))();
            try
            {
                var v = set().Value;
                Assert.Fail("No exception thrown");
            } 
            catch (InvalidOperationException)
            {
                // Expected                
            }
        }

        [TestMethod]
        public void ThrowSelect()
        {
            var set = Enumerate.Throw<int>(new InvalidOperationException("Epic Fail!")).Select(t => t)();
            try
            {
                var v = set().Value;
                Assert.Fail("No exception thrown");
            }
            catch (InvalidOperationException)
            {
                // Expected                
            }
        }

        [TestMethod]
        public void SelectMany()
        {
            var set = Enumerate.Range(0, 2).SelectMany(t => Enumerate.Range(0, 2))();
            Assert.AreEqual(0, set().Value);
            Assert.AreEqual(1, set().Value);
            Assert.AreEqual(0, set().Value);
            Assert.AreEqual(1, set().Value);
            Assert.IsFalse(set().HasValue);
        }

        [TestMethod]
        public void LinqSelectMany()
        {
            var set = (from a in Enumerate.Range(0, 2)
                       from b in Enumerate.Range(0, 2)
                      select a + b)();
            Assert.AreEqual(0, set().Value);
            Assert.AreEqual(1, set().Value);
            Assert.AreEqual(1, set().Value);
            Assert.AreEqual(2, set().Value);
            Assert.IsFalse(set().HasValue);
        }

        [TestMethod]
        public void ThrowSelectMany()
        {
            var set = Enumerate.Throw<int>(new InvalidOperationException("Epic Fail!")).SelectMany(t => Enumerate.Return(1))();
            try
            {
                var v = set().Value;
                Assert.Fail("No exception thrown");
            }
            catch (InvalidOperationException)
            {
                // Expected                
            }
        }
    }
}
