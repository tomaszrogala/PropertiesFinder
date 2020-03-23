using Models;
using System.Collections;
using System.Collections.Generic;

namespace Interfaces
{
    public interface IWebSiteIntegration : IDumpable
    {
        public WebPage WebPage { get; }
        public IDumpsRepository DumpsRepository { get; }
        public IEqualityComparer<Entry> EntriesComparer { get; }
    }
}
