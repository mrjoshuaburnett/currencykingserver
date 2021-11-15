using CurrencyKing.Data.DatabaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyKing.Data.ViewModels
{
    public class PasswordResetModel : BaseViewModel
    {
        public Guid UserId { get; set; }

        public virtual User User { get; set; }

        public DateTime Expiry { get; set; }


    }
}
