using MZBase.Domain;
using MZSimpleDynamicLinq.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MZBase.Infrastructure.Service
{
    /// <summary>
    /// This interface is used to shape bussiness services and their relation whith the sorages like databases
    /// Implementors are expected to use IUnitOfWork structures and bussiness rules alongside to do the job
    /// Any exception from this class should inherete MZBase.Infrastructure.Service.Exceptions.ServiceException, else it will be considered un-handled
    /// </summary>
    /// <typeparam name="T">The base entity this bussiness service is handeling</typeparam>
    /// <typeparam name="PrimarykeyType">The primary key of the base entity</typeparam>
    public interface IStorageBusinessService<T, PrimarykeyType>
        where T : IModel<PrimarykeyType>
        where PrimarykeyType : struct
    {


        public int LogBaseID { get; }
        /// <summary>
        /// Adds an instance of the entity to the database doing all validations and exception handlings.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<PrimarykeyType> AddAsync(T item);
        Task<T> RetrieveByIdAsync(PrimarykeyType ID);
        Task ModifyAsync(T item);
        Task RemoveByIdAsync(PrimarykeyType ID);
        Task<LinqDataResult<T>> ItemsAsync(LinqDataRequest request);
    }
}
