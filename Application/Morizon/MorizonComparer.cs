using Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Application.Classes {
    public class MorizonComparer : IEqualityComparer<Entry> {
        public bool Equals(Entry x, Entry y) {
            if ( x.OfferDetails.OfferKind.Equals(y.OfferDetails.OfferKind) ) {
                if ( x.PropertyPrice.Equals(y.PropertyPrice) )
                    if ( x.PropertyDetails.Equals(y.PropertyDetails) )
                        if ( x.PropertyAddress.Equals(y.PropertyAddress) )
                            if ( x.PropertyFeatures.Equals(y.PropertyFeatures) )
                                return true;
            }
            return false;
        }


        public int GetHashCode([DisallowNull] Entry obj) {
            return obj.OfferDetails.Url == null ? 0 : obj.OfferDetails.Url.GetHashCode();
        }
    }
}
