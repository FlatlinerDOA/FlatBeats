//--------------------------------------------------------------------------------------------------
// <copyright file="ObservableExtensions.cs" company="DNS Technology Pty Ltd.">
//   Copyright (c) 2011 DNS Technology Pty Ltd. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Windows.Threading;

    using Microsoft.Phone.Reactive;

    public static class ObservableExtensions
    {
        public static IObservable<T> FlowInTime<T>(this IObservable<T> source)
        {
            return from item in source
                   from interval in Observable.Interval(TimeSpan.FromMilliseconds(150)).Take(1)
                   select item;
        }

        public static IObservable<T> SetListItemPositions<T>(this IObservable<T> items) where T : ListItemViewModel
        {
            bool isFirst = true;
            bool setLast = false;
            T last = default(T);
            return items.Do(
                item =>
                {
                    item.IsFirstItem = isFirst;
                    item.IsLastItem = false;
                    isFirst = false;
                    setLast = true;
                    last = item;
                }, 
                () =>
                {
                    if (setLast)
                    {
                        last.IsLastItem = true;
                    }
                });
        }

        public static IEnumerable<T> SetListItemPositions<T>(this IEnumerable<T> items) where T : ListItemViewModel
        {
            bool isFirst = true;
            bool setLast = false;
            T last = default(T);
            foreach (var item in items)
            {
                item.IsFirstItem = isFirst;
                item.IsLastItem = false;
                isFirst = false;
                setLast = true;
                last = item;
                yield return item;
            }
        
            if (setLast)
            {
                last.IsLastItem = true;
            }
        }

        public static IObservable<Indexed<T>> Indexed<T>(this IObservable<T> items)
        {
            int index = 0;
            return items.Select(
                t =>
                    {
                        var result = new Indexed<T>(t, index);
                        index++;
                        return result;
                    });
        }

        public static IEnumerable<Indexed<T>> Indexed<T>(this IEnumerable<T> items)
        {
            int index = 0;
            return items.Select(
                t =>
                {
                    var result = new Indexed<T>(t, index);
                    index++;
                    return result;
                });
        }

        public static IObservable<T> FlowIn<T>(this IObservable<T> source, int delay = 75)
        {
            bool hasBeenRun = false;
            return source.Do(
                _ =>
                    {
                        if (!hasBeenRun)
                        {
                            hasBeenRun = true;
                        }
                        else
                        {
                            Thread.Sleep(delay);
                        }
                    });
        }

        public static IObservable<T> ContinueWhile<T>(this IObservable<T> sequence, Predicate<T> predicate)
        {
            return sequence.ContinueWhile(predicate, null);
        }

        public static IObservable<T> ContinueWhile<T>(this IObservable<T> sequence, Predicate<T> predicate, Action onExit)
        {
            return Observable.CreateWithDisposable<T>(
                observer =>
                    {
                        bool isRunning = true;
                        return sequence.Subscribe(item =>
                        {
                            if (!isRunning)
                            {
                                return;
                            }

                            isRunning = predicate(item);
                            observer.OnNext(item);
                            if (!isRunning)
                            {
                                observer.OnCompleted();
                                if (onExit != null)
                                {
                                    onExit();
                                }
                            }
                        }, 
                        observer.OnError, 
                        observer.OnCompleted);
                    });

        }

        public static IObservable<T> FirstDo<T>(this IObservable<T> sequence, Action<T> firstAction)
        {
            bool hasBeenRun = false;
            return Observable.CreateWithDisposable<T>(
                observer => sequence.Subscribe(
                    _ =>
                    {
                        if (!hasBeenRun)
                        {
                            hasBeenRun = true;
                            firstAction(_);
                        }

                        observer.OnNext(_);
                    }, 
                    observer.OnError, 
                    observer.OnCompleted));
        }

        public static IObservable<TData> AddOrReloadListItems<TViewModel, TData>(this IObservable<TData> dataItems, IList<TViewModel> viewModels, Action<TViewModel, TData> load) where TViewModel : ListItemViewModel, new()
        {
            int removeAfter = 0;
            TViewModel previous = null;
            return dataItems.Indexed().Do(
                dataItem =>
                {
                    var existing = dataItem.Index < viewModels.Count ? viewModels[dataItem.Index] : null;
                    if (existing == null)
                    {
                        existing = new TViewModel();
                        load(existing, dataItem.Item);
                        viewModels.Add(existing);
                    }
                    else
                    {
                        load(existing, dataItem.Item);
                    }

                    existing.IsFirstItem = dataItem.Index == 0;
                    if (previous != null)
                    {
                        previous.IsLastItem = false;
                    }

                    previous = existing;
                    removeAfter = Math.Max(removeAfter, dataItem.Index);
                }).Finally(() =>
                {
                    while (viewModels.Count > removeAfter + 1)
                    {
                        viewModels.RemoveAt(removeAfter + 1);
                    }

                    if (viewModels.Count != 0)
                    {
                        viewModels.Last().IsLastItem = true;
                    }
                }).Select(t => t.Item);
        }

        public static IObservable<TData> AddOrReloadByPosition<TViewModel, TData>(this IObservable<TData> dataItems, IList<TViewModel> viewModels, Action<TViewModel, TData> load) where TViewModel : class, new()
        {
            int removeAfter = 0;
            return dataItems.Indexed().Do(
                dataItem =>
                {
                    var existing = dataItem.Index < viewModels.Count ? viewModels[dataItem.Index] : null;
                    if (existing == null)
                    {
                        existing = new TViewModel();
                        load(existing, dataItem.Item);
                        viewModels.Add(existing);
                    }
                    else
                    {
                        load(existing, dataItem.Item);
                    }

                    removeAfter = Math.Max(removeAfter, dataItem.Index);
                }).Finally(() =>
                    {
                        while (viewModels.Count > removeAfter + 1)
                        {
                            viewModels.RemoveAt(removeAfter + 1);
                        }
                    }).Select(t => t.Item);
        }


        public static IObservable<TData> AddOrReload<TViewModel, TData>(this IObservable<TData> dataItems, IList<TViewModel> viewModels, Func<TViewModel, TData, bool> match, Action<TViewModel, TData> load) where TViewModel : new()
        {
            return dataItems.Do(
                dataItem =>
                    {
                        var existing = viewModels.FirstOrDefault(vm => match(vm, dataItem));
                        if (existing == null)
                        {
                            existing = new TViewModel();
                            load(existing, dataItem);
                            viewModels.Add(existing);
                        }
                        else
                        {
                            load(existing, dataItem);
                        }
                    });
        }

        public static IObservable<TResult> FinallySelect<T, TResult>(this IObservable<T> sequence, Func<TResult> finalValue)
        {
            return Observable.CreateWithDisposable<TResult>(
                observer => sequence.Subscribe(
                    _ =>
                    {
                    },
                    observer.OnError,
                    () =>
                    { 
                        observer.OnNext(finalValue());
                        observer.OnCompleted();
                    }));
        }
    }

    public class Indexed<T>
    {
        /// <summary>
        /// Initializes a new instance of the Index class.
        /// </summary>
        public Indexed(T item, int index)
        {
            this.Item = item;
            this.Index = index;
        }

        public T Item { get; private set; }
        public int Index { get; private set; }
    }
}
