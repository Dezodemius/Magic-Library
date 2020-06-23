﻿using System.IO;

namespace Library
{
  public static class LibraryConstants
  {
    public static readonly DirectoryInfo DefaultBookshelfDirectoryPath = 
      new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "Bookshelf"));

    public const string DefaultIndexName = "books";

    public static readonly string[] SupportedExtensions = { "pdf"};

    public const string HelpMessage = @"
Esc - Выход.
S - Поиск.
A - Добавить книгу в Elasticsearch.
D - Добавить книги в Elasticsearch.
W - Показать все книги в Elasticsearch";
  }
}