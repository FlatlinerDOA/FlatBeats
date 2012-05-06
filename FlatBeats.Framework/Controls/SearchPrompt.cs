// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SearchPrompt.cs" company="">
//   
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace FlatBeats.Framework.Controls
{
    using System;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;

    using Coding4Fun.Phone.Controls;

    /// <summary>
    /// </summary>
    public class SearchPrompt : UserPrompt
    {
        #region Constants and Fields

        /// <summary>
        /// </summary>
        public static readonly DependencyProperty IsSubmitOnEnterKeyProperty =
            DependencyProperty.Register(
                "IsSubmitOnEnterKey", 
                typeof(bool), 
                typeof(SearchPrompt), 
                new PropertyMetadata(true, OnIsSubmitOnEnterKeyPropertyChanged));

        /// <summary>
        /// </summary>
        protected TextBox InputBox;

        /// <summary>
        /// </summary>
        private const string InputBoxName = "inputBox";

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// </summary>
        public SearchPrompt()
        {
            this.DefaultStyleKey = typeof(SearchPrompt);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// </summary>
        public bool IsSubmitOnEnterKey
        {
            get
            {
                return (bool)this.GetValue(IsSubmitOnEnterKeyProperty);
            }

            set
            {
                this.SetValue(IsSubmitOnEnterKeyProperty, value);
            }
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.InputBox = this.GetTemplateChild(InputBoxName) as TextBox;

            if (this.InputBox != null)
            {
                // manually adding
                // GetBindingExpression doesn't seem to respect TemplateBinding
                // so TextBoxBinding's code doesn't fire
                var scope = new InputScope();
                scope.Names.Add(new InputScopeName() { NameValue = InputScopeNameValue.Search });
                this.InputBox.InputScope = scope;
                var binding = new Binding { Source = this.InputBox, Path = new PropertyPath("Text"), };

                this.SetBinding(ValueProperty, binding);

                this.HookUpEventForIsSubmitOnEnterKey();

                ThreadPool.QueueUserWorkItem(this.DelayInputSelect);
            }
        }

        #endregion

        // Using a DependencyProperty as the backing store for IsSubmitOnEnterKey.  This enables animation, styling, binding, etc...
        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="d">
        /// </param>
        /// <param name="e">
        /// </param>
        private static void OnIsSubmitOnEnterKeyPropertyChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var inputPrompt = d as SearchPrompt;

            if (inputPrompt != null)
            {
                inputPrompt.HookUpEventForIsSubmitOnEnterKey();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="value">
        /// </param>
        private void DelayInputSelect(object value)
        {
            Thread.Sleep(250);
            this.Dispatcher.BeginInvoke(
                () =>
                    {
                        this.InputBox.Focus();
                        this.InputBox.SelectAll();
                    });
        }

        /// <summary>
        /// </summary>
        private void HookUpEventForIsSubmitOnEnterKey()
        {
            this.InputBox = this.GetTemplateChild(InputBoxName) as TextBox;

            if (this.InputBox == null)
            {
                return;
            }

            this.InputBox.KeyDown -= this.InputBoxKeyDown;

            if (this.IsSubmitOnEnterKey)
            {
                this.InputBox.KeyDown += this.InputBoxKeyDown;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void InputBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.OnCompleted(
                    new PopUpEventArgs<string, PopUpResult> { Result = this.Value, PopUpResult = PopUpResult.Ok });
            }
        }

        #endregion
    }
}