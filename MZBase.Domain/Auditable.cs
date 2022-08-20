using System;
using System.Collections.Generic;
using System.Text;

namespace MZBase.Domain
{
    public class Auditable<PrimaryKey> : Model<PrimaryKey>,IAuditable<PrimaryKey>
        where PrimaryKey : struct
    {
        public virtual string CreatedBy { get; set; }
        public virtual string LastModifiedBy { get; set ; }
        public virtual DateTime CreationTime { get; set; }
        public virtual DateTime LastModificationTime { get; set; }
        public virtual PrimaryKey ID { get; set; }
    }
}
