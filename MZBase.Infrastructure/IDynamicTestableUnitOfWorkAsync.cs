using MZBase.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace MZBase.Infrastructure
{
    public interface IDynamicTestableUnitOfWorkAsync:IUnitOfWorkAsync
    {
        ILDRCompatibleRepositoryAsync<T, PrimKey> GetRepo<T, PrimKey>()
            where T : Model<PrimKey>
           where PrimKey : struct;
    }
}
