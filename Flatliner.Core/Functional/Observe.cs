namespace Flatliner.Functional
{
    using System;
    using System.Collections.Generic;


    public static class Observe
    {
        public static Observable<T> Empty<T>()
        {
            return o => o(new None<T>());
        }

        public static Observable<T> Return<T>(T value)
        {
            return o =>
                {
                    o(new Some<T>(value));
                    o(new None<T>());
                };
        }

        public static Observable<int> Range(int start, int count)
        {
            return handler =>
                {
                    for (int i = start; i < start + count; i++)
                    {
                        handler(new Some<int>(i));   
                    }

                    handler(new None<int>());
                };
        }

        public static void Subscribe<T>(this Observable<T> source, Action<T> onNext, Action<Exception> onError)
        {
            source(
                result =>
                    {
                        if (result is Error<T>)
                        {
                            onError(((Error<T>)result).Exception);
                        } 
                        else if (result is Some<T>)
                        {
                            onNext(result.Value);
                        }
                    });
        }

        public static void Subscribe<T>(this Observable<T> source, Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            source(
                result =>
                    {
                        if (result is Error<T>)
                        {
                            onError(((Error<T>)result).Exception);
                        }
                        else if (result is Some<T>)
                        {
                            onNext(result.Value);
                        }
                        else if (result is None<T>)
                        {
                            onCompleted();
                        }
                    });
        }

        public static Observable<T> Throw<T>(Exception error)
        {
            return o => o(new Error<T>(error));
        }

        public static Observable<TInner> SelectMany<TOuter, TInner>(this Observable<TOuter> source, Func<TOuter, Observable<TInner>> innerSource)
        {
            return o => source(x =>
                {
                    if (!x.HasValue)
                    {
                        o(x.TransposeEmpty<TInner>());
                    }
                    else
                    {
                        innerSource(x.Value)(
                            y =>
                                {
                                    if (y is Some<TInner>)
                                    {
                                        o(y);
                                    }
                                });
                    }
                });
        }

        /// <summary>
        /// Takes an Enumerable{T} and a Function that takes each T and converts to an Enumerable{C} then 
        /// combines the results and returns an enumerable of TResult
        /// </summary>
        /// <typeparam name="TOuter"></typeparam>
        /// <typeparam name="TInner"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="innerSource"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static Observable<TResult> SelectMany<TOuter, TInner, TResult>(this Observable<TOuter> source, Func<TOuter, Observable<TInner>> innerSource, Func<TOuter, TInner, TResult> selector)
        {
            return o => source(
                outerSet =>
                    {
                        if (!outerSet.HasValue)
                        {
                            o(outerSet.TransposeEmpty<TResult>());
                        }
                        else
                        {
                            var innerSet = innerSource(outerSet.Value);
                            innerSet(
                                innerValue =>
                                    {
                                        if (innerValue is Some<TInner>)
                                        {
                                            o(new Some<TResult>(selector(outerSet.Value, innerValue.Value)));
                                        }
                                    });
                        }
                    });
        }

        public static Observable<TResult> Select<T, TResult>(this Observable<T> source, Func<T, TResult> selector)
        {
            return o => source(x => o(!x.HasValue ? x.TransposeEmpty<TResult>() : new Some<TResult>(selector(x.Value))));
        }

        public static Observable<T> Where<T>(this Observable<T> source, Func<T, bool> filter)
        {
            return source.SelectMany(t => filter(t) ? Return(t) : Empty<T>());
        }

        public static Observable<T> DefaultIfEmpty<T>(this Observable<T> source)
        {
            return source.DefaultIfEmpty(default(T));
        }

        public static Observable<T> DefaultIfEmpty<T>(this Observable<T> source, T defaultValue)
        {
            bool isEmpty = true;
            return o => source(t =>
                {
                    if (!isEmpty)
                    {
                        o(t);
                    }

                    if (!(t is None<T>))
                    {
                        isEmpty = false;
                    }

                    o(isEmpty ? new Some<T>(defaultValue) : t);
                });
        }

        public static Observable<T> Concat<T>(this Observable<T> source, Observable<T> secondSource)
        {
            return o => source(x =>
            {
                if (x is None<T>)
                {
                    secondSource(o);
                } 
                else
                {
                    o(x);
                }
            });
        }


        public static Observable<T> Merge<T>(this Observable<T> source, params Observable<T>[] secondSources)
        {
            var queue = new Queue<IMaybe<T>>();
            return o => 
            { 
                source(v => PushToQueue(queue, v, o));
                foreach (var secondSource in secondSources)
                {
                    secondSource(v => PushToQueue(queue, v, o));
                }

                lock (queue)
                {
                    if (queue.Count != 0)
                    {
                        o(queue.Dequeue());
                    }
                }
            };
        }

        private static void PushToQueue<T>(Queue<IMaybe<T>> queue, IMaybe<T> v, Observer<T> o)
        {
            lock (queue)
            {
                queue.Enqueue(v);
            }

            lock (queue)
            {
                if (queue.Count != 0)
                {
                    o(queue.Dequeue());
                }
            }
        }

        public static Observable<T> Merge<T>(params Observable<T>[] sources)
        {
            return o =>
            {
                foreach (var source in sources)
                {
                    source(o);
                }
            };
        }


        public static Observable<List<T>> ToList<T>(this Observable<T> source)
        {
            var result = new List<T>();
            return o => source(
                v =>
                    { 
                        if (v is Error<T>)
                        {
                            o(v.TransposeEmpty<List<T>>());
                        } 
                        else if (v is None<T>)
                        {
                            o(new Some<List<T>>(result));
                        }
                        else
                        {
                            result.Add(v.Value);
                        }
                    });
        }
    }
}