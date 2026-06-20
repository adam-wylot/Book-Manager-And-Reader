using BookReader.Model;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BookReader.ViewModel;

internal class MainViewModel : BaseViewModel
{
    private readonly BookRepository _repository;
    public ObservableCollection<Book> Books { get; } = new();


    private BaseViewModel? _currentPage;
    public BaseViewModel? CurrentPage
    {
        get => _currentPage;
        set
        {
            if (SetProperty(ref _currentPage, value))
            {
                OnPropertyChanged(nameof(IsRepositoryOpen));
            }
        }
    }

    public bool IsRepositoryOpen => CurrentPage != null && CurrentPage is not WelcomeViewModel;


    private string _message = "";
    public string Message { get => _message; set => SetProperty(ref _message, value); }


    // Statystyki paska na dole
    public int TotalBooks => Books.Count;

    private int _readBooks;
    public int ReadBooks { get => _readBooks; set => SetProperty(ref _readBooks, value); }

    private int _inProgressBooks;
    public int InProgressBooks { get => _inProgressBooks; set => SetProperty(ref _inProgressBooks, value); }

    private string _averageRating = "—";
    public string AverageRating { get => _averageRating; set => SetProperty(ref _averageRating, value); }

    private string _mostPopularGenre = "—";
    public string MostPopularGenre { get => _mostPopularGenre; set => SetProperty(ref _mostPopularGenre, value); }


    // Komendy
    public ICommand CreateRepositoryCommand { get; }
    public ICommand OpenRepositoryCommand { get; }


    // Konstruktor
    public MainViewModel()
    {
        _repository = new BookRepository();

        CreateRepositoryCommand = new RelayCommand(ExecuteCreateRepository);
        OpenRepositoryCommand = new RelayCommand(ExecuteOpenRepository);

        Books.CollectionChanged += (s, e) => UpdateStatistics();

        GoToWelcome(); // ekran startowy
    }



    // Metody
    private void UpdateStatistics()
    {
        OnPropertyChanged(nameof(TotalBooks));

        int read = 0;
        int inProgress = 0;
        int ratingSum = 0;
        int ratingCount = 0;
        var tagCounts = new Dictionary<string, int>();

        foreach (var book in Books)
        {
            if (book.TotalPages > 0)
            {
                if (book.CurrentPage >= book.TotalPages)
                {
                    read++;
                }
                else if (book.CurrentPage > 0)
                {
                    inProgress++;
                }
            }

            if (book.Rating > 0)
            {
                ratingSum += book.Rating;
                ratingCount++;
            }

            if (book.Tags != null)
            {
                foreach (var tag in book.Tags)
                {
                    if (tagCounts.ContainsKey(tag))
                    {
                        tagCounts[tag]++;
                    }
                    else
                    {
                        tagCounts[tag] = 1;
                    }
                }
            }
        }

        ReadBooks = read;
        InProgressBooks = inProgress;
        AverageRating = ratingCount > 0 ? $"{(double)ratingSum / ratingCount:F1} ★" : "—";
        MostPopularGenre = tagCounts.Count > 0 ? tagCounts.OrderByDescending(t => t.Value).First().Key : "—";
    }

    private void SetStatusMessage(string msg)
    {
        Message = msg;
        UpdateStatistics();
    }


    /// Nawigacja
    private void GoToWelcome() =>
        CurrentPage = new WelcomeViewModel(CreateRepositoryCommand, OpenRepositoryCommand);

    private void GoToBookList() =>
        CurrentPage = new BookListViewModel(_repository, Books, GoToBookDetail, SetStatusMessage);

    private void GoToBookDetail(Book book) =>
        CurrentPage = new BookDetailViewModel(book, GoToBookList, _repository, Books, SetStatusMessage);


    /// Repozytorium
    private void ExecuteCreateRepository()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Libraria Repository (*.librepo)|*.librepo",
            DefaultExt = ".librepo",
            Title = "Create New Repository",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (dialog.ShowDialog() == true)
        {
            _repository.Initialize(dialog.FileName);
            Books.Clear();
            GoToBookList();
            Message = "New repository created.";
        }
    }

    private void ExecuteOpenRepository()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Libraria Repository (*.librepo)|*.librepo",
            DefaultExt = ".librepo",
            Title = "Open Repository",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (dialog.ShowDialog() == true)
        {
            if (_repository.Load(dialog.FileName))
            {
                Books.Clear();
                foreach (var book in _repository.GetAllBooks())
                {
                    Books.Add(book);
                }

                GoToBookList();
                Message = "Repository loaded.";
            }
            else
            {
                GoToWelcome();
                Message = "Error: failed to load repository.";
            }
        }
    }
}