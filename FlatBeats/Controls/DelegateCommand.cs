namespace FlatBeats.Controls
{
    using System;
    using System.Windows.Input;

    public class DelegateCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the DelegateCommand class.
        /// </summary>
        public DelegateCommand(Action execute)
        {
            ////this.Dispatcher = Deployment.Current.Dispatcher;
            this.ExecuteDelegate = execute;
        }

        /// <summary>
        /// Initializes a new instance of the DelegateCommand class.
        /// </summary>
        public DelegateCommand(Action execute, Func<bool> canExecute) : this(execute)
        {
            this.CanExecuteDelegate = canExecute;
        }

        ////protected Dispatcher Dispatcher
        ////{
        ////    get;
        ////    set;
        ////}

        public Func<bool> CanExecuteDelegate
        {
            get;
            set;
        }

        public Action ExecuteDelegate
        {
            get;
            set;
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null. </param>
        bool ICommand.CanExecute(object parameter)
        {
            return this.CanExecute();
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null. </param>
        void ICommand.Execute(object parameter)
        {
            this.Execute();
        }

        public bool CanExecute()
        {
            return this.CanExecuteDelegate == null || this.CanExecuteDelegate();
        }

        public void Execute()
        {
            if (!this.CanExecute())
            {
                return;
            }

            if (this.ExecuteDelegate == null)
            {
                return;
            }

            this.ExecuteDelegate();
            this.RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged()
        {
            ////if (!this.Dispatcher.CheckAccess())
            ////{
            ////    this.Dispatcher.BeginInvoke(this.OnCanExecuteChanged);
            ////}
            ////else
            ////{
                this.OnCanExecuteChanged();
            ////}
        }

        /// <summary>
        /// Prevents re-entrancy by other commands depending on this 
        /// one and vice versa in an endless loop
        /// </summary>
        private bool isRaisingCanExecuteChanged ;

        private void OnCanExecuteChanged()
        {
            if (this.isRaisingCanExecuteChanged)
            {
                return;
            }

            this.isRaisingCanExecuteChanged = true;
            EventHandler temp = this.CanExecuteChanged;
            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }

            this.isRaisingCanExecuteChanged = false;
        }

        ////public void AffectedBy(ICommand command)
        ////{
        ////    command.CanExecuteChanged += this.RaiseCanExecuteChanged;
        ////}

        ////public void AffectedBy<T>(INotifyPropertyChanged source, Expression<Func<T>> expression)
        ////{
        ////    string propertyName = ((MemberExpression)expression.Body).Member.Name;
        ////    Observable.FromEvent<PropertyChangedEventArgs>(source, "PropertyChanged").Where(
        ////        p => p.EventArgs.PropertyName == propertyName).Subscribe(_ => this.OnCanExecuteChanged());
        ////    source.PropertyChanged += this.RaiseCanExecuteChanged;
        ////}
        

        ////private void RaiseCanExecuteChanged(object sender, EventArgs e)
        ////{
        ////    this.RaiseCanExecuteChanged();
        ////}

        public event EventHandler CanExecuteChanged;
    }
}
