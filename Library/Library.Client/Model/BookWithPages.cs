using System;
using System.Collections.Generic;

namespace Library.Entity
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