namespace Flatliner.UnitTests
{
    using System;
    using System.Linq;

    using Flatliner.Functional;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Reactive.Linq;

    [TestClass]
    public sealed class ObserveSpec
    {
        [TestMethod]
        public void Return()
        {
            int count = 0;
            int completion = 0;
            Observe.Return(1).Subscribe(
                set => count++, 
                ex => { throw ex; }, 
                () => completion++);

            Assert.AreEqual(1, count);
            Assert.AreEqual(1, completion);
        }

        [TestMethod]
        public void Select()
        {
            int result = 0;
            int completion = 0;
            Observe.Return(1).Select(t => t + 1).Subscribe(
                x => result = x, 
                ex => { throw ex; }, 
                () => completion++);
            Assert.AreEqual(2, result);
            Assert.AreEqual(1, completion);
        }

        [TestMethod]
        public void SelectNone()
        {
            Observe.Empty<int>().Select(t => t)(set => Assert.IsFalse(set.HasValue));
        }

        [TestMethod]
        public void ToObservable()
        {
            Observe.Return(1)(set => Assert.AreEqual(1, set));
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
            var i = 0;
            Observe.Range(0, 3)(set =>
                {
                    if (i == 3)
                    {
                        Assert.IsFalse(set.HasValue);
                    }
                    else
                    {
                        Assert.AreEqual(i, set.Value);
                    }

                    i++;
                });
        }

        [TestMethod]
        public void Throw()
        {
            Observe.Throw<int>(new InvalidOperationException("Epic Fail!"))(result =>
                {
                    try
                    {
                        var v = result.Value;
                        Assert.Fail("No exception thrown");
                    }
                    catch (InvalidOperationException)
                    {
                        // Expected                
                    }
                });
        }

        [TestMethod]
        public void ThrowSelect()
        {
            Observe.Throw<int>(new InvalidOperationException("Epic Fail!")).Select(t => t)(result =>
            {
                try
                {
                    var v = result.Value;
                    Assert.Fail("No exception thrown");
                }
                catch (InvalidOperationException)
                {
                    // Expected                
                }
            });
        }

        [TestMethod]
        public void SelectMany()
        {
            int i = 0;
            Observe.Range(0, 2).SelectMany(t => Observe.Range(0, 2))(
                result =>
                {
                    if (i == 4)
                    {
                        Assert.IsFalse(result.HasValue);
                    }
                    else
                    {
                        Assert.AreEqual(i % 2, result.Value);
                    }

                    i++;
                });
        }

        [TestMethod]
        public void LinqSelectMany()
        {
            int i = 0;
            (from a in Observe.Range(0, 2)
             from b in Observe.Range(0, 2) 
             select a + b)(result =>
                           {
                               if (i == 4)
                               {
                                   Assert.IsFalse(result.HasValue);
                               }
                               else
                               {
                                   Assert.AreEqual(i % 2, result.Value);
                               }

                               i++;
                           });
        }

        [TestMethod]
        public void ThrowSelectMany()
        {
            bool handledError = false;
            Observe.Throw<int>(new InvalidOperationException("Epic Fail!"))
                .SelectMany(t => Observe.Return(1)).Subscribe(
                value => Assert.Fail("No exception thrown"),
                ex => { handledError = true; }, 
                () => Assert.Fail("No exception thrown"));
            Assert.IsTrue(handledError);
        }
    }
}
