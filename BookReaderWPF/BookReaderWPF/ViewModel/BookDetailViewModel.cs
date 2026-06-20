using BookReader.Model;
using BookReader.View;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace BookReader.ViewModel;

internal class BookDetailViewModel : BaseViewModel
{
    private readonly BookRepository _repository;
    private readonly ObservableCollection<Book> _sharedBooks;
    private readonly Action _goBackAction;
    private readonly Action<string> _setStatusMessage;


    public Book Book { get; }
    public ObservableCollection<string> DisplayTags { get; }


    // Komendy
    public ICommand GoBackCommand { get; }
    public ICommand EditBookCommand { get; }
    public ICommand AddBookCommand { get; }
    public ICommand DeleteBookCommand { get; }
    public ICommand ReadFromStartCommand { get; }
    public ICommand ContinueReadingCommand { get; }


    // Widoczność elementów
    public Visibility DescriptionVisibility =>
            string.IsNullOrWhiteSpace(Book.Description) ? Visibility.Collapsed : Visibility.Visible;

    public Visibility TagsVisibility =>
        (Book.Tags == null || Book.Tags.Count == 0) ? Visibility.Collapsed : Visibility.Visible;

    public Visibility ReadingProgressVisibility =>
        Book.TotalPages > 0 ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ReadFromStartVisibility =>
        !string.IsNullOrWhiteSpace(Book.ContentFilePath) ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ContinueReadingVisibility =>
        (!string.IsNullOrWhiteSpace(Book.ContentFilePath) && Book.CurrentPage > 1) ? Visibility.Visible : Visibility.Collapsed;


    // Konstruktor
    public BookDetailViewModel(Book book, Action goBackAction, BookRepository repository, ObservableCollection<Book> sharedBooks, Action<string> setStatusMessage)
    {
        Book = book;
        _repository = repository;
        _goBackAction = goBackAction;
        _sharedBooks = sharedBooks;
        _setStatusMessage = setStatusMessage;
        DisplayTags = new ObservableCollection<string>(Book.Tags ?? []);

        GoBackCommand = new RelayCommand(goBackAction);
        EditBookCommand = new RelayCommand(ExecuteEditBook);
        AddBookCommand = new RelayCommand(() => { }, () => false);
        DeleteBookCommand = new RelayCommand(ExecuteDeleteBook);
        ReadFromStartCommand = new RelayCommand(ExecuteReadFromStart);
        ContinueReadingCommand = new RelayCommand(ExecuteContinueReading);
    }


    // Metody
    private void ExecuteEditBook()
    {
        var editVm = new BookEditViewModel(Book, isNewBook: false);
        var editWindow = new BookEditWindow { DataContext = editVm };

        if (editWindow.ShowDialog() == true)
        {
            _repository.UpdateBook(Book);

            DisplayTags.Clear();
            foreach (var tag in Book.Tags)
            {
                DisplayTags.Add(tag);
            }

            _setStatusMessage("Book updated.");

            OnPropertyChanged(nameof(Book));
            OnPropertyChanged(nameof(DescriptionVisibility));
            OnPropertyChanged(nameof(TagsVisibility));
            OnPropertyChanged(nameof(ReadFromStartVisibility));
            OnPropertyChanged(nameof(ContinueReadingVisibility));
        }
    }

    private void ExecuteDeleteBook()
    {
        _repository.RemoveBook(Book);
        _sharedBooks.Remove(Book);
        _setStatusMessage("Book deleted.");
        _goBackAction();
    }


    /// Reader
    private void ExecuteReadFromStart()
    {
        Book.CurrentPage = 1;
        Book.ScrollPosition = 0;
        _repository.UpdateBook(Book);

        OpenReader(1);
    }

    private void ExecuteContinueReading()
    {
        OpenReader(Book.CurrentPage > 0 ? Book.CurrentPage : 1);
    }

    private void OpenReader(int page)
    {
        if (!File.Exists(Book.ContentFilePath))
        {
            MessageBox.Show(
                "The assigned EPUB file could not be found. It may have been moved or deleted.",
                "File Not Found",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );

            return;
        }

        var readerVm = new ReaderViewModel(Book, _repository, page);
        var readerWindow = new ReaderWindow { DataContext = readerVm };
        readerWindow.ShowDialog();

        OnPropertyChanged(nameof(Book));
        OnPropertyChanged(nameof(ContinueReadingVisibility));
        _setStatusMessage("Reading progress saved.");
    }
}