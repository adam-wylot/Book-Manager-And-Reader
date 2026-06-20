using BookReader.Model;
using System.IO;
using System.IO.Compression;
using System.Windows.Input;
using System.Xml.Linq;

namespace BookReader.ViewModel;

internal class ReaderViewModel : BaseViewModel
{
    private readonly Book _book;
    private readonly BookRepository _repository;
    private List<string> _chapterPaths = new();
    private string _tempExtractPath = "";

    public string WindowTitle => $"Reader - {_book.Title}";


    private int _currentChapterIndex;
    public int CurrentChapterIndex
    {
        get => _currentChapterIndex;
        set
        {
            if (SetProperty(ref _currentChapterIndex, value))
            {
                OnPropertyChanged(nameof(ProgressText));
                OnPropertyChanged(nameof(CurrentChapterUrl));
                UpdateBookProgress();

                ((RelayCommand) NextCommand).RaiseCanExecuteChanged();
                ((RelayCommand) PreviousCommand).RaiseCanExecuteChanged();
            }
        }
    }
    

    public string ProgressText => $"Chapter {CurrentChapterIndex + 1} of {_chapterPaths.Count}";

    public string CurrentChapterUrl => _chapterPaths.Count > 0 && CurrentChapterIndex < _chapterPaths.Count
                                            ? _chapterPaths[CurrentChapterIndex]
                                            : "";


    public int CurrentScrollPosition
    {
        get => _book.ScrollPosition;
        set
        {
            if (_book.ScrollPosition != value)
            {
                _book.ScrollPosition = value;
                OnPropertyChanged();

                _repository.UpdateBook(_book);
            }
        }
    }


    // Komendy
    public ICommand NextCommand { get; }
    public ICommand PreviousCommand { get; }
    public ICommand WindowClosingCommand { get; }


    
    // Konstruktory
    public ReaderViewModel(Book book, BookRepository repository, int startPage)
    {
        _book = book;
        _repository = repository;

        NextCommand = new RelayCommand(ExecuteNext, CanExecuteNext);
        PreviousCommand = new RelayCommand(ExecutePrevious, CanExecutePrevious);
        WindowClosingCommand = new RelayCommand(ExecuteWindowClosing);

        LoadEpub(book.ContentFilePath);

        if (_chapterPaths.Count > 0)
        {
            CurrentChapterIndex = Math.Clamp(startPage - 1, 0, _chapterPaths.Count - 1);
        }
    }



    // Metody
    private void LoadEpub(string epubPath)
    {
        if (!File.Exists(epubPath))
        {
            return;
        }

        try
        {
            // Folder tymczasowy
            _tempExtractPath = Path.Combine(Path.GetTempPath(), "Libraria_EPUB", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempExtractPath);

            // Wypakowanie EPUB
            ZipFile.ExtractToDirectory(epubPath, _tempExtractPath, overwriteFiles: true);

            // Parsowanie container.xml
            string containerPath = Path.Combine(_tempExtractPath, "META-INF", "container.xml");
            XDocument containerXml = XDocument.Load(containerPath);
            XNamespace nsContainer = "urn:oasis:names:tc:opendocument:xmlns:container";

            string opfRelativePath = containerXml.Descendants(nsContainer + "rootfile")
                                                 .First()
                                                 .Attribute("full-path")!.Value;

            string opfFullPath = Path.Combine(_tempExtractPath, opfRelativePath);
            string opfBaseDir = Path.GetDirectoryName(opfFullPath) ?? _tempExtractPath;

            // Parsowanie content.opf
            XDocument opfXml = XDocument.Load(opfFullPath);
            XNamespace nsOpf = "http://www.idpf.org/2007/opf";

            // manifets
            var manifestItems = opfXml.Descendants(nsOpf + "item")
                                      .ToDictionary(
                                          x => x.Attribute("id")!.Value,
                                          x => x.Attribute("href")!.Value
                                      );

            var spineItems = opfXml.Descendants(nsOpf + "itemref")
                                   .Select(x => x.Attribute("idref")!.Value)
                                   .ToList();

            _chapterPaths = spineItems.Select(idref => Path.Combine(opfBaseDir, manifestItems[idref])).ToList();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Failed to load EPUB: {ex.Message}");
        }
    }


    private void ExecuteNext()
    {
        CurrentScrollPosition = 0;
        CurrentChapterIndex++;
    }
    private bool CanExecuteNext() => CurrentChapterIndex < _chapterPaths.Count - 1;


    private void ExecutePrevious()
    {
        CurrentScrollPosition = 0;
        CurrentChapterIndex--;
    }
    private bool CanExecutePrevious() => CurrentChapterIndex > 0;


    private void UpdateBookProgress()
    {
        _book.CurrentPage = CurrentChapterIndex + 1;
        _book.TotalPages = _chapterPaths.Count;

        _repository.UpdateBook(_book); // autom. zapis
    }

    private void ExecuteWindowClosing()
    {
        if (Directory.Exists(_tempExtractPath))
        {
            try
            {
                Directory.Delete(_tempExtractPath, true);
            }
            catch { }
        }
    }

    public void UpdateChapterFromNavigation(string navigatedUrl)
    {
        if (string.IsNullOrEmpty(navigatedUrl))
        {
            return;
        }

        try
        {
            var uri = new Uri(navigatedUrl);
            if (uri.IsFile)
            {
                string localPath = uri.LocalPath;

                for (int i = 0; i < _chapterPaths.Count; i++)
                {
                    if (string.Equals(Path.GetFullPath(_chapterPaths[i]), localPath, StringComparison.OrdinalIgnoreCase))
                    {
                        if (CurrentChapterIndex != i)
                        {
                            CurrentScrollPosition = 0;
                            CurrentChapterIndex = i;
                        }
                        break;
                    }
                }
            }
        }
        catch { }
    }
}