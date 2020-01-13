namespace Core
{
    /// <summary>
    /// Объект автора.
    /// </summary>
    public class Author : Entity
    {
        /// <summary>
        /// Имя автора.
        /// </summary>
        public string FirstName { get; } = string.Empty;

        /// <summary>
        /// Фамилия автора.
        /// </summary>
        public string LastName { get; } = string.Empty;

        /// <summary>
        /// Полное имя.
        /// </summary>
        public string FullName { get; }

        public Author(string lastName, string firstName)
        {
            LastName = lastName;
            FirstName = firstName;

            FullName = $"{FirstName} {LastName}";
        }
    }
}