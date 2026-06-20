using BookReader.Model;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace BookReader.ViewModel;

internal class BookEditViewModel : BaseViewModel
{
    private readonly Book _originalBook;
    public ObservableCollection<string> Tags { get; }


    public string WindowTitle { get; }
    public string SaveButtonText { get; }


    private string _title = "";
    public string Title
    {
        get => _title;
        set
        {
            if (SetProperty(ref _title, value))
            {
                ((RelayCommand) SaveCommand)?.RaiseCanExecuteChanged();
            }
        }
    }
    

    private string _author = "";
    public string Author { get => _author; set => SetProperty(ref _author, value); }


    private string _description = "";
    public string Description { get => _description; set => SetProperty(ref _description, value); }


    private string _coverBase64 = "";
    public string CoverBase64
    {
        get => _coverBase64;
        set
        {
            if (SetProperty(ref _coverBase64, value))
            {
                OnPropertyChanged(nameof(NoCoverVisibility));
                OnPropertyChanged(nameof(CoverVisibility));
            }
        }
    }

    public System.Windows.Visibility NoCoverVisibility =>
        string.IsNullOrEmpty(CoverBase64) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

    public System.Windows.Visibility CoverVisibility =>
        string.IsNullOrEmpty(CoverBase64) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;


    private string _contentFilePath = "";
    public string ContentFilePath { get => _contentFilePath; set => SetProperty(ref _contentFilePath, value); }


    private string _newTag = "";
    public string NewTag
    {
        get => _newTag;
        set
        {
            if (SetProperty(ref _newTag, value))
            {
                ((RelayCommand) AddTagCommand)?.RaiseCanExecuteChanged();
            }
        }
    }


    private int _rating;
    public int Rating
    {
        get => _rating;
        set => SetProperty(ref _rating, value);
    }


    // Komendy
    public ICommand BrowseCoverCommand { get; }
    public ICommand BrowseBookCommand { get; }
    public ICommand AddTagCommand { get; }
    public ICommand RemoveTagCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action<bool>? RequestClose;


    // Konstruktory
    public BookEditViewModel(Book book, bool isNewBook = false)
    {
        _originalBook = book;

        // Ustawienie tytułów w zależności od akcji
        WindowTitle = isNewBook ? "Add Book" : "Edit Book";
        SaveButtonText = isNewBook ? "Add Book" : "Save Changes";

        BrowseCoverCommand = new RelayCommand(ExecuteBrowseCover);
        BrowseBookCommand = new RelayCommand(ExecuteBrowseBook);
        AddTagCommand = new RelayCommand(ExecuteAddTag, CanExecuteAddTag);
        RemoveTagCommand = new RelayCommand(ExecuteRemoveTag);
        SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
        CancelCommand = new RelayCommand(ExecuteCancel);

        Title = book.Title ?? "";
        Author = book.Author ?? "";
        Description = book.Description ?? "";
        CoverBase64 = book.CoverBase64 ?? "";
        ContentFilePath = book.ContentFilePath ?? "";
        Tags = new ObservableCollection<string>(book.Tags ?? new System.Collections.Generic.List<string>());
        Rating = book.Rating;
    }


    // Metody
    private void ExecuteBrowseCover()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
        };

        string coverDirPath = @".\SampleBookData\covers";
        if (Directory.Exists(coverDirPath))
        {
            dialog.InitialDirectory = Path.GetFullPath(coverDirPath);
        }
        else
        {
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var bitmap = new BitmapImage();

                using (var stream = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.DecodePixelWidth = 400;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                }

                var encoder = new JpegBitmapEncoder { QualityLevel = 80 };
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                using (var outStream = new MemoryStream())
                {
                    encoder.Save(outStream);
                    byte[] resizedImageArray = outStream.ToArray();
                    CoverBase64 = Convert.ToBase64String(resizedImageArray);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load the cover.\n\nInfo: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ExecuteBrowseBook()
    {
        var dialog = new OpenFileDialog { Filter = "EPUB Book (*.epub)|*.epub" };
        if (dialog.ShowDialog() == true)
        {
            ContentFilePath = dialog.FileName;
        }
    }

    private void ExecuteAddTag()
    {
        var newTag = NewTag.Trim();
        if (!string.IsNullOrWhiteSpace(newTag) && !Tags.Any(t => string.Equals(t, newTag, StringComparison.OrdinalIgnoreCase)))
        {
            Tags.Add(newTag);
            NewTag = string.Empty;
        }
    }

    private bool CanExecuteAddTag() => !string.IsNullOrWhiteSpace(NewTag);

    private void ExecuteRemoveTag(object? parameter)
    {
        if (parameter is string tag)
        {
            Tags.Remove(tag);
        }
    }

    private void ExecuteSave()
    {
        _originalBook.Title = Title;
        _originalBook.Author = Author;
        _originalBook.Description = Description;
        _originalBook.CoverBase64 = CoverBase64;
        _originalBook.ContentFilePath = ContentFilePath;
        _originalBook.Tags = Tags.ToList();
        _originalBook.Rating = Rating;

        RequestClose?.Invoke(true);
    }

    private bool CanExecuteSave() => !string.IsNullOrWhiteSpace(Title);

    private void ExecuteCancel() => RequestClose?.Invoke(false);
}