﻿using System;
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
    using System.Collections.ObjectModel;

    using FlatBeats.DataModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    public class ReviewsPanelViewModel : PanelViewModel
    {
        /// <summary>
        /// Initializes a new instance of the ReviewsPanelViewModel class.
        /// </summary>
        public ReviewsPanelViewModel()
        {
            this.Reviews = new ObservableCollection<ReviewViewModel>();
            this.Title = StringResources.TItle_Reviews;
        }

        public IObservable<Unit> LoadAsync(string mixId)
        {
            this.MixId = mixId;
            return this.LoadCommentsAsync();
        }

        public string MixId { get; private set; }

        public ObservableCollection<ReviewViewModel> Reviews { get; private set; }
        
        /// <summary>
        /// </summary>
        private IObservable<Unit> LoadCommentsAsync()
        {
            var downloadComments = from response in MixesService.GetMixReviews(this.MixId)
                                   from review in response.Reviews.ToObservable()
                                   select new ReviewViewModel(review);
            return downloadComments.ObserveOnDispatcher().Do(
                r => this.Reviews.Add(r),
                this.ShowError, 
                () =>
                {
                    if (this.Reviews.Count == 0)
                    {
                        this.Message = StringResources.Message_NoReviews;
                        this.ShowMessage = true;
                    }
                    else
                    {
                        this.Message = null;
                    }
                }).FinallySelect(() => new Unit());
        }


    }
}
