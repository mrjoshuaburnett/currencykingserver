using AutoMapper;
using CurrencyKing.Data.DatabaseModels;
using CurrencyKing.Data.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CurrencyKing.Services
{
    public abstract class BaseService<T, ViewModel>
        where T : BaseDbModel, new()
        where ViewModel : BaseViewModel, new()
    {
        public readonly IMapper _mapper;

        private readonly string _name;

        protected BaseService(IMapper mapper)
        {
            _name = typeof(T).Name;
            _mapper = mapper;
        }

        public IQueryable<T> Get(DatabaseContext context)
        {
            return context.Set<T>().AsQueryable();
        }

        public IQueryable<T> Get(DatabaseContext context, Guid Id)
        {
            return Get(context).Where(x => x.Id == Id).AsQueryable();
        }

        public async Task<T> GetConcrete(DatabaseContext context, Guid Id)
        {
            return await Get(context).Where(x => x.Id == Id).SingleOrDefaultAsync();
        }

        public virtual async Task Delete(DatabaseContext context, Guid Id)
        {
            var entity = await GetConcrete(context, Id);

            entity.IsDeleted = true;
        }


        public async Task<T> CreateOrUpdate(DatabaseContext context, ViewModel model)
        {

            var entity = new T();
            var isNew = !model.Id.HasValue;
            if (isNew)
            {
                entity = _mapper.Map<T>(model);
                await context.AddAsync(entity);
            }
            else
            {
                entity = await GetConcrete(context, model.Id.Value);

                _mapper.Map(model, entity);

            }

            return entity;
        }

    }
}
