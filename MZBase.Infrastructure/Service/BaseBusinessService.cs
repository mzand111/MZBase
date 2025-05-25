using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace MZBase.Infrastructure.Service
{
    /// <summary>
    /// An abstract base class designed to be used as the root of every business service class . 
    /// It  also provide logging functionality for derived business service classes.
    /// </summary>
    /// <typeparam name="LogCategory"></typeparam>
    public abstract class BaseBusinessService<LogCategory>
    {
        protected readonly ILogger<LogCategory> _logger;

        public BaseBusinessService(ILogger<LogCategory> logger)
        {
            _logger = logger;
        }
        protected static string GetExceptionMessage(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            if (ex != null)
            {
                sb.Append(ex.Message);
            }

            Exception inner = ex?.InnerException;
            int counter = 1;
            while (inner != null)
            {
                sb.Append(" ,InnerException" + counter.ToString() + " Message:" + inner.Message);
                inner = inner.InnerException;
            }

            return sb.ToString();
        }
        protected void Log(EventId? eventId, string details, string? objectID = null, LogTypeEnum logType = LogTypeEnum.SuccessLog)
        {
            //If datials has some {} it will be regarded as log parameter which is not acceptable
            details = details.Replace("{", "[");
            details = details.Replace("}", "]");
            if (logType == LogTypeEnum.ErrorLog)
            {
                if (eventId == null)
                {
                    _logger.LogError(message: details + " ,ObjectID:{ObjectID},logTypeID:{LogTypeID}", objectID, logType);
                }
                else
                {
                    _logger.LogError(eventId: eventId.Value, message: details + " ,ActionID:{ActionID},ObjectID:{ObjectID},logTypeID:{LogTypeID}", eventId.Value, objectID, logType);
                }
            }
            else
            {
                if (eventId == null)
                {
                    _logger.LogInformation(message: details + " ,ObjectID:{ObjectID},logTypeID:{LogTypeID}", objectID, logType);
                }
                else
                {
                    _logger.LogInformation(eventId.Value, message: details + " ,ActionID:{ActionID},ObjectID:{ObjectID},logTypeID:{LogTypeID}", eventId.Value, objectID, logType);
                }
            }
        }
    }
}
