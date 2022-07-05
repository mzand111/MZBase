using System;
using System.Collections.Generic;
using System.Text;

namespace MZBase.Domain
{
    public class Auditable<PrimaryKey> : IAuditable<PrimaryKey>
        where PrimaryKey : struct
    {
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set ; }
        public DateTime CreationTime { get; set; }
        public DateTime LastModificationTime { get; set; }
        public PrimaryKey ID { get; set; }
    }
}
