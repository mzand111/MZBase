using System;

namespace MZBase.Infrastructure.Service.Exceptions
{
    public class ServiceStorageException : ServiceException
    {
        public ServiceStorageException(string message, Exception innerException) : base(message, innerException, 1)
        {

        }
    }
}
