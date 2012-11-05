namespace Flatliner.Functional
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    public static class Observe
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observe<T> Empty<T>()
        {
            return o => o(new None<T>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Observe<T> Return<T>(T value)
        {
            return o =>
                {
                    o(new Some<T>(value));
                    o(new None<T>());
                };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Observe<int> Range(int start, int count)
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="onNext"></param>
        /// <param name="onError"></param>
        public static void Subscribe<T>(this Observe<T> source, Action<T> onNext, Action<Exception> onError)
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="onNext"></param>
        /// <param name="onError"></param>
        /// <param name="onCompleted"></param>
        public static void Subscribe<T>(this Observe<T> source, Action<T> onNext, Action<Exception> onError, Action onCompleted)
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="error"></param>
        /// <returns></returns>
        public static Observe<T> Throw<T>(Exception error)
        {
            return o => o(new Error<T>(error));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOuter"></typeparam>
        /// <typeparam name="TInner"></typeparam>
        /// <param name="source"></param>
        /// <param name="innerSource"></param>
        /// <returns></returns>
        public static Observe<TInner> SelectMany<TOuter, TInner>(this Observe<TOuter> source, Func<TOuter, Observe<TInner>> innerSource)
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
        public static Observe<TResult> SelectMany<TOuter, TInner, TResult>(this Observe<TOuter> source, Func<TOuter, Observe<TInner>> innerSource, Func<TOuter, TInner, TResult> selector)
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static Observe<TResult> Select<TSource, TResult>(this Observe<TSource> source, Func<TSource, TResult> selector)
        {
            return o =>
                {
                    bool completed = false;
                    source(
                        x =>
                        {
                            if (completed)
                            {
                                return;
                            }

                            if (!x.HasValue)
                            {
                                completed = true;
                                o(x.TransposeEmpty<TResult>());
                            }
                            else
                            {
                                TResult result;
                                try
                                {
                                    result = selector(x.Value);
                                }
                                catch (Exception ex)
                                {
                                    completed = true;
                                    o(new Error<TResult>(ex));
                                    return;
                                }

                                o(new Some<TResult>(result));
                            }
                        });
                };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Observe<TSource> Do<TSource>(this Observe<TSource> source, Action<TSource> onNext)
        {
            return o =>
            {
                bool completed = false;
                source(
                    x =>
                    {
                        if (completed)
                        {
                            return;
                        }

                        if (!x.HasValue)
                        {
                            completed = true;
                            o(x);
                        }
                        else
                        {
                            try
                            {
                                onNext(x.Value);
                            }
                            catch (Exception ex)
                            {
                                completed = true;
                                o(new Error<TSource>(ex));
                                return;
                            }

                            o(x);
                        }
                    });
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static Observe<TResult> Select<TSource, TResult>(this Observe<TSource> source, Func<TSource, int, TResult> selector)
        {
            return o => 
            {
                bool completed = false;
                int counter = 0;
                source(
                    x =>
                    {
                        if (completed)
                        {
                            return;
                        }

                        if (!x.HasValue)
                        {
                            completed = true;
                            o(x.TransposeEmpty<TResult>());
                        }
                        else
                        {
                            TResult result;
                            try
                            {
                                result = selector(x.Value, counter);
                            }
                            catch (Exception ex)
                            {
                                completed = true;
                                o(new Error<TResult>(ex));
                                return;
                            }

                            o(new Some<TResult>(result));
                            counter++;
                        }
                    });
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Observe<T> Take<T>(this Observe<T> source, int count)
        {
            return o =>
                {
                    bool completed = false;
                    int counter = 0;
                    source(x =>
                    {
                        if (completed)
                        {
                            return;
                        }

                        if (!x.HasValue)
                        {
                            completed = true;
                            o(x);
                            return;
                        }

                        if (counter < count)
                        {
                            o(x);
                        }
                        else
                        {
                            completed = true;
                            o(new None<T>());
                            return;
                        }

                        counter++;
                    });
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TFinal"></typeparam>
        /// <param name="source"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Observe<T> TakeUntil<T, TFinal>(this Observe<T> source, Observe<TFinal> other)
        {
            return o =>
            {
                bool completed = false;
                other(v =>
                    {
                        if (completed)
                        {
                            return;
                        }

                        if (v.HasValue)
                        {
                            completed = true;
                            o(new None<T>());
                        }
                    });
                source(x =>
                {
                    if (completed)
                    {
                        return;
                    }

                    if (!x.HasValue)
                    {
                        completed = true;
                        o(x);
                        return;
                    }

                    o(x);
                });
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static Observe<TResult> TrySelect<TSource, TResult>(this Observe<TSource> source, Func<TSource, TResult> selector)
        {
            return o => source(x =>
            {
                if (!x.HasValue)
                {
                    o(x.TransposeEmpty<TResult>());
                }
                else
                {
                    TResult result;
                    try
                    {
                        result = selector(x.Value);
                    }
                    catch (Exception ex)
                    {
                        o(new Error<TResult>(ex));
                        return;
                    }

                    o(new Some<TResult>(result));
                }
            });
        }


    
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static Observe<T> Where<T>(this Observe<T> source, Func<T, bool> filter)
        {
            return source.SelectMany(t => filter(t) ? Return(t) : Empty<T>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Observe<T> DefaultIfEmpty<T>(this Observe<T> source)
        {
            return source.DefaultIfEmpty(default(T));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static Observe<T> DefaultIfEmpty<T>(this Observe<T> source, T defaultValue)
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="secondSource"></param>
        /// <returns></returns>
        public static Observe<T> Concat<T>(this Observe<T> source, Observe<T> secondSource)
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="secondSources"></param>
        /// <returns></returns>
        public static Observe<T> Merge<T>(this Observe<T> source, params Observe<T>[] secondSources)
        {
            return o => 
            { 
                source(o);
                foreach (var secondSource in secondSources)
                {
                    secondSource(o);
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sources"></param>
        /// <returns></returns>
        public static Observe<T> Merge<T>(params Observe<T>[] sources)
        {
            return o =>
            {
                foreach (var source in sources)
                {
                    source(o);
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Observe<List<T>> ToList<T>(this Observe<T> source)
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="completionOrError"></param>
        /// <returns></returns>
        public static Observe<T> Finally<T>(this Observe<T> source, Action completionOrError)
        {
            return o =>
                {
                    bool completed = false;
                    source(
                        result =>
                            {
                                if (completed)
                                {
                                    return;
                                }

                                o(result);

                                if (!result.HasValue)
                                {
                                    completed = true;
                                    completionOrError();
                                }
                            });
                };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="createResource"></param>
        /// <param name="useResource"></param>
        /// <returns></returns>
        public static Observe<T> Using<T, TResource>(Func<TResource> createResource, Func<TResource, Observe<T>> useResource) where TResource : IDisposable
        {
            return o =>
                {
                    var resource = createResource();
                    var usage = useResource(resource);
                    usage.Finally(resource.Dispose)(o);
                };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TException"></typeparam>
        /// <param name="source"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static Observe<T> Catch<T, TException>(this Observe<T> source, Func<TException, Observe<T>> handler) where TException : Exception
        {
            return o =>
                {
                    bool completed = false;
                    source(
                        v =>
                            {
                                if (completed)
                                {
                                    return;
                                }

                                if (v is Error<T>)
                                {
                                    var error = (Error<T>)v;
                                    if (error.Exception is TException)
                                    {
                                        completed = true;
                                        handler((TException)error.Exception)(o);
                                        return;
                                    }
                                } 
                                else if (!v.HasValue)
                                {
                                    completed = true;
                                }

                                o(v);
                            });
                };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static Func<Observe<T>> FromAsyncPattern<T>(Func<AsyncCallback, object, IAsyncResult> begin, Func<IAsyncResult, T> end)
        {
            if (begin == null)
            {
                throw new ArgumentNullException("begin");
            }

            if (end == null)
            {
                throw new ArgumentNullException("end");
            }

            return () => o => begin(
                asyncResult =>
                {
                    T result;
                    try
                    {
                        result = end(asyncResult);
                    }
                    catch (Exception ex)
                    {
                        o(new Error<T>(ex));
                        return;
                    }

                    o(new Some<T>(result));
                    o(new None<T>());
                }, 
                null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static Func<T1, Observe<T>> FromAsyncPattern<T1, T>(Func<T1, AsyncCallback, object, IAsyncResult> begin, Func<IAsyncResult, T> end)
        {
            if (begin == null)
            {
                throw new ArgumentNullException("begin");
            }

            if (end == null)
            {
                throw new ArgumentNullException("end");
            }

            return arg1 => o => begin(
                arg1,
                asyncResult =>
                {
                    T result;
                    try
                    {
                        result = end(asyncResult);
                    }
                    catch (Exception ex)
                    {
                        o(new Error<T>(ex));
                        return;
                    }

                    o(new Some<T>(result));
                    o(new None<T>());
                },
                null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static Func<T1, T2, Observe<T>> FromAsyncPattern<T1, T2, T>(Func<T1, T2, AsyncCallback, object, IAsyncResult> begin, Func<IAsyncResult, T> end)
        {
            if (begin == null)
            {
                throw new ArgumentNullException("begin");
            }

            if (end == null)
            {
                throw new ArgumentNullException("end");
            }

            return (arg1, arg2) => o => begin(
                arg1,
                arg2,
                asyncResult =>
                {
                    T result;
                    try
                    {
                        result = end(asyncResult);
                    }
                    catch (Exception ex)
                    {
                        o(new Error<T>(ex));
                        return;
                    }

                    o(new Some<T>(result));
                    o(new None<T>());
                },
                null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Observe<PortableUnit> DeferredStart(Action action)
        {
            return o =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    o(new Error<PortableUnit>(ex));
                    return;
                }

                o(new Some<PortableUnit>(new PortableUnit()));
                o(new None<PortableUnit>());
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Observe<T> DeferredStart<T>(Func<T> action)
        {
            return o =>
                {
                    T result;
                    try
                    {
                        result = action();
                    }
                    catch (Exception ex)
                    {
                        o(new Error<T>(ex));
                        return;
                    }

                    o(new Some<T>(result));
                    o(new None<T>());
                };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="scheduler"></param>
        /// <returns></returns>
        public static Observe<T> SubscribeOn<T>(this Observe<T> source, ScheduleAction scheduler)
        {
            return o =>
                {
                    IDisposable disposable = null;
                    disposable = scheduler(() => 
                        source.Finally(() =>
                        {
                            if (disposable != null)
                            {
                                disposable.Dispose();
                            }
                        })(o));
                };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="scheduler"></param>
        /// <returns></returns>
        public static Observe<T> ObserveOn<T>(this Observe<T> source, ScheduleAction scheduler)
        {
            // TODO: Verify this as I don't think it's correct.
            return o => source(value =>
                {
                    IDisposable disposable = null;
                    disposable = scheduler(() =>
                        { 
                            o(value);
                            if (disposable != null)
                            {
                                disposable.Dispose();
                            }
                        });
                });
        }
    }
}