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

        public static IObservable<T> FlowIn<T>(this IObservable<T> source, int millisecondDelay = 85)
        {
            //bool hasBeenRun = false;
            return from item in source
                   from delayTimer in Observable.Timer(TimeSpan.FromMilliseconds(millisecondDelay))
                   select item;
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

        ////public static IObservable<TData> AddOrReloadListItems<TViewModel, TData>(this IObservable<TData> dataItems, IList<TViewModel> viewModels, Action<TViewModel, TData> load) where TViewModel : ListItemViewModel, new()
        ////{
        ////    int removeAfter = 0;
        ////    TViewModel previous = null;
        ////    return dataItems.Indexed().Do(
        ////        dataItem =>
        ////        {
        ////            var existing = dataItem.Index < viewModels.Count ? viewModels[dataItem.Index] : null;
        ////            if (existing == null)
        ////            {
        ////                existing = new TViewModel();
        ////                load(existing, dataItem.Item);
        ////                viewModels.Add(existing);
        ////            }
        ////            else
        ////            {
        ////                load(existing, dataItem.Item);
        ////            }

        ////            existing.IsFirstItem = dataItem.Index == 0;
        ////            if (previous != null)
        ////            {
        ////                previous.IsLastItem = false;
        ////            }

        ////            previous = existing;
        ////            removeAfter = Math.Max(removeAfter, dataItem.Index);
        ////        }).Finally(() =>
        ////        {
        ////            while (viewModels.Count > removeAfter + 1)
        ////            {
        ////                viewModels.RemoveAt(removeAfter + 1);
        ////            }

        ////            if (viewModels.Count != 0)
        ////            {
        ////                viewModels.Last().IsLastItem = true;
        ////            }
        ////        }).Select(t => t.Item);
        ////}

        public static IObservable<IList<TData>> AddOrReloadPage<TViewModel, TData>(this IObservable<Page<TData>> pages, IList<TViewModel> target, Action<TViewModel, TData> load) where TViewModel : ListItemViewModel, new() 
        {
            return pages.Do(
                page =>
                    {
                       
                        var startIndex = (page.PageNumber - 1) * page.PageSize;
                        var endIndex = startIndex + page.Items.Count;
                        for (int i = 0; i < page.Items.Count; i++)
                        {
                            var targetIndex = startIndex + i;
                            if (target.Count <= targetIndex)
                            {
                                target.Add(new TViewModel());
                                targetIndex = target.Count - 1;
                            }

                            var vm = target[targetIndex];
                            load(vm, page.Items[i]);
                        }

                        if (page.Items.Count < page.PageSize)
                        {
                            while (target.Count > endIndex)
                            {
                                target.RemoveAt(target.Count - 1);
                            }
                        }

                        UpdateFirstAndLastItems(target);
                        
                    }).Select(t => t.Items);
        }

        public static IObservable<TData> AddOrReloadByPosition<TViewModel, TData>(this IObservable<TData> dataItems, IList<TViewModel> viewModels, Action<TViewModel, TData> load) where TViewModel : ListItemViewModel, new()
        {
            int removeAfter = -1;
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

                        UpdateFirstAndLastItems(viewModels);
                    }).Select(t => t.Item);
        }

        private static void UpdateFirstAndLastItems<TViewModel>(IList<TViewModel> viewModels) where TViewModel : ListItemViewModel
        {
            for (int i = 0; i < viewModels.Count; i++)
            {
                var vm = viewModels[i];
                vm.IsFirstItem = i == 0;
                vm.IsLastItem = i == viewModels.Count - 1;
            }
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
}
