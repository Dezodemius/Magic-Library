namespace Core
{
    /// <summary>
    /// Объект Книга.
    /// </summary>
    public class Book : Entity
    {
        /// <summary>
        /// Идентификатор книги.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название книги.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Автор книги.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Дата добавления книги.
        /// </summary>
        public string CreationDate { get; set; } = string.Empty;

        /// <summary>
        /// Размер файла.
        /// </summary>
        public int? Size { get; set; } = null;

        /// <summary>
        /// Текстовый слой книги.
        /// </summary>
        public string Body { get; private set; } = string.Empty;
    }
}
