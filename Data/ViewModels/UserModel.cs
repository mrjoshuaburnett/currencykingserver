using CurrencyKing.Data.DatabaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyKing.Data.ViewModels
{
    public class UserModel : BaseViewModel
    {
        public string FullName { get; set; }

        public string EmailAddress { get; set; }

        public string Role { get; set; }


    }
}
