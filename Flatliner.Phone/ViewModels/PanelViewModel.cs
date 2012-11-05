// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PanelViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Flatliner.Phone.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;

    using Microsoft.Phone.Reactive;

    /// <summary>
    /// </summary>
    public abstract class PanelViewModel : ViewModel
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        private readonly IDictionary<Type, Func<Exception, ErrorMessage>> errorHandlers = new Dictionary<Type, Func<Exception, ErrorMessage>>();

        /// <summary>
        /// </summary>
        private Subject<bool> isInProgressChanges;

        /// <summary>
        /// </summary>
        private string inProgressMessage;

        /// <summary>
        /// </summary>
        private bool isInProgress;

        /// <summary>
        /// </summary>
        private CompositeDisposable lifetime;

        /// <summary>
        /// </summary>
        private string message;

        /// <summary>
        /// </summary>
        private bool showMessage;

        /// <summary>
        /// </summary>
        private string title;

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public string InProgressMessage
        {
            get
            {
                return this.inProgressMessage;
            }

            private set
            {
                if (this.inProgressMessage == value)
                {
                    return;
                }

                this.inProgressMessage = value;
                this.OnPropertyChanged("InProgressMessage");
            }
        }

        /// <summary>
        /// </summary>
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
                this.IsInProgressChanges.OnNext(this.isInProgress);
            }
        }

        /// <summary>
        /// </summary>
        public Subject<bool> IsInProgressChanges
        {
            get
            {
                return this.isInProgressChanges = (this.isInProgressChanges ?? new Subject<bool>());
            }
        }

        /// <summary>
        /// </summary>
        public string Message
        {
            get
            {
                return this.message;
            }

            set
            {
                if (this.message == value)
                {
                    return;
                }

                this.message = value;
                this.OnPropertyChanged("Message");

                if (this.message == null)
                {
                    this.ShowMessage = false;
                }
            }
        }

        /// <summary>
        /// </summary>
        public bool ShowMessage
        {
            get
            {
                return this.showMessage;
            }

            set
            {
                if (this.showMessage == value)
                {
                    return;
                }

                this.showMessage = value;
                this.OnPropertyChanged("ShowMessage");
            }
        }

        /// <summary>
        /// </summary>
        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                if (this.title == value)
                {
                    return;
                }

                this.title = value;
                this.OnPropertyChanged("Title");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// </summary>
        public virtual void Unload()
        {
            this.HideProgress();

            if (this.isInProgressChanges != null)
            {
                this.isInProgressChanges.OnCompleted();
                this.isInProgressChanges = null;
            }

            if (this.lifetime == null)
            {
                return;
            }

            this.lifetime.Dispose();
            this.lifetime = null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="disposable">
        /// </param>
        protected void AddToLifetime(IDisposable disposable)
        {
            if (this.lifetime == null)
            {
                this.lifetime = new CompositeDisposable();
            }

            this.lifetime.Add(disposable);
        }

        /// <summary>
        /// </summary>
        /// <param name="exception">
        /// </param>
        protected virtual void HandleError(Exception exception)
        {
            this.HideProgress();
            this.ShowErrorMessage(null);

            if (exception == null)
            {
                return;
            }

            try
            {
                var errorType = exception.GetType();
                if (this.errorHandlers.ContainsKey(errorType))
                {
                    var response = this.UpdateErrorMessage(exception, errorType);
                    if (response == null || !response.IsCritical)
                    {
                        return;
                    }
                }

                var baseType = this.errorHandlers.Keys.FirstOrDefault(t => t.IsAssignableFrom(errorType));
                if (baseType != null)
                {
                    var response = this.UpdateErrorMessage(exception, baseType);
                    if (response == null || !response.IsCritical)
                    {
                        return;
                    }
                }

                LittleWatson.ReportException(exception, this.GetType().FullName);
            }
            catch (Exception unknownException)
            {
                LittleWatson.ReportException(unknownException, this.GetType().FullName);
            }
        }

        /// <summary>
        /// Can be called from any thread
        /// </summary>
        protected void HideProgress()
        {
            if (!Deployment.Current.Dispatcher.CheckAccess())
            {
                Deployment.Current.Dispatcher.BeginInvoke(this.HideProgress);
                return;
            }

            this.IsInProgress = false;
        }

        /// <summary>
        /// </summary>
        /// <param name="handler">
        /// </param>
        /// <typeparam name="TException">
        /// </typeparam>
        protected void RegisterErrorHandler<TException>(Func<TException, ErrorMessage> handler)
            where TException : Exception
        {
            var errorType = typeof(TException);
            if (this.errorHandlers.ContainsKey(errorType))
            {
                this.errorHandlers.Remove(errorType);
            }

            this.errorHandlers.Add(errorType, ex => handler((TException)ex));
        }

        protected void ShowErrorMessage(ErrorMessage result)
        {
            if (!Deployment.Current.Dispatcher.CheckAccess())
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => this.ShowErrorMessageOverride(result));
                return;
            }
            
            this.ShowErrorMessageOverride(result);
        }

        /// <summary>
        /// Can be called from any thread.
        /// </summary>
        /// <param name="result">
        /// </param>
        protected virtual void ShowErrorMessageOverride(ErrorMessage result)
        {
            this.Message = result != null ? result.Message : null;
            this.ShowMessage = this.Message != null;
        }

        /// <summary>
        /// Can be called from any thread
        /// </summary>
        protected void ShowProgress(string message)
        {
            if (!Deployment.Current.Dispatcher.CheckAccess())
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => this.ShowProgress(message));
                return;
            }

            this.InProgressMessage = message;
            this.IsInProgress = true;
        }

        /// <summary>
        /// </summary>
        /// <param name="exception">
        /// </param>
        /// <param name="exceptionType">
        /// </param>
        /// <returns>
        /// </returns>
        protected ErrorMessage UpdateErrorMessage(Exception exception, Type exceptionType)
        {
            if (!this.errorHandlers.ContainsKey(exceptionType))
            {
                return null;
            }

            var result = this.errorHandlers[exceptionType](exception);
            if (result != null)
            {
                this.ShowErrorMessage(result);
                return result;
            }

            return null;
        }

        #endregion
    }
}