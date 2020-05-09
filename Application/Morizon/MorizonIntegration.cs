using HtmlAgilityPack;
using Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Classes {
    public class MorizonIntegration : IWebSiteIntegration {
        public WebPage WebPage { get; }
        public IDumpsRepository DumpsRepository { get; }

        public IEqualityComparer<Entry> EntriesComparer { get; }

        public MorizonIntegration(IDumpsRepository dumpsRepository,
            IEqualityComparer<Entry> equalityComparer) {
            DumpsRepository = dumpsRepository;
            EntriesComparer = equalityComparer;
            WebPage = new WebPage {
                Url = "https://morizon.pl/",
                Name = "Morizon Integration",
                WebPageFeatures = new WebPageFeatures {
                    HomeSale = true,
                    HomeRental = true,
                    HouseSale = true,
                    HouseRental = true
                }
            };
        }

        public static Dictionary<string, int> MonthsMapped = new Dictionary<string, int> {
                { "stycznia", 1},
                { "lutego", 2},
                { "marca", 3},
                { "kwietnia", 4},
                { "maja", 5},
                { "czerwca", 6},
                { "lipca", 7},
                { "sierpnia", 8},
                { "września", 9},
                { "października", 10},
                { "listopada", 11},
                { "grudnia", 12}};

        Dictionary<string, string> PolishCharactersMapper = new Dictionary<string, string> {
                    { "Ą", "A" },
                    { "Ć", "C" },
                    { "Ę", "E" },
                    { "Ł", "L" },
                    { "Ń", "N" },
                    { "Ó", "O" },
                    { "Ś", "S" },
                    { "Ź", "Z" },
                    { "Ż", "Z" },
                    {" ", "_" }};

        public static DateTime getDate(string date) {
            DateTime NewDate;
            date = date.Trim();

            if ( date == "dzisiaj" ) {
                NewDate = DateTime.Now;
            }
            else if ( date == "wczoraj" ) {
                NewDate = DateTime.Now.AddDays(-1);
            }
            else {
                string[] parts = date.Split(null);
                int month = MonthsMapped[parts[1]];
                NewDate = new DateTime(int.Parse(parts[2]), month, int.Parse(parts[0]));
            }
            return NewDate;
        }

        public static int getFloor(string data) {
            string floor = data.Split("/")[0].Trim();
            return floor == "parter" ? 0 : int.Parse(floor);
        }

        public OfferDetails CreateOfferDetails(HtmlNode Property, string PropertyUrl) {
            string CreationDate = Property.SelectSingleNode("//*[text()[contains(., 'Opublikowano: ')]]").ParentNode.SelectSingleNode("td").InnerText;
            string UpdateDate = Property.SelectSingleNode("//*[text()[contains(., 'Zaktualizowano: ')]]").ParentNode.SelectSingleNode("td").InnerText;

            string Name = Property.SelectSingleNode("//div[@class='companyName']")?.InnerText;
            if ( Name == null ) {
                var data = Property.SelectSingleNode("//div[@class='agentName']")?.InnerText;
                Name = data != null ? data : "";
                Name = Name + "(osoba prywatna)";
            }

            SellerContact SellerContact;
            string Telephone = Property.SelectSingleNode("//span[@class='phone hidden']")?.InnerText;
            if ( Telephone != null ) {
                SellerContact = new SellerContact {
                    Name = Name,
                    Telephone = Telephone
                };
            }
            else {
                SellerContact = new SellerContact {
                    Name = Name
                };
            }

            OfferDetails OfferDetails = new OfferDetails {
                Url = PropertyUrl,
                CreationDateTime = getDate(CreationDate),
                LastUpdateDateTime = getDate(UpdateDate),
                OfferKind = OfferKind.SALE,
                SellerContact = SellerContact,
                IsStillValid = true
            };
            return OfferDetails;
        }

        public PropertyPrice CreatePropertyPrice(HtmlNode Property) {
            Decimal Price;
            string DataPrice = Property.SelectSingleNode("//li[@class='paramIconPrice']/em")?.InnerText;
            if ( !Decimal.TryParse(DataPrice, out Price) ) {
                // These page does not contain information about price (need to ask a seller)
                Price = -1;
            }

            Decimal PricePerMeter;
            string DataPricePerMeter = Property.SelectSingleNode("//li[@class='paramIconPriceM2']/em")?.InnerText;
            if ( DataPricePerMeter == null || !Decimal.TryParse(DataPricePerMeter.Split("&")[0], out PricePerMeter) ) {
                // These page does not contain information about price per meter (need to ask a seller)
                PricePerMeter = -1;
            }

            // Morizon does not provide data about Residental Rent 
            PropertyPrice PropertyPrice = new PropertyPrice {
                TotalGrossPrice = Price,
                PricePerMeter = PricePerMeter,
                ResidentalRent = null
            };
            return PropertyPrice;
        }

        public PropertyDetails CreatePropertyDetails(HtmlNode Property) {

            string FloorData = Property.SelectSingleNode("//*[text()[contains(., 'Piętro: ')]]")?.ParentNode.SelectSingleNode("td")?.InnerText;
            int? FloorNumber = null;
            if ( FloorData != null )
                FloorNumber = getFloor(FloorData);

            int NumberOfRooms;
            string Rooms = Property.SelectSingleNode("//li[@class='paramIconNumberOfRooms']/em")?.InnerText;
            if ( Rooms == null || !int.TryParse(Rooms.Split("&")[0], out NumberOfRooms) ) {
                NumberOfRooms = -1;
            }

            string Year = Property.SelectSingleNode("//*[text()[contains(., 'Rok budowy: ')]]")?.ParentNode?.SelectSingleNode("td")?.InnerText;

            PropertyDetails PropertyDetails = new PropertyDetails {
                Area = Decimal.Parse(Property.SelectSingleNode("//li[@class='paramIconLivingArea']/em").InnerText.Split("&")[0]),
                NumberOfRooms = NumberOfRooms,
                FloorNumber = FloorNumber,
                YearOfConstruction = ( Year == null ) ? (int?)null : int.Parse(Year)
            };
            return PropertyDetails;
        }

        public PropertyAddress CreatePropertyAddress(HtmlNode Property) {
            string[] Address = Property.SelectSingleNode("//nav[@class='breadcrumbs']").InnerText.Split("\n");
            Address = Address.Where(val => val != "").ToArray();

            string city = PolishCharactersMapper.Aggregate(Address[3].Trim(null).ToUpper(), (current, value) =>
                    current.Replace(value.Key, value.Value));

            string District = Address[Address.Length - 2].Trim().ToLower();
            string StreetData = Address.Last().Trim().ToLower();

            if ( StreetData.ToLower().StartsWith("ul.") )
                StreetData = StreetData.Substring(9, StreetData.Length - 9).Trim().ToLower();

            string[] StreetDataArray = StreetData.Split("&nbsp;");
            string StreetNumber = StreetDataArray.Last();
            string[] Street = StreetDataArray.Take(StreetDataArray.Count() - 1).ToArray();

            PropertyAddress PropertyAddress;
            try {
                PropertyAddress = new PropertyAddress {
                    City = (PolishCity)Enum.Parse(typeof(PolishCity), city),
                    District = Address[Address.Length - 2],
                    StreetName = string.Join(" ", Street),
                    DetailedAddress = StreetNumber
                };
            }
            catch ( ArgumentException ) {
                PropertyAddress = new PropertyAddress {
                    District = Address[Address.Length - 2],
                    StreetName = string.Join(" ", Street),
                    DetailedAddress = StreetNumber
                };
            }

            return PropertyAddress;
        }

        public PropertyFeatures CreatePropertyFeatures(HtmlNode Property, string Description) {
            var Facilities = Property.SelectSingleNode("//*[text()[contains(., 'Udogodnienia')]]")?.NextSibling?.NextSibling?.InnerText;
            // When page does not contain information about facilities it means that property does not have it => value 0
            // When page contains information, but we can't get exact quantity => value null

            if ( Facilities == null )
                Facilities = "";

            decimal? GardenArea = 0;
            if ( Facilities.Contains("ogr&oacute;d") || Description.Contains("ogr&oacute;d") ) {
                GardenArea = null;
            }

            int? Balconies = 0;
            if ( Facilities.Contains("balkon") || Description.Contains("balkon") ) {
                Balconies = null;
            }

            int? OutdoorParkingPlaces = 0;
            if ( Facilities.Contains("miejsce parkingowe") || Description.Contains("parking") )
                OutdoorParkingPlaces = null;

            int? IndoorParkingPlaces = 0;
            if ( Facilities.Contains("parking podziemny") ||
                Facilities.Contains("garaż") ||
                Description.Contains("parking podziemny") ||
                Description.Contains("garaż") )
                IndoorParkingPlaces = null;

            decimal? BasementArea = 0;
            if ( Facilities.Contains("piwnica") ||
                Description.Contains("piwnica") ||
                Description.Contains("komogr&amp;oacute;rka lokatorska") ||
                Description.Contains("komogr&amp;oacute;rki  lokatorskie") ) {
                BasementArea = null;
            }

            PropertyFeatures PropertyFeatures = new PropertyFeatures {
                GardenArea = GardenArea,
                Balconies = Balconies,
                BasementArea = BasementArea,
                OutdoorParkingPlaces = OutdoorParkingPlaces,
                IndoorParkingPlaces = IndoorParkingPlaces,
            };

            return PropertyFeatures;
        }

        List<Entry> getEntries(string Url, string UrlPath, List<Entry> Entries) {
            var Web = new HtmlWeb();
            var Doc = Web.Load(Url + UrlPath);
            Console.WriteLine(UrlPath); // TODO

            string NextUrl = Doc.DocumentNode.SelectSingleNode(".//*[contains(@title,'następna strona')]")?.Attributes["href"]?.Value;

            List<string> PropertiesUrls = new List<string>();

            foreach ( HtmlAgilityPack.HtmlNode Node in Doc.DocumentNode.SelectNodes("//a[@class='property_link property-url']") ) {
                PropertiesUrls.Add(Node.Attributes["href"].Value);
            }

            foreach ( string PropertyUrl in PropertiesUrls ) {
                Console.WriteLine(PropertyUrl); // TODO
                var Property = Web.Load(PropertyUrl).DocumentNode;

                var Description = Property.SelectSingleNode("//div[@class='description']")?.InnerText;
                Description = Description != null ? Description : "";

                Entry PropertyOffer = new Entry {
                    OfferDetails = CreateOfferDetails(Property, PropertyUrl),
                    PropertyPrice = CreatePropertyPrice(Property),
                    PropertyDetails = CreatePropertyDetails(Property),
                    PropertyAddress = CreatePropertyAddress(Property),
                    PropertyFeatures = CreatePropertyFeatures(Property, Description),
                    RawDescription = Description
                };
                Entries.Add(PropertyOffer);
            }

            if ( NextUrl != null )
                return getEntries(Url, NextUrl, Entries);

            return Entries;
        }

        public Dump GenerateDump() {

            List<Entry> Entries = new List<Entry>();

            var Url = "https://www.morizon.pl";

            Entries = getEntries(Url, "/mieszkania", Entries);

            List<Entry> uniqueEntries = Entries.Distinct(new MorizonComparer()).ToList();

            return new Dump {
                DateTime = DateTime.Now,
                WebPage = WebPage,
                Entries = uniqueEntries
            };
        }
    }
}