using BookReader.Model;
using BookReader.ViewModel;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BookReader.View;

public partial class MainWindow : Window
{
    private MainViewModel _data = new();

    public MainWindow()
    {
        InitializeComponent();

        this.DataContext = _data;
    }
}