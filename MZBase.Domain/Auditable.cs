using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MZBase.Domain
{
    public class Auditable<PrimaryKey> : Model<PrimaryKey>,IAuditable<PrimaryKey>
        where PrimaryKey : struct
    {
        [Column(Order = 100)]
        public virtual string CreatedBy { get; set; }
        [Column(Order = 101)]
        public virtual string LastModifiedBy { get; set ; }
        [Column(Order = 102)]
        public virtual DateTime CreationTime { get; set; }
        [Column(Order = 103)]
        public virtual DateTime LastModificationTime { get; set; }
        [Column(Order =0)]
        public virtual PrimaryKey ID { get; set; }
    }
}
