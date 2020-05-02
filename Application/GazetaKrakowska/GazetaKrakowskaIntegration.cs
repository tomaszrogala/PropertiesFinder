using HtmlAgilityPack;
using Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.GazetaKrakowska
{
    public class GazetaKrakowskaIntegration : IWebSiteIntegration
    {
        public List<GazetaKrakowskaOffer> Offers { get; set; }

        public HtmlWeb Web { get; }
        public WebPage WebPage { get; }
        public IDumpsRepository DumpsRepository { get; }

        public IEqualityComparer<Entry> EntriesComparer { get; }

        public GazetaKrakowskaIntegration(IDumpsRepository dumpsRepository, IEqualityComparer<Entry> equalityComparer)
        {
            DumpsRepository = dumpsRepository;
            EntriesComparer = equalityComparer;
            Web = new HtmlWeb();

            WebPage = new WebPage
            {
                Url = "http://gazetakrakowska.pl",
                Name = "Gazeta Krakowska WebSite Integration",
                WebPageFeatures = new WebPageFeatures
                {
                    HomeSale = false,
                    HomeRental = false,
                    HouseSale = true,
                    HouseRental = true
                }
            };

            // Domy
            string address = "http://gazetakrakowska.pl/ogloszenia/28733,8437,fm,pk.html";
            FetchAllOffers(address);
        }

        public Dump GenerateDump()
        {
            return new Dump
            {
                DateTime = DateTime.Now,
                WebPage = WebPage,
                Entries = CreateEntries()
            };
        }

        private void FetchAllOffers(string firstPageAddress)
        {
            Offers = GetOffersFromSinglePage(firstPageAddress);
            int pageNumber = 1;
            int count = Offers.Count;
            while (count > 0)
            {
                pageNumber++;
                string nextAddress = ChangeAddressPage(pageNumber);
                List<GazetaKrakowskaOffer> singlePageOffers = GetOffersFromSinglePage(nextAddress);
                Offers.AddRange(singlePageOffers);
                count = singlePageOffers.Count;
            }
        }
        private string ChangeAddressPage(int pageNumber)
        {
            return "http://gazetakrakowska.pl/ogloszenia/" + pageNumber + ",28733,8437,fm,pk.html";
        }

        private List<GazetaKrakowskaOffer> GetOffersFromSinglePage(String address)
        {
            var page = new HtmlDocument();
            try
            {
                page = Web.Load(address);
            }
            catch (Exception)
            {
                return new List<GazetaKrakowskaOffer>();
            }

            var linksNodes = page.DocumentNode.SelectNodes("//*[@id=\"lista-ogloszen\"]/ul/li//a");
            List<string> links = new List<string>();

            if (linksNodes != null)
                links = linksNodes.Select(link => link.GetAttributeValue("href", "")).ToList();

            var creationDates = GetValuesFromNodeList(page.DocumentNode.SelectNodes("//*[@id=\"lista-ogloszen\"]/ul/li//a/div/footer/p//time[1]"));
            var updatedDates = GetValuesFromNodeList(page.DocumentNode.SelectNodes("//*[@id=\"lista-ogloszen\"]/ul/li//a/div/footer/p//time[2]"));

            return links.ZipThree(creationDates, updatedDates, (link, createDate, updateDate) => new GazetaKrakowskaOffer
            {
                UrlDetails = link,
                CreationDateTime = DateTime.Parse(createDate),
                LastUpdateDateTime = DateTime.Parse(updateDate)
            }).ToList();
        }

        private List<String> GetValuesFromNodeList(HtmlNodeCollection nodes)
        {
            if (nodes != null)
                return nodes.Select(parameter => parameter.InnerText).ToList();
            return new List<String>();
        }

        private List<Entry> CreateEntries()
        {
            List<Entry> entries = new List<Entry>();

            Offers.ForEach(offer =>
            {
                entries.Add(CreateNewEntry(offer));
            });

            return entries;
        }

        private Entry CreateNewEntry(GazetaKrakowskaOffer offer)
        {
            var detailsPage = new HtmlDocument();

            try
            {
                detailsPage = Web.Load(offer.UrlDetails);
            }
            catch (Exception)
            {
                return CreateSimpleEntry(offer);
            }

            var offerParameters = GetPageParameters(detailsPage);

            return new Entry
            {
                OfferDetails = CreateOfferDetails(offer, detailsPage),
                PropertyPrice = CreatePropertyPrice(detailsPage, offerParameters),
                PropertyDetails = CreatePropertyDetails(offerParameters),
                PropertyAddress = CreatePropertyAddress(detailsPage),
                PropertyFeatures = CreatePropertyFeatures(offerParameters),
                RawDescription = GetNodeText(detailsPage.DocumentNode.SelectSingleNode("//*[@class=\"description\"]/div/section/div"))
            };
        }

        private Entry CreateSimpleEntry(GazetaKrakowskaOffer offer)
        {
            return new Entry
            {
                OfferDetails = new OfferDetails
                {
                    Url = offer.UrlDetails,
                    CreationDateTime = offer.CreationDateTime,
                    LastUpdateDateTime = offer.LastUpdateDateTime
                }
            };
        }

        private Dictionary<String, String> GetPageParameters(HtmlDocument page)
        {
            var paramtersMap = new Dictionary<String, String>();
            var parameters = GetValuesFromNodeList(page.DocumentNode.SelectNodes("//*[@class=\"parameters\"]/div/ul/li/span"));
            var values = GetValuesFromNodeList(page.DocumentNode.SelectNodes("//*[@class=\"parameters\"]/div/ul/li/b"));

            return parameters.Zip(values, (parameter, value) => new { parameter, value }).ToDictionary(x => x.parameter, x => x.value);
        }

        private OfferDetails CreateOfferDetails(GazetaKrakowskaOffer offer, HtmlDocument page)
        {
            return new OfferDetails
            {
                Url = offer.UrlDetails,
                CreationDateTime = offer.CreationDateTime,
                LastUpdateDateTime = offer.LastUpdateDateTime,
                OfferKind = DetermineOfferKind(page.DocumentNode.SelectSingleNode("//*[@class=\"description\"]/div/section/div")),
                SellerContact = CreateSellerContact(page),
                IsStillValid = true
            };
        }

        private OfferKind DetermineOfferKind(HtmlNode node)
        {
            if (node != null && node.InnerText != null && node.InnerText.Contains("sprzed"))
                return OfferKind.SALE;
            return OfferKind.RENTAL;
        }

        private SellerContact CreateSellerContact(HtmlDocument page)
        {
            string name = null;

            if (page.DocumentNode.SelectSingleNode("//*[@id=\"contact_container\"]/div[1]/div/h3") != null)
            {
                name = page.DocumentNode.SelectSingleNode("//*[@id=\"contact_container\"]/div[1]/div/h3").InnerText;
            } else if (page.DocumentNode.SelectSingleNode("//*[@id=\"contact_container\"]/div[1]/div[2]/a[2]/h3") != null)
            {
                name = page.DocumentNode.SelectSingleNode("//*[@id=\"contact_container\"]/div[1]/div[2]/a[2]/h3").InnerText;
            }

            return new SellerContact
            {
                Email = null, // brak danych
                Telephone = GetNodeText(page.DocumentNode.SelectSingleNode("//*[@class=\"phoneButton\"]/strong")),
                Name = name
            };
        }

        private String GetNodeText(HtmlNode node)
        {
            if (node != null)
                return node.InnerText;
            return null;
        }

        private PropertyPrice CreatePropertyPrice(HtmlDocument page, Dictionary<String, String> offerParameters)
        {
            var totalGrossPrice = page.DocumentNode.SelectSingleNode("//*[@class=\"priceInfo__value\"]/text()");
            var pricePerMeter = page.DocumentNode.SelectSingleNode("//*[@class=\"priceInfo__additional\"]/text()");

            PropertyPrice propertyPrice = new PropertyPrice();
            propertyPrice.TotalGrossPrice = SetPrice(totalGrossPrice);
            propertyPrice.PricePerMeter = SetPrice(pricePerMeter);

            if (offerParameters.GetValueOrDefault("Op³aty (czynsz administracyjny, media)") != null)
                propertyPrice.ResidentalRent = Decimal.Parse(GetAllDigits(offerParameters.GetValueOrDefault("Op³aty (czynsz administracyjny, media)")));

            return propertyPrice;
        }

        private Decimal SetPrice(HtmlNode priceNode)
        {
            var priceValue = "";

            if (priceNode != null && priceNode.InnerText != null)
            {
                var priceInnerText = priceNode.InnerText;
                priceValue = GetAllDigits(priceInnerText);

                if (priceValue != "")
                    return decimal.Parse(priceValue);
            }

            return 0;
        }

        private string GetAllDigits(string value)
        {
            return new String(value.Where(c => Char.IsDigit(c) || c == Char.Parse(",") || c == Char.Parse(".")).ToArray()).Trim();
        }

        private PropertyDetails CreatePropertyDetails(Dictionary<String, String> offerParameters)
        {
            var area = offerParameters.GetValueOrDefault("Powierzchnia w m2", null);
            var numberOfRooms = offerParameters.GetValueOrDefault("Liczba pokoi", null);
            var floorNumber = offerParameters.GetValueOrDefault("Piêtro", null);
            var yearOfConstruction = offerParameters.GetValueOrDefault("Rok budowy", null);

            var propertyDetails = new PropertyDetails();

            if (area != null)
                propertyDetails.Area = decimal.Parse(GetAllDigits(RemoveM2(area)));

            if (numberOfRooms != null)
                propertyDetails.NumberOfRooms = int.Parse(GetAllDigits(numberOfRooms));

            if (floorNumber != null)
                propertyDetails.FloorNumber = GetFloorNumber(floorNumber);

            if (yearOfConstruction != null)
                propertyDetails.YearOfConstruction = int.Parse(yearOfConstruction);

            return propertyDetails;
        }

        private string RemoveM2(string value)
        {
            return value.Replace("m2", "");
        }

        private int GetFloorNumber(string floor)
        {
            if (floor == "parter")
                return 0;
            return int.Parse(floor);
        }

        private PropertyAddress CreatePropertyAddress(HtmlDocument page)
        {
            // Wygl¹da na to, ¿e gratka w jakiœ sposób zablokowa³a pobieranie adresu. Za ka¿dym razem odtrzymujê null - zarówno kiedy u¿ywam @class jak i @id
            var addressProperties = page.DocumentNode.SelectSingleNode("//*[@class=\"presentationMap__address\"]");
            var propertyAddress = new PropertyAddress(); 

            if (addressProperties != null)
            {
                // Metoda wywo³ywana w przypadku otrzymania danych na temat adresu
            }

            return propertyAddress;
        }

        private PropertyFeatures CreatePropertyFeatures(Dictionary<String, String> offerParameters)
        {
            var propertyFeatures = new PropertyFeatures();

            if (offerParameters.GetValueOrDefault("Balkon", null) != null)
            {
                propertyFeatures.Balconies = int.Parse(GetAllDigits(offerParameters.GetValueOrDefault("Balkon")));
            }

            if (offerParameters.GetValueOrDefault("Miejsce parkingowe", null) != null)
            {
                if (offerParameters.GetValueOrDefault("Miejsce parkingowe").Contains("przynale¿ne na ulicy") || offerParameters.GetValueOrDefault("Miejsce parkingowe").Contains("pod wiat¹"))
                {
                    if (offerParameters.GetValueOrDefault("Liczba miejsc parkingowych") != null)
                        propertyFeatures.OutdoorParkingPlaces = int.Parse(GetAllDigits(offerParameters.GetValueOrDefault("Liczba miejsc parkingowych")));
                    else
                        propertyFeatures.OutdoorParkingPlaces = 1;
                }
                    
                if (offerParameters.GetValueOrDefault("Miejsce parkingowe").Contains("w gara¿u") || offerParameters.GetValueOrDefault("Miejsce parkingowe").Contains("pod wiat¹"))
                {
                    if (offerParameters.GetValueOrDefault("Liczba miejsc parkingowych") != null)
                        propertyFeatures.IndoorParkingPlaces = int.Parse(GetAllDigits(offerParameters.GetValueOrDefault("Liczba miejsc parkingowych")));
                    else
                        propertyFeatures.IndoorParkingPlaces = 1;
                }
            }

            return propertyFeatures;
        }
    }

    public class GazetaKrakowskaOffer
    {
        public DateTime CreationDateTime { get; set; }
        public DateTime? LastUpdateDateTime { get; set; }
        public string UrlDetails { get; set; }
    }

    public static class ZipThreeListExtension
    {
        public static IEnumerable<TResult> ZipThree<T1, T2, T3, TResult>(
            this IEnumerable<T1> source,
            IEnumerable<T2> second,
            IEnumerable<T3> third,
            Func<T1, T2, T3, TResult> func)
        {
            using (var e1 = source.GetEnumerator())
            using (var e2 = second.GetEnumerator())
            using (var e3 = third.GetEnumerator())
            {
                while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext())
                    yield return func(e1.Current, e2.Current, e3.Current);
            }
        }
    }
}