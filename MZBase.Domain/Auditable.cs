using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MZBase.Domain
{
    public class Auditable<PrimaryKey> : Model<PrimaryKey>, IAuditable<PrimaryKey>
        where PrimaryKey : struct
    {
        [Column(Order = 100)]
        public virtual string CreatedBy { get; set; }
        [Column(Order = 101)]
        public virtual string LastModifiedBy { get; set; }
        [Column(Order = 102)]
        public virtual DateTime CreationTime { get; set; }
        [Column(Order = 103)]
        public virtual DateTime LastModificationTime { get; set; }

        public void SetAuditablesFrom<T>(T t)
            where T : Auditable<PrimaryKey>
        {
            CreatedBy = t.CreatedBy;
            CreationTime = t.CreationTime;
            LastModificationTime = t.LastModificationTime;
            LastModifiedBy = t.LastModifiedBy;
        }

    }
}
