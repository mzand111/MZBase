using Microsoft.EntityFrameworkCore;
using MZBase.Domain;
using MZBase.Infrastructure;
using MZSimpleDynamicLinq.Core;
using MZSimpleDynamicLinq.EFCoreExtensions;

namespace MZBase.EntityFrameworkCore
{
    /// <summary>
    /// Provides an asynchronous repository implementation that is compatible with LinqDataRequest handling.
    /// </summary>
    /// <typeparam name="DBModelEntity">The type of the database model entity.</typeparam>
    /// <typeparam name="DomainModelEntity">The type of the domain model entity.</typeparam>
    /// <typeparam name="PrimaryKeyType">The type of the primary key.</typeparam>
    public class LDRCompatibleRepositoryAsync<DomainModelEntity, DBModelEntity, PrimaryKeyType> : BaseRepositoryAsync<DomainModelEntity, DBModelEntity, PrimaryKeyType>, ILDRCompatibleRepositoryAsync<DomainModelEntity, DBModelEntity, PrimaryKeyType>
        where DomainModelEntity : Model<PrimaryKeyType>
        where DBModelEntity : DomainModelEntity, IConvertibleDBModelEntity<DomainModelEntity>, new()
        where PrimaryKeyType : struct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LDRCompatibleRepositoryAsync{DBModelEntity, DomainModelEntity, PrimaryKeyType}"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public LDRCompatibleRepositoryAsync(DbContext context) : base(context)
        {
        }

        /// <summary>
        /// Gets all items asynchronously based on the specified LinqDataRequest.
        /// </summary>
        /// <param name="request">The LinqDataRequest containing the query parameters.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the LinqDataResult of the domain model entities.</returns>
        public virtual async Task<LinqDataResult<DomainModelEntity>> AllItemsAsync(LinqDataRequest request)
        {
            return await _context.Set<DBModelEntity>().ToLinqDataResultAsync<DomainModelEntity>(request.Take, request.Skip, request.Sort, request.Filter);
        }
    }
}
