using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Models;

namespace Application.GazetaKrakowska
{
    class GazetaKrakowskaComparer : IEqualityComparer<Entry>
    {
        public bool Equals(Entry x, Entry y)
        {
            return OfferDetailsComparer.Equals(x.OfferDetails, y.OfferDetails)
                && PropertyPriceComparer.Equals(x.PropertyPrice, y.PropertyPrice)
                && PropertyDetailsComparer.Equals(x.PropertyDetails, y.PropertyDetails)
                && PropertyAddressComparer.Equals(x.PropertyAddress, y.PropertyAddress)
                && PropertyFeaturesComparer.Equals(x.PropertyAddress, y.PropertyAddress)
                && x.RawDescription.Equals(y.RawDescription);
        }

        public int GetHashCode([DisallowNull] Entry obj)
        {
            return (OfferDetailsComparer.GetHashCode(obj.OfferDetails))
                + (PropertyPriceComparer.GetHashCode(obj.PropertyPrice))
                + (PropertyDetailsComparer.GetHashCode(obj.PropertyDetails))
                + (PropertyAddressComparer.GetHashCode(obj.PropertyAddress))
                + (PropertyFeaturesComparer.GetHashCode(obj.PropertyFeatures))
                + (obj.RawDescription == null ? 0 : obj.RawDescription.GetHashCode());
        }
    }

    class OfferDetailsComparer
    {
        public static bool Equals(OfferDetails x, OfferDetails y)
        {
            return x.Url.Equals(y.Url)
                && x.CreationDateTime.Equals(y.CreationDateTime)
                && x.LastUpdateDateTime.Equals(y.LastUpdateDateTime)
                && x.OfferKind.Equals(y.OfferKind)
                && SellerContactComparer.Equals(x.SellerContact, y.SellerContact)
                && x.IsStillValid.Equals(y.IsStillValid);
        }

        public static int GetHashCode([DisallowNull] OfferDetails obj)
        {
            return (obj.Url == null ? 0 : obj.Url.GetHashCode())
                + (obj.CreationDateTime == null ? 0 : obj.CreationDateTime.GetHashCode())
                + (obj.LastUpdateDateTime == null ? 0 : obj.LastUpdateDateTime.GetHashCode())
                + (obj.OfferKind.GetHashCode())
                + (obj.SellerContact == null ? 0 : SellerContactComparer.GetHashCode(obj.SellerContact))
                + (obj.IsStillValid.GetHashCode());
        }
    }

    class SellerContactComparer
    {
        public static bool Equals(SellerContact x, SellerContact y)
        {
            return x.Email.Equals(y.Email)
                && x.Telephone.Equals(y.Telephone)
                && x.Name.Equals(y.Name);
        }

        public static int GetHashCode([DisallowNull] SellerContact obj)
        {
            return (obj.Email == null ? 0 : obj.Email.GetHashCode())
                + (obj.Telephone == null ? 0 : obj.Telephone.GetHashCode())
                + (obj.Name == null ? 0 : obj.Name.GetHashCode());
        }
    }

    class PropertyPriceComparer
    {
        public static bool Equals(PropertyPrice x, PropertyPrice y)
        {
            return x.PricePerMeter.Equals(y.PricePerMeter)
                && x.TotalGrossPrice.Equals(y.TotalGrossPrice)
                && x.ResidentalRent.Equals(y.ResidentalRent);
        }

        public static int GetHashCode([DisallowNull] PropertyPrice obj)
        {
            return (obj.PricePerMeter.GetHashCode())
                + (obj.TotalGrossPrice.GetHashCode())
                + (obj.ResidentalRent == null ? 0 : obj.ResidentalRent.GetHashCode());
        }
    }

    class PropertyDetailsComparer
    {
        public static bool Equals(PropertyDetails x, PropertyDetails y)
        {
            return x.Area.Equals(y.Area)
                && x.NumberOfRooms.Equals(y.NumberOfRooms)
                && x.FloorNumber.Equals(y.FloorNumber)
                && x.YearOfConstruction.Equals(y.YearOfConstruction);
        }

        public static int GetHashCode([DisallowNull] PropertyDetails obj)
        {
            return obj.Area.GetHashCode()
                + obj.NumberOfRooms.GetHashCode()
                + (obj.FloorNumber == null ? 0 : obj.FloorNumber.GetHashCode())
                + (obj.YearOfConstruction == null ? 0 : obj.YearOfConstruction.GetHashCode());
        }
    }

    class PropertyAddressComparer
    {
        public static bool Equals(PropertyAddress x, PropertyAddress y)
        {
            return x.City.Equals(y.City)
                && x.District.Equals(y.District)
                && x.StreetName.Equals(y.StreetName)
                && x.DetailedAddress.Equals(y.DetailedAddress);
        }

        public static int GetHashCode([DisallowNull] PropertyAddress obj)
        {
            return obj.City.GetHashCode()
                + (obj.District == null ? 0 : obj.District.GetHashCode())
                + (obj.StreetName == null ? 0 : obj.StreetName.GetHashCode())
                + (obj.DetailedAddress == null ? 0 : obj.DetailedAddress.GetHashCode());
        }
    }

    class PropertyFeaturesComparer
    {
        public static bool Equals(PropertyFeatures x, PropertyFeatures y)
        {
            return x.GardenArea.Equals(y.GardenArea)
                && x.Balconies.Equals(y.Balconies)
                && x.BasementArea.Equals(y.BasementArea)
                && x.IndoorParkingPlaces.Equals(y.IndoorParkingPlaces)
                && x.OutdoorParkingPlaces.Equals(y.OutdoorParkingPlaces);
        }

        public static int GetHashCode([DisallowNull] PropertyFeatures obj)
        {
            return (obj.GardenArea == null ? 0 : obj.GardenArea.GetHashCode())
                + (obj.Balconies == null ? 0 : obj.Balconies.GetHashCode())
                + (obj.BasementArea == null ? 0 : obj.BasementArea.GetHashCode())
                + (obj.IndoorParkingPlaces == null ? 0 : obj.IndoorParkingPlaces.GetHashCode())
                + (obj.OutdoorParkingPlaces == null ? 0 : obj.OutdoorParkingPlaces.GetHashCode());
        }
    }
}
