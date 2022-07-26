using Microsoft.EntityFrameworkCore;
using MZBase.Domain;
using MZBase.Infrastructure;
using MZSimpleDynamicLinq.Core;
using MZSimpleDynamicLinq.EFCoreExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MZBase.EntityFrameworkCore
{
    public class LDRCompatibleRepositoryAsync<DBModelEntity,DomainModelEntity, PrimaryKeyType> : RepositoryAsync<DBModelEntity, DomainModelEntity, PrimaryKeyType>, ILDRCompatibleRepositoryAsync<DomainModelEntity, PrimaryKeyType>
        where DomainModelEntity : Model<PrimaryKeyType>
          where DBModelEntity :  DomainModelEntity
        where PrimaryKeyType : struct
    {
        public LDRCompatibleRepositoryAsync(DbContext context):base(context)
        {

        }
        public async Task<LinqDataResult<DomainModelEntity>> AllItemsAsync(LinqDataRequest request)
        {
            return await _context.Set<DBModelEntity>().ToLinqDataResultAsync< DomainModelEntity>(request.Take, request.Skip, request.Sort, request.Filter);
        }
    }
}
