using BookReader.Model;
using BookReader.View;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace BookReader.ViewModel;

internal class BookListViewModel : BaseViewModel
{
    private readonly BookRepository _repository;
    private readonly Action<Book> _onBookSelected;
    private readonly Action<string> _setStatusMessage;

    public ObservableCollection<Book> Books { get; }
    public ICollectionView FilteredBooks { get; }


    private Book? _selectedBook;
    public Book? SelectedBook
    {
        get => _selectedBook;
        set
        {
            if (SetProperty(ref _selectedBook, value))
            {
                OnPropertyChanged(nameof(SelectedBook));
                ((RelayCommand) DeleteBookCommand)?.RaiseCanExecuteChanged();
                ((RelayCommand) EditBookCommand)?.RaiseCanExecuteChanged();
                ((RelayCommand) OpenDetailsCommand)?.RaiseCanExecuteChanged();
            }
        }
    }


    // Właściwości filtrów
    private string _searchQuery = "";
    public string SearchQuery
    {
        get => _searchQuery;
        set { if (SetProperty(ref _searchQuery, value)) ApplyFilters(); }
    }

    private string _selectedTag = "All Tags";
    public string SelectedTag
    {
        get => _selectedTag;
        set { if (SetProperty(ref _selectedTag, value)) ApplyFilters(); }
    }

    private string _selectedStatus = "All";
    public string SelectedStatus
    {
        get => _selectedStatus;
        set { if (SetProperty(ref _selectedStatus, value)) ApplyFilters(); }
    }

    private string _selectedRating = "Any";
    public string SelectedRating
    {
        get => _selectedRating;
        set { if (SetProperty(ref _selectedRating, value)) ApplyFilters(); }
    }

    private string _selectedSort = "Title";
    public string SelectedSort
    {
        get => _selectedSort;
        set { if (SetProperty(ref _selectedSort, value)) ApplySort(); }
    }


    // Comboboxy
    public ObservableCollection<string> AvailableTags { get; } = new();
    public ObservableCollection<string> Statuses { get; } = new() { "All", "Unread", "In Progress", "Read" };
    public ObservableCollection<string> Ratings { get; } = new() { "Any", "1+ Stars", "2+ Stars", "3+ Stars", "4+ Stars", "5 Stars" };
    public ObservableCollection<string> SortOptions { get; } = new() { "Title", "Author", "Rating" };


    public Visibility ClearFiltersVisibility =>
        (!string.IsNullOrEmpty(SearchQuery) || SelectedStatus != "All" || SelectedRating != "Any" || SelectedTag != "All Tags" || SelectedSort != "Title")
        ? Visibility.Visible : Visibility.Collapsed;


    // Komendy
    public ICommand AddBookCommand { get; }
    public ICommand EditBookCommand { get; }
    public ICommand DeleteBookCommand { get; }
    public ICommand OpenDetailsCommand { get; }
    public ICommand ClearFiltersCommand { get; }


    // Konstruktor
    public BookListViewModel(BookRepository repository, ObservableCollection<Book> sharedBooks, Action<Book> onBookSelected, Action<string> setStatusMessage)
    {
        _repository = repository;
        _onBookSelected = onBookSelected;
        _setStatusMessage = setStatusMessage;
        Books = sharedBooks;

        FilteredBooks = CollectionViewSource.GetDefaultView(Books);
        FilteredBooks.Filter = FilterLogic;
        UpdateAvailableTags();
        ApplySort();

        AddBookCommand = new RelayCommand(ExecuteAddBook);
        EditBookCommand = new RelayCommand(ExecuteEditBook, CanExecuteEditBook);
        DeleteBookCommand = new RelayCommand(ExecuteDeleteBook, CanExecuteDeleteBook);
        OpenDetailsCommand = new RelayCommand(ExecuteOpenDetails, CanExecuteOpenDetails);
        ClearFiltersCommand = new RelayCommand(ExecuteClearFilters);
    }


    // Metody
    private void ExecuteAddBook()
    {
        var newBook = new Book();
        var editVm = new BookEditViewModel(newBook, isNewBook: true);
        var editWindow = new BookEditWindow { DataContext = editVm };

        if (editWindow.ShowDialog() == true)
        {
            // Dodanie ksiązki do repo i listy w widoku
            _repository.AddBook(newBook);
            Books.Add(newBook);
            UpdateAvailableTags();

            _setStatusMessage("Book added.");
        }
    }

private void ExecuteEditBook()
    {
        if (SelectedBook == null)
        {
            return;
        }

        var editVm = new BookEditViewModel(SelectedBook, isNewBook: false);
        var editWindow = new BookEditWindow { DataContext = editVm };
        if (editWindow.ShowDialog() == true)
        {
            _repository.UpdateBook(SelectedBook);
            
            var existingBook = Books.FirstOrDefault(b => b.Id == SelectedBook.Id);
            if (existingBook != null)
            {
                int index = Books.IndexOf(existingBook);
                Books.RemoveAt(index);
                Books.Insert(index, SelectedBook);
                SelectedBook = Books[index];
            }
            
            UpdateAvailableTags();
            ApplyFilters();
            _setStatusMessage("Book updated.");
        }
    }

    private bool CanExecuteEditBook() => SelectedBook != null;

    private void ExecuteDeleteBook()
    {
        if (SelectedBook != null)
        {
            _repository.RemoveBook(SelectedBook);

            var bookToRemove = Books.FirstOrDefault(b => b.Id == SelectedBook.Id);
            if (bookToRemove != null)
            {
                Books.Remove(bookToRemove);
            }

            UpdateAvailableTags();
            _setStatusMessage("Book deleted.");
        }
    }

    private bool CanExecuteDeleteBook() => SelectedBook != null;

    private void ExecuteOpenDetails()
    {
        if (SelectedBook != null)
        {
            _onBookSelected(SelectedBook);
        }
    }

    private bool CanExecuteOpenDetails() => SelectedBook != null;



    /// Filtrowanie
    private void ApplyFilters()
    {
        FilteredBooks.Refresh();
        OnPropertyChanged(nameof(ClearFiltersVisibility));
    }

    private void ApplySort()
    {
        FilteredBooks.SortDescriptions.Clear();

        switch (SelectedSort)
        {
            case "Title":
            {
                FilteredBooks.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
                break;
            }

            case "Author":
            {
                FilteredBooks.SortDescriptions.Add(new SortDescription("Author", ListSortDirection.Ascending));
                break;
            }

            case "Rating":
            {
                FilteredBooks.SortDescriptions.Add(new SortDescription("Rating", ListSortDirection.Descending));
                break;
            }
        }

        ApplyFilters();
    }

    private bool FilterLogic(object item)
    {
        if (item is not Book book) return false;

        // Wyszukiwanie
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            bool matchTitle = book.Title?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) == true;
            bool matchAuthor = book.Author?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) == true;
            if (!matchTitle && !matchAuthor) return false;
        }

        // Filtrowanie tagów
        if (SelectedTag != "All Tags")
        {
            if (book.Tags == null || !book.Tags.Any(t => string.Equals(t, SelectedTag, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        // Filtrowanie statusu
        if (SelectedStatus != "All")
        {
            bool isUnread = book.CurrentPage == 0;
            bool isRead = book.TotalPages > 0 && book.CurrentPage >= book.TotalPages;
            bool isInProgress = book.CurrentPage > 0 && !isRead;

            if (SelectedStatus == "Unread" && !isUnread) return false;
            if (SelectedStatus == "In Progress" && !isInProgress) return false;
            if (SelectedStatus == "Read" && !isRead) return false;
        }

        // Filtrowanie oceny
        if (SelectedRating != "Any")
        {
            int minRating = int.Parse(SelectedRating.Substring(0, 1));
            if (book.Rating < minRating) return false;
        }

        return true;
    }

    private void ExecuteClearFilters()
    {
        SearchQuery = "";
        SelectedTag = "All Tags";
        SelectedStatus = "All";
        SelectedRating = "Any";
        SelectedSort = "Title";
    }

    private void UpdateAvailableTags()
    {
        var currentTag = SelectedTag;
        AvailableTags.Clear();
        AvailableTags.Add("All Tags");

        var tags = Books.Where(b => b.Tags != null)
                        .SelectMany(b => b.Tags)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(t => t);

        foreach (var tag in tags)
        {
            AvailableTags.Add(tag);
        }

        SelectedTag = AvailableTags.Contains(currentTag) ? currentTag : "All Tags";
    }
}