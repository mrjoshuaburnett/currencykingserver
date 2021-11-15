using CurrencyKing.Data.DatabaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyKing.Data.ViewModels
{
    public class UserExchangeModel : BaseViewModel
    {
        public string BaseCurrency { get; set; }

        public string TargetCurrency { get; set; }

        public decimal ExchangeRate { get; set; }

        public decimal Amount { get; set; }

        public Guid? UserId { get; set; }

        public decimal ConvertedValue
        {
            get
            {
                return ExchangeRate * Amount;
            }
        }

        public string CreatedDate { get; set; }

    }
}
