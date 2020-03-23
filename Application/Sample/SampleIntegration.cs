using Interfaces;
using Models;
using System;
using System.Collections.Generic;

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
                Url = "http://sampleUrl.org",
                Name = "Sample WebSite Integration",
                WebPageFeatures = new WebPageFeatures
                {
                    HomeSale = false,
                    HomeRental = false,
                    HouseSale = false,
                    HouseRental = false
                }
            };
        }

        public Dump GenerateDump()
        {
            var random = new Random();
            var randomValue = random.Next() % 10;
            //Tutaj w normalnej sytuacji musimy ściągnąć dane z konkretnej strony, przeparsować je i dopiero wtedy zapisać do modelu Dump
            return new Dump
            {
                DateTime = DateTime.Now,
                WebPage = WebPage,
                Entries = new List<Entry>
                {
                    new Entry
                    {
                        OfferDetails = new OfferDetails
                        {
                            Url = $"{WebPage.Url}/{randomValue}",
                            CreationDateTime = DateTime.Now,
                            OfferKind = OfferKind.SALE,
                            SellerContact = new SellerContact
                            {
                                Email = "okazje@mieszkania.pl"
                            },
                            IsStillValid = true
                        },
                        RawDescription = "Kup Teraz!",
                    },
                    new Entry
                    {
                        OfferDetails = new OfferDetails
                        {
                            Url = $"{WebPage.Url}/{(randomValue+1)%10}",
                            CreationDateTime = DateTime.Now,
                            OfferKind = OfferKind.RENTAL,
                            SellerContact = new SellerContact
                            {
                                Email = "przeceny@mieszkania.pl"
                            },
                            IsStillValid = true
                        },
                        RawDescription = "NAPRAWDĘ WARTO!!",
                    }
                }
            };
        }
    }
}
