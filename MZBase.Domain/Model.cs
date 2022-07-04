using System;
using System.Collections.Generic;
using System.Text;

namespace MZBase.Domain
{
    public class Model<PrimaryKey> : IModel<PrimaryKey>
       where PrimaryKey : struct
    {
        public virtual PrimaryKey ID { get; set; }
    }
}
