using Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;

namespace Application.Sample
{
    public class BazosComparer : IEqualityComparer<Entry>
    {
        public bool Equals(Entry x, Entry y)
        {
            //Porownywanie informacji - jeżeli 90% informacji jest zgodnych, uznaję ofertę za tą samą
            int a = 0;

            a = CompareOfferDetails(x, y, a);

            a = ComparePropertyPrice(x, y, a);

            a = ComparePropertyDetails(x, y, a);

            a = ComparePropertyAddress(x, y, a);

            a = ComparePropertyFeatures(x, y, a);

            float b = a / 19f;
            if(b>0.90)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static int ComparePropertyFeatures(Entry x, Entry y, int a)
        {
            if (x.PropertyFeatures.Balconies == y.PropertyFeatures.Balconies)
                a++;
            if (x.PropertyFeatures.BasementArea == y.PropertyFeatures.BasementArea)
                a++;
            if (x.PropertyFeatures.GardenArea == y.PropertyFeatures.GardenArea)
                a++;
            if (x.PropertyFeatures.IndoorParkingPlaces == y.PropertyFeatures.IndoorParkingPlaces)
                a++;
            if (x.PropertyFeatures.OutdoorParkingPlaces == y.PropertyFeatures.OutdoorParkingPlaces)
                a++;
            return a;
        }

        private static int ComparePropertyAddress(Entry x, Entry y, int a)
        {
            if (x.PropertyAddress.City == y.PropertyAddress.City)
                a++;
            if (x.PropertyAddress.DetailedAddress == y.PropertyAddress.DetailedAddress)
                a++;
            if (x.PropertyAddress.District == y.PropertyAddress.District)
                a++;
            if (x.PropertyAddress.StreetName == y.PropertyAddress.StreetName)
                a++;
            return a;
        }

        private static int CompareOfferDetails(Entry x, Entry y, int a)
        {
            if (x.OfferDetails.Url == y.OfferDetails.Url)
                a++;
            if (x.OfferDetails.OfferKind == y.OfferDetails.OfferKind)
                a++;
            a = CompareSellerContact(x, y, a);
            return a;
        }

        private static int ComparePropertyDetails(Entry x, Entry y, int a)
        {
            if (x.PropertyDetails.Area == y.PropertyDetails.Area)
                a++;
            if (x.PropertyDetails.FloorNumber == y.PropertyDetails.FloorNumber)
                a++;
            if (x.PropertyDetails.NumberOfRooms == y.PropertyDetails.NumberOfRooms)
                a++;
            if (x.PropertyDetails.YearOfConstruction == y.PropertyDetails.YearOfConstruction)
                a++;
            return a;
        }

        private static int ComparePropertyPrice(Entry x, Entry y, int a)
        {
            if (x.PropertyPrice.TotalGrossPrice == y.PropertyPrice.TotalGrossPrice)
                a++;
            return a;
        }

        private static int CompareSellerContact(Entry x, Entry y, int a)
        {
            var c = x.OfferDetails.SellerContact.Name;
            if (x.OfferDetails.SellerContact.Name == y.OfferDetails.SellerContact.Name)
                a++;
            if (x.OfferDetails.SellerContact.Telephone == y.OfferDetails.SellerContact.Telephone)
                a++;
            if (x.OfferDetails.SellerContact.Email == y.OfferDetails.SellerContact.Email)
                a++;
            return a;
        }

        public int GetHashCode([DisallowNull] Entry obj)
        {
            int hashCode = obj.PropertyAddress.City.GetHashCode() * obj.PropertyPrice.TotalGrossPrice.GetHashCode() ^ obj.OfferDetails.SellerContact.Name.GetHashCode();
            return hashCode;
        }
    }
}
