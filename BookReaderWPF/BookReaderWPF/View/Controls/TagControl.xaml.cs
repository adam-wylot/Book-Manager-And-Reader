using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BookReader.View;

public partial class TagControl : UserControl
{
    public static readonly DependencyProperty TagsProperty =
        DependencyProperty.Register(
            "Tags",
            typeof(IEnumerable),
            typeof(TagControl),
            new PropertyMetadata(null)
        );

    public IEnumerable Tags
    {
        get => (IEnumerable)GetValue(TagsProperty);
        set => SetValue(TagsProperty, value);
    }

    public static readonly DependencyProperty IsEditableProperty =
        DependencyProperty.Register(
            "IsEditable",
            typeof(bool),
            typeof(TagControl),
            new PropertyMetadata(false)
        );

    public bool IsEditable
    {
        get => (bool)GetValue(IsEditableProperty);
        set => SetValue(IsEditableProperty, value);
    }

    public static readonly DependencyProperty RemoveCommandProperty =
        DependencyProperty.Register(
            "RemoveCommand",
            typeof(ICommand),
            typeof(TagControl),
            new PropertyMetadata(null)
        );

    public ICommand RemoveCommand
    {
        get => (ICommand)GetValue(RemoveCommandProperty);
        set => SetValue(RemoveCommandProperty, value);
    }

    public static readonly DependencyProperty ChipBackgroundProperty =
        DependencyProperty.Register(
            "ChipBackground",
            typeof(Brush),
            typeof(TagControl),
            new PropertyMetadata(new SolidColorBrush(Colors.CadetBlue))
        );

    public Brush ChipBackground
    {
        get => (Brush)GetValue(ChipBackgroundProperty);
        set => SetValue(ChipBackgroundProperty, value);
    }

    public static readonly DependencyProperty ChipForegroundProperty =
        DependencyProperty.Register(
            "ChipForeground",
            typeof(Brush),
            typeof(TagControl),
            new PropertyMetadata(Brushes.White)
        );

    public Brush ChipForeground
    {
        get => (Brush)GetValue(ChipForegroundProperty);
        set => SetValue(ChipForegroundProperty, value);
    }

    public TagControl()
    {
        InitializeComponent();
    }
}