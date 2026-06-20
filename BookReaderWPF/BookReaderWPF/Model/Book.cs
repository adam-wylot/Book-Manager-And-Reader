using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BookReader.Model;

internal class Book
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CoverBase64 { get; set; } = "";
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public string ContentFilePath { get; set; } = "";
    public int Rating { get; set; }
    public int ScrollPosition { get; set; }
}
