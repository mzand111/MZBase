using System;
using System.Collections.Generic;
using System.Text;

namespace MZBase.Infrastructure.Service.Exceptions
{
    public class ServiceModelValidationException : ServiceException
    {
        public string JSONFormattedErrors
        {
            get
            {
                string messages = "";
                foreach (var item in ValidationErrors)
                {
                    messages += item.JSONStringFormat + ",";
                }
                if (!string.IsNullOrWhiteSpace(messages))
                {
                    messages += "++";
                    messages = messages.Replace(",++", "");
                }
                return messages;
            }
        }
        public IEnumerable<ModelFieldValidationResult> ValidationErrors { get; private set; }
        public ServiceModelValidationException(IEnumerable<ModelFieldValidationResult> validationErrors) : base()
        {
            ValidationErrors = validationErrors;
            _code = 4;
        }
        public ServiceModelValidationException(IEnumerable<ModelFieldValidationResult> validationErrors, string message) : base(message)
        {
            ValidationErrors = validationErrors;
            _code = 4;
        }
    }
}
