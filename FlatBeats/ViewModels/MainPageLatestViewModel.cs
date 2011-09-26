﻿//--------------------------------------------------------------------------------------------------
// <copyright file="MainPageLatestViewModel.cs" company="DNS Technology Pty Ltd.">
//   Copyright (c) 2011 DNS Technology Pty Ltd. All rights reserved.
// </copyright>
//--------------------------------------------------------------------------------------------------
namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using FlatBeats.DataModel.Services;

    using Microsoft.Phone.Reactive;

    public class MainPageLatestViewModel : PanelViewModel
    {
        /// <summary>
        /// Initializes a new instance of the MainPageLatestViewModel class.
        /// </summary>
        public MainPageLatestViewModel()
        {
            this.Mixes = new ObservableCollection<MixViewModel>();
            this.Title = StringResources.Title_LatestMixes;
        }

        public ObservableCollection<MixViewModel> Mixes { get; private set; }

        public IObservable<List<MixViewModel>> LoadAsync()
        {
            var pageData = from latest in MixesService.GetLatestMixes()
                           from mix in latest.Mixes.ToObservable(Scheduler.ThreadPool)
                           select new MixViewModel(mix);
            return pageData.FlowIn()
                .ObserveOnDispatcher()
                .FirstDo(_ => this.Mixes.Clear())
                .Do(m => this.Mixes.Add(m), this.ShowError).Aggregate(
                    new List<MixViewModel>(), 
                    (a, m) =>
                    {
                        a.Add(m);
                        return a;
                    });
        }
    }
}