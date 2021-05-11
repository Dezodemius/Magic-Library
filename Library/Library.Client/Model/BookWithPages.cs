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
      Id = id;
      Name = name;
      Pages = pages;
    }

    public BookWithPages(Book book, IList<float> pages)
    {
      Id = book.Id;
      Name = book.Name;
      Pages = pages;
    }

    public BookWithPages()
    {
      
    }
  }
}