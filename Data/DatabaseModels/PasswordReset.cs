using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyKing.Data.DatabaseModels
{
    public class PasswordReset : BaseDbModel
    {
        public Guid UserId { get; set; }

        public virtual User User { get; set; }

        public DateTime Expiry { get; set; }
    }
}
