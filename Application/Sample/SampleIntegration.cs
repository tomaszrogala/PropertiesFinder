using Interfaces;
using Models;
using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Linq;
using System.Threading;
using System.Linq.Expressions;

namespace Application.Sample
{
    public class SampleIntegration : IWebSiteIntegration
    {
        public WebPage WebPage { get; }
        public IDumpsRepository DumpsRepository { get; }

        public IEqualityComparer<Entry> EntriesComparer { get; }

        public SampleIntegration(IDumpsRepository dumpsRepository,
            IEqualityComparer<Entry> equalityComparer)
        {
            DumpsRepository = dumpsRepository;
            EntriesComparer = equalityComparer;
            WebPage = new WebPage
            {
                Url = @"http://www.bezposrednio.com/mieszkania,sprzedaz",
                Name = "Bezposrednio Integration",
                WebPageFeatures = new WebPageFeatures
                {
                    HomeSale = true,
                    HomeRental = false,
                    HouseSale = false,
                    HouseRental = false
                }
            };
        }

        public static string replacePolChars(String text)
        {
            var notPolish = text.Replace('Ą', 'A');
            notPolish = text.Replace('Ć', 'C');
            notPolish = text.Replace('Ę', 'E');
            notPolish = text.Replace('Ł', 'L');
            notPolish = text.Replace('Ó', 'O');
            notPolish = text.Replace('Ń', 'N');
            notPolish = text.Replace('Ś', 'S');
            notPolish = text.Replace('Ż', 'Z');
            notPolish = text.Replace('Ź', 'Z');
            notPolish = text.Replace('ą', 'a');
            notPolish = text.Replace('ć', 'c');
            notPolish = text.Replace('ę', 'e');
            notPolish = text.Replace('ł', 'l');
            notPolish = text.Replace('ó', 'o');
            notPolish = text.Replace('ń', 'n');
            notPolish = text.Replace('ś', 's');
            notPolish = text.Replace('ż', 'z');
            notPolish = text.Replace('ź', 'z');

            return notPolish;
        }


        public static List<Entry> entries;

        public static void scrap_data(string url, int i)
        {
            entries = new List<Entry>();
            HtmlWeb web = new HtmlWeb();

            var htmlDoc = web.Load(url);
            var OfferUrlList = htmlDoc.DocumentNode.SelectNodes("//div[@class='tytul']");
            
            var entry = new Entry();

            foreach (var u in OfferUrlList)
            {
                var offerUrl = u.SelectSingleNode(".//a[@href]");
                Console.WriteLine(offerUrl.Attributes["href"].Value);
                htmlDoc = web.Load(offerUrl.Attributes["href"].Value);
                //Kontakt jest w postaci obrazka ( brak możliwości wykorzystania OCR )
                //var sellerContact = htmlDoc.DocumentNode.SelectNodes("//div[@class='tytul']");
                //var creationTime = htmlDoc.DocumentNode.SelectNodes("//div[@class='mediumBarRed']");
                //...regex search date


                var offerInfo = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='OfferInfo']");
                var offerInfos = offerInfo.SelectNodes(".//span[@class='fontGrafit font14 fontBold']");
                var offerInfoTitles = offerInfo.SelectNodes(".//span[@class='fontRed fontBold']");
                var zip = offerInfoTitles.Zip(offerInfos, (t, o) => new { t, o });
                
                var offerDetails = new OfferDetails();
                offerDetails.Url = offerUrl.Attributes["href"].Value;
                offerDetails.OfferKind = OfferKind.SALE;
                offerDetails.CreationDateTime = DateTime.Now;
                offerDetails.IsStillValid = true;

                var propertyDetails = new PropertyDetails();
                var propertyAddress = new PropertyAddress();
                var propertyPrice = new PropertyPrice();

                foreach (var z in zip)
                {
                    if (z.t.InnerHtml.Contains("Lokalizacja"))
                    {
                        //Console.WriteLine(z.o.InnerHtml);
                        string[] separator = { "<br>", " " };
                        string[] temp = z.o.InnerHtml.Split(separator, 3, StringSplitOptions.RemoveEmptyEntries);
                        if (temp.Count() > 2)
                        {
                            try
                            {
                                string translated = replacePolChars(temp[0].ToUpper());
                                propertyAddress.City = (PolishCity)Enum.Parse(typeof(PolishCity), translated);
                            }
                            catch
                            {
                                Console.WriteLine(temp[0] + " is not a Polish City!");
                            }
                            propertyAddress.District = temp[1];
                            propertyAddress.StreetName = temp[2];
                        }
                        else if (temp.Count() > 2)
                        {
                            string translated = replacePolChars(temp[0].ToUpper());
                            propertyAddress.City = (PolishCity)Enum.Parse(typeof(PolishCity), translated);
                            propertyAddress.StreetName = temp[1];
                        }

                    }
                    else if (z.t.InnerHtml.Contains("Metra"))
                    {
                        //Console.WriteLine(z.o.InnerHtml);
                        string[] separator = { "/" };
                        string[] temp = z.o.InnerHtml.Split(separator, 3, StringSplitOptions.RemoveEmptyEntries);
                        float meter = float.Parse(temp[0].Replace("m&#178;", ""));
                        //int rooms = Int32.Parse(temp[1].Replace("m&", ""));
                        Console.WriteLine(temp[1].Replace(" ", "").Replace("pok.", ""));
                        propertyDetails.Area = Convert.ToInt32(meter);
                        propertyDetails.NumberOfRooms = Int32.Parse(temp[1].Replace(" ", "").Replace("pok.", ""));

                    }
                    else if (z.t.InnerHtml.Contains("tro"))
                    {
                        Console.WriteLine(z.o.InnerHtml);
                        string[] separator = { "/" };
                        string[] temp = z.o.InnerHtml.Split(separator, 2, StringSplitOptions.RemoveEmptyEntries);
                        if (temp.Count() > 1)
                        {
                            propertyDetails.FloorNumber = Int32.Parse(temp[0]);
                        }
                        else if (temp.Count() <= 1)
                        {
                            propertyDetails.FloorNumber = Int32.Parse(temp[0].Replace("p", ""));
                        }
                    }
                    else if (z.t.InnerHtml.Contains("Cena"))
                    {
                        
                        string[] separator = { "," };
                        string[] temp = z.o.InnerHtml.Split(separator, 2, StringSplitOptions.RemoveEmptyEntries);
                        string newStr = temp[0].Replace(".", "");
                        string price = newStr.Substring(0,newStr.Count() - 3);
                        
                        propertyPrice.TotalGrossPrice = Int32.Parse(price);

                        try
                        {
                            string newStr2 = temp[1].Replace(".", "");
                            string pricePerMeter = newStr2.Substring(0, newStr2.Count() - 11);

                            propertyPrice.PricePerMeter = Int32.Parse(pricePerMeter);
                            Console.WriteLine(propertyPrice.PricePerMeter);
                        }
                        catch {
                            Console.WriteLine("No price per meter visible!");
                        }
                    }

                    entry = new Entry
                    {
                        OfferDetails = offerDetails,
                        PropertyDetails = propertyDetails,
                        PropertyAddress = propertyAddress,
                        PropertyPrice = propertyPrice,

                        RawDescription = "Kup Teraz!",
                    };

                    entries.Add(entry);
                }

            }

            i++;
            if (i < 3)
            {
                string html = @"http://www.bezposrednio.com/mieszkania,sprzedaz," + i.ToString();
                scrap_data(html, i);
            }
        }

        public Dump GenerateDump()
        {
            var random = new Random();
            var randomValue = random.Next() % 10;
            //Tutaj w normalnej sytuacji musimy ściągnąć dane z konkretnej strony, przeparsować je i dopiero wtedy zapisać do modelu Dump

            scrap_data(WebPage.Url,0);
            //var dump = new Dump();
            //dump.DateTime = DateTime.Now;
            //dump.WebPage = WebPage;
            //dump.Entries = entries;

            //return dump;

            return new Dump
            {
                DateTime = DateTime.Now,
                WebPage = WebPage,

                Entries = entries,

            };
        }
    }
}
