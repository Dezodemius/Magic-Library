using System;
using System.Collections.Generic;
using Library.Entity;

namespace Library.Client.Model
{
  public class HighlightWithPages
  {
    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Highlight { get; set; }
    public float Page { get; set; }
  }
  
  public class BookWithPages : Book
  {
    public IList<float> Pages { get; }

    public List<HighlightWithPages> Highlights { get; set; } = new List<HighlightWithPages>();

    public BookWithPages(Guid id, string name, IList<float> pages)
    {
      this.Id = id;
      this.Name = name;
      this.Pages = pages;
    }

    public BookWithPages(Book book, IList<float> pages)
    {
      this.Id = book.Id;
      this.Name = book.Name;
      this.Pages = pages;
    }

    public BookWithPages()
    {
      
    }
  }
}