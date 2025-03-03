using System;

namespace MZBase.Domain
{
    public interface IAuditable<PrimaryKey> : IModel<PrimaryKey>
        where PrimaryKey : struct
    {
        string CreatedBy { get; set; }
        string LastModifiedBy { get; set; }
        DateTime CreationTime { get; set; }
        DateTime LastModificationTime { get; set; }
    }
}