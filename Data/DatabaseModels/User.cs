using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyKing.Data.DatabaseModels
{
    public class User : BaseDbModel
    {
        public string FullName { get; set; }

        public string EmailAddress { get; set; }

        public string PasswordHash { get; set; }

        public string PasswordSalt { get; set; }

        public string Role { get; set; }

        public string RefreshToken { get; set; }

        public bool UserVerified { get; set; }

        public virtual ICollection<UserExchange> UserConversions { get; set; }

        public virtual ICollection<PasswordReset> PasswordResets { get; set; }
    }
}
