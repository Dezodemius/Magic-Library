using System;
using Library.Entity;

namespace Library.Client.Model
{
  public class BookWithPages : Book
  {
    public string Pages { get; }

    public BookWithPages(Guid id, string name, string pages)
    {
      this.Id = id;
      this.Name = name;
      this.Pages = pages;
    }

    public BookWithPages(Book book, string pages)
    {
      this.Id = book.Id;
      this.Name = book.Name;
      this.Pages = pages;
    }
  }
}