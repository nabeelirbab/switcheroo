using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.OfferAnalytics
{
    public class OfferCountByType
    {
        public int CashOffers { get; set; }
        public int AcceptedCashOffers { get; set; }
        public int MatchedOffers { get; set; }
        public int UnMatchedOffers { get; set; }

    }
}
