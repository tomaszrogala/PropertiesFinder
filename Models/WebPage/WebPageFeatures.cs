namespace Models
{
    public class WebPageFeatures
    {
        /// <summary>
        /// Czy strona udostępnia mieszkania na sprzedaż
        /// </summary>
        public bool HomeSale { get; set; }

        /// <summary>
        /// Czy strona udostępnia mieszkania na wynajem
        /// </summary>
        public bool HomeRental { get; set; }

        /// <summary>
        /// Czy strona udostępnia domy na sprzedaż
        /// </summary>
        public bool HouseSale { get; set; }

        /// <summary>
        /// Czy strona udostępnia domy na wynajem
        /// </summary>
        public bool HouseRental { get; set; }
    }
}