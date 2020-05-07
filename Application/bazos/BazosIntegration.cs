using Interfaces;
using Models;
using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Linq;
using System.Threading;
using System.Reflection;

namespace Application.Sample
{
    public class BazosIntegration : IWebSiteIntegration
    {
        /*
         * Zmiany:
         * 
         * na stronie bazos jedynymi informacjami gwarantowanymi dla ogłoszenią są:
         * - Imię właściciela
         * - Miasto z numerem pocztowym
         * 
         * Reszta informacji może zawierać się jedynie w opisie ogłoszenia, ale nie musi
         * W związku z tym model został dostosowany do strony bazos.pl - niektóre zmienne zostały zamienione na nullowalne
         * 
         * Dodałem również miasto "NIEZNANY" w razie gdyby miasto podane przez właściciela nie znajdowało się na liście
         * 
         * W BazosIntergation w pierwszej kolejności dodaję do listy wszelkie linki do poszczególnych ofert (z Mieszkanie - Sprzedam oraz Mieszkanie - Wynajmę)
         * W InfoExtracter wpisuję do słownika wszelkie informacje jakie udaje mi się wyłapać z oferty
         * Ponownie w BazosIntergation przypisuje wartości ze słownika do odpowiednich zmiennych modelu
         * W BazosComparer porównuję oferty na podstawie zmiennych modelu - jeżeli zgadza się 90% informacji, uznaję ofertę za tą samą
         * 
         * Niestety nie potrafiłem wydobyć numeru telefonu - strona bazos ma zabezpieczenia przed ściąganiem numerów ze strony
         * Aby zobaczyć numer, trzeba potwierdzić swoje konto weryfikując własny numer telefonu - nie byłem w stanie zaimplementować do programu procesu autoryzacji
         * 
         */
        public WebPage WebPage { get; }
        public IDumpsRepository DumpsRepository { get; }

        public IEqualityComparer<Entry> EntriesComparer { get; }

        public BazosIntegration(IDumpsRepository dumpsRepository,
            IEqualityComparer<Entry> equalityComparer)
        {
            DumpsRepository = dumpsRepository;
            EntriesComparer = equalityComparer;
            WebPage = new WebPage
            {
                Url = "https://nieruchomosci.bazos.pl/",
                Name = "Bazos Integration",
                WebPageFeatures = new WebPageFeatures
                {
                    HomeSale = true,
                    HomeRental = true,
                    HouseSale = false,
                    HouseRental = false
                }
            };
        }

        public Dump GenerateDump()
        {
            List<string> pages = new List<string>();
            HtmlWeb web = new HtmlWeb();
            string url = WebPage.Url + "wynajem/mieszkania/";
            HtmlDocument doc = web.Load(url);
            GetAllPages(pages, doc, url);
            url = WebPage.Url + "sprzedaz/mieszkania/";
            doc = web.Load(url);
            GetAllPages(pages, doc, url);
            //Tutaj w normalnej sytuacji musimy ściągnąć dane z konkretnej strony, przeparsować je i dopiero wtedy zapisać do modelu Dump

            var dump = new Dump()
            {
                DateTime = DateTime.Now,
                WebPage = WebPage,
                Entries = new List<Entry>()
            };

            List<Entry> dumpEntries = new List<Entry>();
            // Dla każdego ogłoszenia ze strony głównej tworzymy nowe Entry i dodajemy do Dumpa
            foreach (var page in pages)
            {
                Dictionary<string, string> info = CreateDictionary();

                doc = web.Load(page);
                InfoExtracter.ExtractInfoFromPropertyPage(info, doc);
                decimal ppm;
                PolishCity city;

                var dateStr = info["CreationDateTime"];
                var dateDT = DateTime.Parse(dateStr);

                if (Enum.IsDefined(typeof(PolishCity), info["City"]))
                {
                    city = (PolishCity)System.Enum.Parse(typeof(PolishCity), info["City"].ToUpper());
                }
                else
                {
                    city = 0;
                }

                if (info["Area"] != "-1")
                {
                    ppm = Convert.ToDecimal(info["TotalGrossPrice"]) / Convert.ToDecimal(info["Area"]);
                }
                else
                {
                    ppm = 0;
                }

                OfferKind offer;
                if (info["Rental"] == "WYNAJEM")
                {
                    offer = OfferKind.RENTAL;
                }
                else
                {
                    offer = OfferKind.SALE;
                }

                Entry entry = new Entry
                {

                    OfferDetails = new OfferDetails
                    {
                        Url = page,
                        CreationDateTime = dateDT, 
                        LastUpdateDateTime = null, //Strona bazos nie zawiera informacji o aktualizacji ogłoszenia 
                        OfferKind = offer,
                        SellerContact = new SellerContact
                        {
                            Email = info["Email"],
                            Name = info["Name"],
                            Telephone = info["Telephone"]
                        },

                        IsStillValid = true
                    },
                    PropertyDetails = new PropertyDetails
                    {
                        Area = Convert.ToDecimal(info["Area"]),
                        NumberOfRooms = Convert.ToInt32(info["NumberOfRooms"]),
                        FloorNumber = Convert.ToInt32(info["FloorNumber"]),
                        YearOfConstruction = Convert.ToInt32(info["YearOfConstruction"]),
                    },
                    PropertyFeatures = new PropertyFeatures
                    {
                        GardenArea = Convert.ToDecimal(info["GardenArea"]),
                        Balconies = Convert.ToInt32(info["Balconies"]),
                        BasementArea = Convert.ToDecimal(info["BasementArea"]),
                        OutdoorParkingPlaces = Convert.ToInt32(info["OutdoorParkingPlaces"]),
                        IndoorParkingPlaces = Convert.ToInt32(info["IndoorParkingPlaces"]),
                    },
                    PropertyAddress = new PropertyAddress
                    {
                        City = city,
                        District = info["District"],
                        StreetName = info["StreetName"],
                        DetailedAddress = info["DetailedAddress"],
                    },
                    PropertyPrice = new PropertyPrice
                    {
                        TotalGrossPrice = Convert.ToDecimal(info["TotalGrossPrice"]),
                        PricePerMeter = ppm,
                        ResidentalRent = Convert.ToInt32(info["ResidentalRent"]),
                    },
                    RawDescription = info["RawDescription"]
                };

                ChangeEmptyToNull(entry); //Wszelkie puste infromacje zamieniamy na nulla

                dumpEntries.Add(entry);
            }
            dump.Entries = dumpEntries;

            return dump;
        }

        private static void ChangeEmptyToNull(Object entry)
        {
            foreach (PropertyInfo propertyInfo in entry.GetType().GetProperties())
            {
                var prop = propertyInfo.GetValue(entry);
                if (propertyInfo.PropertyType == typeof(string))
                {
                    if (prop.ToString() == "null")
                        propertyInfo.SetValue(entry, null);
                    prop = propertyInfo.GetValue(entry);
                }
                else if (propertyInfo.PropertyType == typeof(decimal) || propertyInfo.PropertyType == typeof(decimal?) || propertyInfo.PropertyType == typeof(int) || propertyInfo.PropertyType == typeof(int?))
                {
                    if (prop.ToString() == "-1")
                        propertyInfo.SetValue(entry, null);
                }
                else if (propertyInfo.PropertyType == typeof(DateTime) || propertyInfo.PropertyType == typeof(DateTime?) || propertyInfo.PropertyType == typeof(OfferKind) || propertyInfo.PropertyType == typeof(PolishCity))
                    continue;
                else
                {
                    ChangeEmptyToNull(prop);
                }
            }
        }

        private static Dictionary<string, string> CreateDictionary()
        {
            //Dodajemy podstawowe informacje, by później ułatwić zamianę na nulla
            Dictionary<string, string> info = new Dictionary<string, string>();
            info = new Dictionary<string, string>
            {                
                {"Email", "null"},
                {"Area", "-1"},
                {"NumberOfRooms", "-1"},
                {"FloorNumber", "-1"},
                {"YearOfConstruction", "-1"},
                {"GardenArea", "-1"},
                {"Balconies", "-1"},
                {"BasementArea", "-1"},
                {"OutdoorParkingPlaces", "-1"},
                {"IndoorParkingPlaces", "-1"},
                {"District", "null"},
                {"StreetName", "null"},
                {"TotalGrossPrice", "-1"},
                {"PricePerMeter", "-1"},
                {"ResidentalRent", "-1"},
            };
            return info;
        }


        private static void GetAllPages(List<string> pages, HtmlDocument doc, string url)
        {
            //Znajdujemy informację o ilości ogłoszeń, dzielimy to przez liczbę ogłoszeń na stronę, dostajemy liczbę wszystkich stron z których należy ściągać informacje
            var node = doc.DocumentNode.SelectSingleNode("//table[@class=\"listainzerat\"]");
            var children = node.ChildNodes;
            List<string> pageNumberInfo = new List<string>();
            InfoExtracter.ExtractInnerText(children, pageNumberInfo);
            string tempPageNumber = pageNumberInfo[1].Replace(" Wyświetlono 1-20 ogłoszeń z ", string.Empty);
            tempPageNumber = tempPageNumber.Replace(" ", string.Empty);
            var pageNumber = Convert.ToInt32(tempPageNumber) / 20;
            for (int i = 0; i < pageNumber + 1; i++)
            {
                HtmlWeb webNew = new HtmlWeb();
                HtmlDocument docNew = webNew.Load(url + i*20 + "/");
                GetAllPropertyPages(pages, docNew);
            }
        }

        private static void GetAllPropertyPages(List<string> pages, HtmlDocument doc) 
        {
            //Zbieramy linki wszystkich ogłoszeń ze strony głównej
            var pagesNodes = doc.DocumentNode.SelectNodes("//span[@class=\"nadpis\"]");
            foreach (HtmlNode node in pagesNodes)
            {
                var address = node.FirstChild.GetAttributeValue("href", string.Empty);
                address = "https://nieruchomosci.bazos.pl" + address;
                pages.Add(address);
            }
        }
    }
}

