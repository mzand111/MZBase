using System;
using System.Collections.Generic;
using System.Text;

namespace MZBase.Infrastructure
{
    public interface IDateTimeProviderService
    {
        DateTime GetNow();
    }
}
