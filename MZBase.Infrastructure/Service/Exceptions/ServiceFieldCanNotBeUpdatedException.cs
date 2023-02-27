using System;
using System.Collections.Generic;
using System.Text;

namespace MZBase.Infrastructure.Service.Exceptions
{
    public class ServiceFieldCanNotBeUpdatedException : ServiceException
    {
        public string FieldName { get; private set; }
        public string ModelName { get; private set; }
        public ServiceFieldCanNotBeUpdatedException(string modelName, string fieldName)
        {
            FieldName = fieldName;
            ModelName = modelName;
            _code = 3;
        }
        public ServiceFieldCanNotBeUpdatedException(string modelName, string fieldName, string message) : base(message)
        {
            FieldName = fieldName;
            ModelName = modelName;
            _code = 3;
        }
    }
}
