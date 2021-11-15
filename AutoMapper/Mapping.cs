using AutoMapper;
using CurrencyKing.Data.DatabaseModels;
using CurrencyKing.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyKing.AutoMapper
{
    public class AutoMapping : Profile
    {

        public AutoMapping()
        {
            //User Exchange Rates
            CreateMap<UserExchangeModel, UserExchange>()
                .ForMember(e => e.CreatedDate, m => m.Ignore());
            CreateMap<UserExchange, UserExchangeModel>()
                .ForMember(m => m.CreatedDate, m => m.MapFrom(e => e.CreatedDate.ToString("HH:mm dd/MM/yyyy")));

            //User
            CreateMap<UserModel, User>();
            CreateMap<User, UserModel>();

            //Password Reset
            CreateMap<PasswordResetModel, PasswordReset>()
                .ForMember(a => a.CreatedDate, b => b.Ignore())
                .ForMember(a => a.ModifiedDate, b => b.Ignore())
                .ForMember(a => a.IsDeleted, b => b.Ignore());
            CreateMap<PasswordReset, PasswordResetModel>();
        }
    }
}
