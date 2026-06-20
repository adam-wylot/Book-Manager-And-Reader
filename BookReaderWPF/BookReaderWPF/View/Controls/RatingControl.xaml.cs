using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BookReader.View;

public partial class RatingControl : UserControl
{
    private const int MaxStars = 5;
    private readonly TextBlock[] _stars;


    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            "Value",
            typeof(int),
            typeof(RatingControl),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged)
        );

    public int Value
    {
        get => (int) GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(
            "IsReadOnly",
            typeof(bool),
            typeof(RatingControl),
            new PropertyMetadata(true)
        );

    public bool IsReadOnly
    {
        get => (bool) GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly DependencyProperty StarSizeProperty =
        DependencyProperty.Register(
            "StarSize",
            typeof(double),
            typeof(RatingControl),
            new PropertyMetadata(24.0, OnStarSizeChanged)
        );

    public double StarSize
    {
        get => (double) GetValue(StarSizeProperty);
        set => SetValue(StarSizeProperty, value);
    }

    public RatingControl()
    {
        InitializeComponent();
        _stars = new TextBlock[MaxStars];

        for (int i = 0; i < MaxStars; i++)
        {
            var tb = new TextBlock
            {
                Text = "☆",
                FontSize = this.StarSize,
                Foreground = Brushes.Gold,
                Cursor = Cursors.Hand,
                Tag = i + 1,
                Margin = new Thickness(2, 0, 2, 0)
            };

            tb.MouseEnter += Star_MouseEnter;
            tb.MouseLeftButtonUp += Star_Click;

            _stars[i] = tb;
            StarsPanel.Children.Add(tb);
        }
        UpdateStars(Value);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((RatingControl)d).UpdateStars((int)e.NewValue);
    }

    private static void OnStarSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (RatingControl)d;
        foreach (var star in control._stars)
        {
            star.FontSize = (double)e.NewValue;
        }
    }

    private void UpdateStars(int rating)
    {
        for (int i = 0; i < MaxStars; i++)
        {
            _stars[i].Text = i < rating ? "★" : "☆";
            _stars[i].Cursor = IsReadOnly ? Cursors.Arrow : Cursors.Hand;
        }
    }

    private void Star_MouseEnter(object sender, MouseEventArgs e)
    {
        if (IsReadOnly) return;

        if (sender is TextBlock hoveredStar && hoveredStar.Tag is int rating)
        {
            UpdateStars(rating); // podświetla najechaną i poprzednie
        }
    }

    private void StarsPanel_MouseLeave(object sender, MouseEventArgs e)
    {
        if (IsReadOnly) return;
        UpdateStars(Value); // wraca do prawdziwej oceny po zjechaniu kursorem
    }

    private void Star_Click(object sender, MouseButtonEventArgs e)
    {
        if (IsReadOnly) return;

        if (sender is TextBlock clickedStar && clickedStar.Tag is int rating)
        {
            Value = rating; // zapisuje ocenę
            UpdateStars(Value);
        }
    }
}