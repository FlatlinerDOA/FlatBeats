namespace EightTracks.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Collections;

    /// <summary>
  /// A lightweight list control that raises a Navigation event when items are clicked
  /// </summary>
  public class NavigationList : Control
  {
    #region fields

    private bool _manipulationDeltaStarted;

    #endregion

    #region ItemsSource dependency property

    /// <summary>
    /// Gets / sets the ItemsSource of the NavigationList. This is a dependency property.
    /// </summary>
    public IEnumerable ItemsSource
    {
      get { return (IEnumerable)GetValue(ItemsSourceProperty); }
      set { SetValue(ItemsSourceProperty, value); }
    }

    /// <summary>
    /// Identifies the ItemsSource dependency property.
    /// </summary>
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register("ItemsSource", typeof(IEnumerable),
        typeof(NavigationList), new PropertyMetadata(null));

    #endregion

    #region ItemTemplate dependency property

    /// <summary>
    /// Gets / sets the template used to render the list content.
    /// This is a dependency property.
    /// </summary>
    public DataTemplate ItemTemplate
    {
      get { return (DataTemplate)GetValue(ItemTemplateProperty); }
      set { SetValue(ItemTemplateProperty, value); }
    }

    /// <summary>
    /// Identifies the ItemTemplate dependency property.
    /// </summary>
    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register("ItemTemplate", typeof(DataTemplate),
        typeof(NavigationList), new PropertyMetadata(null));

    #endregion

    public NavigationList()
    {
      DefaultStyleKey = typeof(NavigationList);
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      var itemsControl = GetTemplateChild("itemsControl") as ItemsControlEx;
      itemsControl.PrepareContainerForItem += ItemsControl_PrepareContainerForItem;
    }

    private void ItemsControl_PrepareContainerForItem(object sender, PrepareContainerForItemEventArgs e)
    {
      var element = e.Element as UIElement;

      // handle events on the elements added to the ItemsControl
      element.MouseLeftButtonUp += Element_MouseLeftButtonUp;
      element.ManipulationStarted += Element_ManipulationStarted;
      element.ManipulationDelta += Element_ManipulationDelta;
    }

    private void Element_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
      _manipulationDeltaStarted = true;
    }

    private void Element_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      _manipulationDeltaStarted = false;
    }

    private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      if (_manipulationDeltaStarted)
        return;

      // raises the Navigation event on mouse up, but only if a manipulation delta
      // has not started.            
      var element = sender as FrameworkElement;
      OnNavigation(new NavigationEventArgs(element.DataContext));
    }

    /// <summary>
    /// Occurs when the user clicks on an item in the list
    /// </summary>
    public event EventHandler<NavigationEventArgs> Navigation;
    
    /// <summary>
    /// Raises the Navigation event
    /// </summary>
    protected void OnNavigation(NavigationEventArgs args)
    {
      if (Navigation != null)
      {
        Navigation(this, args);
      }
    }
  }

  /// <summary>
  ///  Provides data for the Navigation event
  /// </summary>
  public class NavigationEventArgs : EventArgs
  {
    internal NavigationEventArgs(object item)
    {
      Item = item;
    }

    /// <summary>
    /// Gets the item that was navigated to
    /// </summary>
    public object Item { private set; get; }
  }
}
