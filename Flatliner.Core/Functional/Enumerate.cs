namespace Flatliner.Functional
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Functional version of IEnumerable
    /// </summary>
    public static class Enumerate
    {
        public static Enumerate<T> ToFunctional<T>(this IEnumerable<T> source)
        {
            return () =>
                {
                    var e = source.GetEnumerator();
                    return () => e.MoveNext()
                                     ? (IMaybe<T>)new Some<T>(e.Current)
                                     : (IMaybe<T>)new None<T>();
                };
        }

        public static IEnumerable<T> ToEnumerable<T>(this Enumerate<T> source)
        {
            var e = source();
            IMaybe<T> value;
            while (!((value = e()) is None<T>))
            {
                yield return value.Value;
            }
        }

        public static List<T> ToList<T>(this Enumerate<T> source)
        {
            var list = new List<T>();
            var e = source();
            IMaybe<T> value;
            while (!((value = e()) is None<T>))
            {
                list.Add(value.Value);
            }

            return list;
        }

        public static Enumerate<T> Empty<T>()
        {
            return () => () => new None<T>();
        }

        public static Enumerate<T> Return<T>(T value)
        {
            return () =>
                {
                    int i = 0;
                    return () =>
                           i++ == 0
                               ? (IMaybe<T>)new Some<T>(value)
                               : (IMaybe<T>)new None<T>();
                };
        }


        public static Enumerate<int> Range(int start, int count)
        {
            return () =>
            {
                int i = start;
                return () =>
                    {
                        var result = i < start + count ? new Some<int>(i) : (IMaybe<int>)new None<int>();
                        i++;
                        return result;
                    };
            };
        }

        public static Enumerate<T> Throw<T>(Exception error)
        {
            return () => () => new Error<T>(error);
        }

        public static Enumerate<TResult> SelectMany<T, TResult>(this Enumerate<T> source, Func<T, Enumerate<TResult>> selector)
        {
            return () =>
                {
                    var e = source();
                    IMaybe<T> lastOuter = new None<T>();
                    IMaybe<TResult> lastInner = new None<TResult>();
                    Enumeration<TResult> innerSet = null;
                    return () =>
                        {
                            do
                            {
                                while (lastInner is None<TResult>)
                                {
                                    lastOuter = e();

                                    if (!lastOuter.HasValue)
                                    {
                                        return lastOuter.TransposeEmpty<TResult>();
                                    }

                                    innerSet = selector(lastOuter.Value)();
                                    lastInner = innerSet();
                                    if (lastInner is Some<TResult>)
                                    {
                                        return lastInner;
                                    }
                                }

                                lastInner = innerSet();
                            } 
                            while (lastInner is None<TResult>);

                            return lastInner;
                        };
                };
        }

        /// <summary>
        /// Takes an Enumerable{T} and a Function that takes each T and converts to an Enumerable{C} then combines the results and returns an enumerable of R
        /// </summary>
        /// <typeparam name="TOuter"></typeparam>
        /// <typeparam name="TInner"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <param name="combiner"></param>
        /// <returns></returns>
        public static Enumerate<TResult> SelectMany<TOuter, TInner, TResult>(this Enumerate<TOuter> source, Func<TOuter, Enumerate<TInner>> selector, Func<TOuter, TInner, TResult> combiner)
        {
            return () =>
                {
                    var outerSet = source();
                    IMaybe<TOuter> lastOuter = new None<TOuter>();
                    IMaybe<TInner> lastInner = new None<TInner>();
                    Enumeration<TInner> innerSet = null;
                    return () =>
                        {
                            do
                            {
                                while (lastInner is None<TResult>)
                                {
                                    lastOuter = outerSet();
                                    if (!lastOuter.HasValue)
                                    {
                                        return lastOuter.TransposeEmpty<TResult>();
                                    }

                                    innerSet = selector(lastOuter.Value)();
                                    lastInner = innerSet();
                                    if (lastInner is Some<TInner>)
                                    {
                                        return new Some<TResult>(combiner(lastOuter.Value, lastInner.Value));
                                    }
                                }

                                lastInner = innerSet();
                            }
                            while (lastInner is None<TResult>);

                            return new Some<TResult>(combiner(lastOuter.Value, lastInner.Value));
                        };
                };
        }

        public static Enumerate<TResult> Select<T, TResult>(this Enumerate<T> source, Func<T, TResult> selector)
        {
            return source.SelectMany(a => Return(selector(a)));
        }

        public static Enumerate<T> Where<T>(this Enumerate<T> source, Func<T, bool> filter)
        {
            return source.SelectMany(t => filter(t) ? Return(t) : Empty<T>());
        }
    }
}