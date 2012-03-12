namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using FlatBeats.DataModel;
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
            for (int i = 0; i < 12; i++)
            {
                this.Mixes.Add(new MixViewModel());
            }

            this.Title = Framework.StringResources.Title_LatestMixes;
        }

        public ObservableCollection<MixViewModel> Mixes { get; private set; }

        public IObservable<IList<MixViewModel>> LoadAsync()
        {
            var pageData = MixesService.GetLatestMixesAsync()
                .Select(p => new Page<MixContract>(p.Mixes, 1, p.Mixes.Count)).ObserveOnDispatcher()
                .AddOrReloadPage(this.Mixes, (vm, d) => vm.Load(d));
            return pageData.FinallySelect(() => (IList<MixViewModel>)this.Mixes);
        }

        private double opacity;

        public double Opacity
        {
            get
            {
                return this.opacity;
            }

            set
            {
                if (this.opacity == value)
                {
                    return;
                }

                this.opacity = value;
                this.OnPropertyChanged(() => this.Opacity);
            }
        }

        public void Display()
        {
            this.Opacity = 1;
        }
    }
}
