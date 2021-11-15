using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyKing.Data.DatabaseModels
{
    public class UserExchange : BaseDbModel
    {
        public string BaseCurrency { get; set; }

        public string TargetCurrency { get; set; }

        public decimal Amount { get; set; }

        public decimal ExchangeRate { get; set; }

        public Guid UserId { get; set; }

        public virtual User User { get; set; }

    }
}
