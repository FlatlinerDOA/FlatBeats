
namespace FlatBeats.ViewModels
{
    using System;

    public abstract class PageViewModel : PanelViewModel
    {
        private bool isInProgress;

        public bool IsInProgress
        {
            get
            {
                return this.isInProgress;
            }
            private set
            {
                if (this.isInProgress == value)
                {
                    return;
                }

                this.isInProgress = value;
                this.OnPropertyChanged("IsInProgress");
            }
        }


        protected void ShowProgress()
        {
            this.IsInProgress = true;
        }

        protected void HideProgress()
        {
            this.IsInProgress = false;
        }
    }
}
