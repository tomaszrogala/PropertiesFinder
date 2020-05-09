using Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Application.Sample
{
    public class SampleComparer : IEqualityComparer<Entry>
    {
        public bool Equals(Entry x, Entry y)
        {
            if (x.OfferDetails.Url.Equals(y.OfferDetails.Url))
                return true;
            return false;
        }

        public int GetHashCode([DisallowNull] Entry obj)
        {
            return obj.OfferDetails.Url == null ? 0 : obj.OfferDetails.Url.GetHashCode();
        }
    }
}
