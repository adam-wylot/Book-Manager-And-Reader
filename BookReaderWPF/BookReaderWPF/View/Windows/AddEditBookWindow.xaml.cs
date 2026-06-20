using BookReader.ViewModel;
using System;
using System.Windows;

namespace BookReader.View;

public partial class BookEditWindow : Window
{
    private BookEditViewModel? ViewModel => DataContext as BookEditViewModel;

    public BookEditWindow()
    {
        InitializeComponent();

        this.DataContextChanged += BookEditWindow_DataContextChanged;
    }

    private void BookEditWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is BookEditViewModel oldVm)
        {
            oldVm.RequestClose -= ViewModel_RequestClose;
        }

        if (e.NewValue is BookEditViewModel newVm)
        {
            newVm.RequestClose += ViewModel_RequestClose;
        }
    }

    private void ViewModel_RequestClose(bool dialogResult)
    {
        DialogResult = dialogResult;
        Close();
    }
}