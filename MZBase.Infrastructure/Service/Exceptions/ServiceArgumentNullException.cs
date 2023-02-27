using System;
using System.Collections.Generic;
using System.Text;

namespace MZBase.Infrastructure.Service.Exceptions
{
    public class ServiceArgumentNullException : ServiceException
    {
        public ServiceArgumentNullException(string message, Exception innerException) : base(message, innerException, 2)
        {

        }
        public ServiceArgumentNullException(string message) : base(message)
        {
            _code = 2;
        }
    }
}
