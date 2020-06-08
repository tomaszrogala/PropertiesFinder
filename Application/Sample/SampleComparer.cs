using Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Application.Sample
{
    public class SampleComparer : IEqualityComparer<Entry>
    {
        public bool Equals(Entry x, Entry y)
        {
            if (GetHashCode(x) == GetHashCode(y)) {
                return true;
            }
            else {
                return false;
            }
        }

        public int GetHashCode([DisallowNull] Entry obj)
        {
            var hashCode = $"{obj.PropertyDetails.NumberOfRooms}" +
                $"{obj.PropertyAddress.City}" +
                $"{obj.PropertyDetails.Area}" +
                $"{obj.OfferDetails.Url}" +
                $"{obj.PropertyPrice.TotalGrossPrice}";
            return hashCode.GetHashCode();
        }
    }
}
