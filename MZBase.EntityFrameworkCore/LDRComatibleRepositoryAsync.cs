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
    public class LDRCompatibleRepositoryAsync<ModelEntity, PrimaryKeyType> : RepositoryAsync<ModelEntity, PrimaryKeyType>, ILDRCompatibleRepositoryAsync<ModelEntity, PrimaryKeyType>
          where ModelEntity : Model<PrimaryKeyType>
        where PrimaryKeyType : struct
    {
        public LDRCompatibleRepositoryAsync(DbContext context):base(context)
        {

        }
        public async Task<LinqDataResult<ModelEntity>> AllItemsAsync(LinqDataRequest request)
        {
            return await _context.Set<ModelEntity>().ToLinqDataResultAsync(request.Take, request.Skip, request.Sort, request.Filter);
        }
    }
}
