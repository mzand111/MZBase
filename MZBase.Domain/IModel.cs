using System;
using System.Collections.Generic;
using System.Text;

namespace MZBase.Domain
{
    public interface IModel<PrimaryKey> where PrimaryKey : struct
    {
        PrimaryKey ID { get; set; }
    }
}
