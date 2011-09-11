using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FlatBeats.ViewModels
{
    using Microsoft.Phone.Reactive;

    public static class ObservableExtensions
    {
        public static IObservable<TResult> SelectFinally<T, TResult>(this IObservable<T> sequence, Func<TResult> finalValue)
        {
            return Observable.CreateWithDisposable<TResult>(
                observer =>
                    {
                        return sequence.Subscribe(
                            _ => { },
                            ex => 
                            { 
                                observer.OnNext(finalValue());
                                observer.OnCompleted();
                            },
                            () =>
                                { 
                                    observer.OnNext(finalValue());
                                    observer.OnCompleted();
                                });
                    });
        }
    }
}
