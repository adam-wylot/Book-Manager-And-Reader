using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using BookReader.ViewModel;
using System.Windows.Navigation;

namespace BookReader.View;

public partial class ReaderWindow : Window
{
    public ReaderWindow()
    {
        InitializeComponent();
    }

    private void Browser_LoadCompleted(object sender, NavigationEventArgs e)
    {
        if (DataContext is ReaderViewModel vm)
        {
            if (e.Uri != null)
            {
                vm.UpdateChapterFromNavigation(e.Uri.ToString());
            }

            bool isAnchorNavigation = e.Uri != null && !string.IsNullOrEmpty(e.Uri.Fragment);
            if (vm.CurrentScrollPosition > 0 && !isAnchorNavigation)
            {
                try
                {
                    Browser.InvokeScript("eval", $"window.scrollTo(0, {vm.CurrentScrollPosition});");
                }
                catch { }
            }
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (DataContext is ReaderViewModel vm)
        {
            try
            {
                var scrollObj = Browser.InvokeScript("eval", "(document.documentElement.scrollTop || document.body.scrollTop || 0).toString()");
                if (scrollObj != null && int.TryParse(scrollObj.ToString(), out int scrollPos))
                {
                    vm.CurrentScrollPosition = scrollPos;
                }
            }
            catch { }

            if (vm.WindowClosingCommand.CanExecute(null))
            {
                vm.WindowClosingCommand.Execute(null);
            }
        }

        base.OnClosing(e);
    }
}

public static class WebBrowserBehavior
{
    public static readonly DependencyProperty BindableSourceProperty =
        DependencyProperty.RegisterAttached(
            "BindableSource",
            typeof(string),
            typeof(WebBrowserBehavior),
            new UIPropertyMetadata(null, BindableSourcePropertyChanged)
        );

    public static string GetBindableSource(DependencyObject obj) =>
        (string)obj.GetValue(BindableSourceProperty);
    public static void SetBindableSource(DependencyObject obj, string value) =>
        obj.SetValue(BindableSourceProperty, value);

    public static void BindableSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        if (o is WebBrowser browser && e.NewValue is string url && !string.IsNullOrEmpty(url))
        {
            browser.Navigate(new Uri(url));
        }
    }
}