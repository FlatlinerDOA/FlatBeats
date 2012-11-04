namespace Flatliner.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using Flatliner.Functional;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            Observe.Return(1).Subscribe(set => Assert.AreEqual(1, set), ex => { });
        }

        [TestMethod]
        public void ToFunctional()
        {
            int i = 0;
            var array = System.Reactive.Linq.Observable.Range(0, 3);
            array.ToFunctional()(
                set =>
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
        public void Concat()
        {
            int i = 0;
            var sequence = Observe.Range(0, 2).Concat(Observe.Range(2, 2));
            sequence(
                set =>
                    {
                        if (i == 4)
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
        public void Range()
        {
            var i = 2;
            Observe.Range(2, 3)(set =>
                {
                    if (i == 5)
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
                                   switch (i)
                                   {
                                       case 0:
                                           Assert.AreEqual(0, result.Value);
                                           break;
                                       case 1:
                                           Assert.AreEqual(1, result.Value);
                                           break;
                                       case 2:
                                           Assert.AreEqual(1, result.Value);
                                           break;
                                       case 3:
                                           Assert.AreEqual(2, result.Value);
                                           break;
                                   }
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

        [TestMethod]
        public async Task AwaitPattern()
        {
            var x = await Observe.Return(1);
            Assert.AreEqual(1, x);
        }

        [TestMethod]
        public async Task AwaitPatternWithEmptySet()
        {
            var x = await Observe.Empty<int>();
            Assert.AreEqual(0, x);

            var y = await Observe.Empty<string>();
            Assert.AreEqual(null, y);
        }

        [TestMethod]
        public void Merge()
        {
            var a = Observe.Range(0, 2).Select(n => "A" + n);
            var b = Observe.Range(0, 2).Select(n => "B" + n);

            a.Merge(b).ToList()(
                result =>
                    {
                        var v = result.Value;
                        Assert.AreEqual("A0", v[0]);
                        Assert.AreEqual("B0", v[1]);
                        Assert.AreEqual("A1", v[2]);
                        Assert.AreEqual("B1", v[3]);
                    });
        }

        [TestMethod]
        public void MergeObservable()
        {
            var a = Observable.Range(0, 2).Select(n => "A" + n);
            var b = Observable.Range(0, 2).Select(n => "B" + n);

            a.Merge(b).ToList().Subscribe(
                result =>
                {
                    var v = result;
                    Assert.AreEqual("A0", v[0]);
                    Assert.AreEqual("B0", v[1]);
                    Assert.AreEqual("A1", v[2]);
                    Assert.AreEqual("B1", v[3]);
                });
        }
    }
}
