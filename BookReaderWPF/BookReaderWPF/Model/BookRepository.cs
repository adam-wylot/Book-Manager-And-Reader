using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace BookReader.Model;

internal class BookRepository
{
    private List<Book> _books = new();
    public string? FilePath { get; private set; }


    // Metody
    public void Initialize(string path)
    {
        FilePath = path;
        _books.Clear();
        Save();
    }

    public IEnumerable<Book> GetAllBooks()
    {
        return _books;
    }


    /// Książki

    public void AddBook(Book book)
    {
        _books.Add(book);
        Save();
    }

    public void RemoveBook(Book book)
    {
        _books.RemoveAll(b => b.Id == book.Id);
        Save();
    }

    public void UpdateBook(Book book)
    {
        var index = _books.FindIndex(b => b.Id == book.Id);
        if (index != -1)
        {
            _books[index] = book;
        }
        Save();
    }

    /// Repo

    /// <summary>
    /// Wczytanie z pliku
    /// </summary>
    /// <param name="path">ścieżka do pliku</param>
    public bool Load(string path)
    {
        FilePath = path;
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                _books = JsonSerializer.Deserialize<List<Book>>(json) ?? new List<Book>();
                return true; // Wszystko poszło ok
            }
            catch (JsonException)
            {
                // Plik istnieje, ale nie jest w poprawnym formacie
                _books.Clear();
                return false;
            }
            catch (IOException)
            {
                // Problem z dostępem
                _books.Clear();
                return false;
            }
        }
        else
        {
            _books.Clear();
            return true; // plik nie istnieje
        }
    }

    /// <summary>
    /// Zapis do pliku
    /// </summary>
    private void Save()
    {
        if (string.IsNullOrEmpty(FilePath)) return;
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_books, options);
            File.WriteAllText(FilePath, json);
        }
        catch (IOException) { /* zapisze sie przy nastepnej akcji */ }
    }
}