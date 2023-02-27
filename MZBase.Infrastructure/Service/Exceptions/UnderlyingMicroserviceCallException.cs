using System;
using System.Collections.Generic;
using System.Text;

namespace MZBase.Infrastructure.Service.Exceptions
{
    public class UnderlyingMicroserviceCallException : ServiceException
    {
        public UnderlyingMicroserviceCallException(string message, Exception innerException) : base(message, innerException, 6)
        {

        }
    }
}
