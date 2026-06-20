# Libraria — Personal Book Manager And Reader

A desktop application for organizing and reading EPUB books, built with **WPF** and **.NET 9**. Libraria lets users create personal libraries, manage book metadata, track reading progress, and read books in a built-in EPUB reader — all without external dependencies beyond the .NET runtime.

---

## Overview

Libraria is a university WPF project that demonstrates desktop application development in C#: from UI design and data binding to file persistence and format parsing. The app uses a custom **MVVM** architecture with clear separation between models, view models, and views.

**Key capabilities:**

- Create and open library repositories (`.librepo` JSON files)
- Add, edit, and delete books with metadata (title, author, description, cover, tags, rating)
- Search, filter, and sort the library by title, author, tag, reading status, and rating
- Read EPUB files in an integrated reader with chapter navigation
- Automatically save reading progress (current chapter and scroll position)
- View library statistics: total books, read / in-progress counts, average rating, most popular genre

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| Framework | .NET 9 (`net9.0-windows`) |
| UI | WPF (XAML) |
| Language | C# 12 (nullable reference types enabled) |
| Architecture | MVVM |
| Data storage | JSON serialization (`System.Text.Json`) |
| EPUB handling | `System.IO.Compression` + XML parsing (`System.Xml.Linq`) |
| Book rendering | WPF `WebBrowser` control |

No third-party NuGet packages — the project relies entirely on the .NET SDK and built-in WPF libraries.

---

## Architecture

```
BookReaderWPF/
├── Model/              # Domain entities and data access
│   ├── Book.cs
│   └── BookRepository.cs
├── ViewModel/          # Application logic and state
│   ├── BaseViewModel.cs
│   ├── MainViewModel.cs
│   ├── BookListViewModel.cs
│   ├── BookDetailViewModel.cs
│   ├── BookEditViewModel.cs
│   ├── ReaderViewModel.cs
│   ├── WelcomeViewModel.cs
│   └── RelayCommand.cs
├── View/
│   ├── Windows/        # MainWindow, ReaderWindow, AddEditBookWindow
│   ├── Controls/       # Reusable user controls (list, detail, rating, tags, stats)
│   └── Converters/     # Base64ToImageConverter
└── Resources/          # App icon and assets
```

### MVVM highlights

- **`BaseViewModel`** — implements `INotifyPropertyChanged` with a reusable `SetProperty<T>` helper
- **`RelayCommand`** — custom `ICommand` implementation with `CanExecute` support
- **View navigation** — `DataTemplate` switching in `MainWindow` binds `CurrentPage` to the active view model (Welcome → Book List → Book Detail)
- **Shared state** — `ObservableCollection<Book>` is shared between view models for consistent UI updates
- **Filtering** — `ICollectionView` with custom filter logic and `SortDescription` for dynamic library views

### Data persistence

Libraries are stored as `.librepo` files — indented JSON arrays of `Book` objects. The `BookRepository` handles load/save with error handling for corrupt or inaccessible files. Every CRUD operation and reading progress update triggers an automatic save.

Book covers are stored as Base64-encoded JPEG images (resized to 400px width, quality 80%) directly in the repository file, keeping libraries self-contained.

### EPUB reader

The reader unpacks EPUB files (ZIP archives) to a temporary directory, parses `META-INF/container.xml` and the OPF manifest/spine, and navigates chapters in reading order. Progress is tracked per chapter (`CurrentPage` / `TotalPages`) and scroll position is preserved via JavaScript interop with the `WebBrowser` control.

A custom **attached property** (`WebBrowserBehavior.BindableSource`) bridges the gap between WPF data binding and the non-bindable `WebBrowser` control.

---

## Features in Detail

### Library management
- Create a new repository or open an existing one via file dialogs
- Full CRUD for books with validation (title required)
- Tag management with duplicate prevention
- Star rating control (1–5)
- Cover image picker with automatic compression

### Search & filters
- Full-text search by title or author
- Filter by tag, reading status (Unread / In Progress / Read), and minimum rating
- Sort by title, author, or rating
- One-click "Clear filters" when any filter is active

### Reading experience
- "Read from start" and "Continue reading" actions
- Previous / Next chapter navigation
- Scroll position restored when returning to a chapter
- Progress bar based on chapters read vs. total chapters
- Graceful handling of missing EPUB files

### Statistics bar
A persistent footer displays:
- Total books in the library
- Books fully read and in progress
- Average star rating across rated books
- Most frequently used tag (genre)

### Keyboard shortcuts
| Shortcut | Action |
|----------|--------|
| `Ctrl+C` | Create repository |
| `Ctrl+O` | Open repository |
| `Ctrl+A` | Add book |
| `Ctrl+E` | Edit selected book |
| `Delete` | Delete selected book |

---

## Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Windows (WPF is Windows-only)

### Build & run

```bash
cd BookReaderWPF/BookReaderWPF
dotnet build
dotnet run --project BookReaderWPF
```

Or open `BookReaderWPF.sln` in Visual Studio 2022+ and press F5.

### Sample data
The `SampleBookData/covers/` folder includes sample cover images that can be used when adding books during development or demos.

---

## Skills Demonstrated

This project showcases practical desktop development skills relevant to .NET/WPF roles:

- **MVVM pattern** — clean separation of UI and logic without external MVVM frameworks
- **Data binding** — two-way bindings, `DataTemplate` selectors, value converters, attached properties
- **WPF UI** — custom control templates, styles, resource dictionaries, user controls
- **File I/O** — JSON persistence, file dialogs, image encoding/decoding
- **Format parsing** — EPUB structure (OPF spine/manifest) parsed with LINQ to XML
- **State management** — shared collections, navigation between view models, automatic progress saving
- **UX details** — keyboard shortcuts, placeholder text, conditional visibility, status messages
