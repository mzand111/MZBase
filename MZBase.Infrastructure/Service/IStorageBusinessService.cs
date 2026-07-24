using MZBase.Domain;
using MZSimpleDynamicLinq.Core;
using System.Threading.Tasks;

namespace MZBase.Infrastructure.Service
{
    /// <summary>
    /// This interface is used to shape business services and their relation with the storages like databases.
    /// Implementors are expected to use IUnitOfWork structures and business rules alongside to do the job
    /// Any exception from this class should inherit MZBase.Infrastructure.Service.Exceptions.ServiceException, else it will be considered un-handled
    /// </summary>
    /// <typeparam name="DomainModel">The base entity this business service is handling</typeparam>a
    /// <typeparam name="PrimaryKeyType">The primary key of the base entity</typeparam>
    public interface IStorageBusinessService<DomainModel, PrimaryKeyType>
        where DomainModel : Model<PrimaryKeyType>
        where PrimaryKeyType : struct
    {
        public int LogBaseID { get; }
        /// <summary>
        /// Adds an instance of the entity to the database doing all validations and exception handlings.
        /// </summary>
        /// <param name="item">The entity instance to be added.</param>
        /// <returns>The primary key of the added entity.</returns>
        Task<PrimaryKeyType> AddAsync(DomainModel item);

        /// <summary>
        /// Retrieves an entity instance by its primary key.
        /// </summary>
        /// <param name="ID">The primary key of the entity to be retrieved.</param>
        /// <returns>The entity instance if found, otherwise null.</returns>
        Task<DomainModel> RetrieveByIdAsync(PrimaryKeyType ID);

        /// <summary>
        /// Modifies an existing entity instance in the database.
        /// </summary>
        /// <param name="item">The entity instance with updated values.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ModifyAsync(DomainModel item);

        /// <summary>
        /// Removes an entity instance from the database by its primary key.
        /// </summary>
        /// <param name="ID">The primary key of the entity to be removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveByIdAsync(PrimaryKeyType ID);

        /// <summary>
        /// Retrieves a collection of entity instances based on the specified Linq data request.
        /// </summary>
        /// <param name="request">The Linq data request containing query parameters.</param>
        /// <returns>A LinqDataResult containing the collection of entity instances.</returns>
        Task<LinqDataResult<DomainModel>> ItemsAsync(LinqDataRequest request);
    }
}
