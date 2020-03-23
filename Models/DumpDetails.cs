using System;

namespace Models
{
    public class DumpDetails
    {
        /// <summary>
        /// Czas zrzutu danych
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Informacje o stronie z której zrzucono dane. Dane te nie muszą być wyciągane ze strony automatycznie.
        /// Można je po prostu "zahardcodować"
        /// </summary>
        public WebPage WebPage { get; set; }
    }
}