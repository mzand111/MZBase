using MZBase.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace MZBase.Test.Common
{
    public class GeneralTestDateTimeProvider : IDateTimeProviderService
    {
        public DateTime GetNow()
        {
            return DateTime.Now;
        }
    }
}
