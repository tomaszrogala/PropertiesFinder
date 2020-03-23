namespace Models
{
    public class WebPage
    {
        /// <summary>
        /// Adres strony głównej z której zebrano dane
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Biznesowa nazwa portalu (np. "Wirtualna Polska")
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Funkcjonalności jakie strona dostarcza
        /// </summary>
        public WebPageFeatures WebPageFeatures { get; set; }
    }
}