using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MZBase.Infrastructure.Service.Exceptions
{
    public class ServiceException : Exception
    {
        protected int? _code;
        public int? Code
        {
            get
            {
                return _code;
            }
        }
        /// <summary>
        /// This property could be used to handle same group of exceptions in the same way at the UI or any other interfaces
        /// </summary>
        public int? GroupingCode { get; protected set; }
        public ServiceException() : base()
        {

        }
        public ServiceException(string message) : base(message)
        {

        }
        public ServiceException(string message, int code) : base(message)
        {
            _code = code;
        }
        public ServiceException(string message, Exception innerException, int code) : base(message, innerException)
        {
            _code = code;
        }
        public ServiceException(string message, Exception innerException, int code, int groupingCode) : base(message, innerException)
        {
            _code = code;
            this.GroupingCode = groupingCode;
        }
        public ServiceException(SerializationInfo info, StreamingContext context, int code) : base(info, context)
        {
            _code = code;
        } 
        public ServiceException(SerializationInfo info, StreamingContext context, int code, int groupingCode) : base(info, context)
        {
            _code = code;
            this.GroupingCode = groupingCode;
        }

        public string ToServiceExceptionString()
        {
            StringBuilder sb = new StringBuilder();


            if (_code != null)
                sb.Append("service_exception_code:" + _code.ToString());
            if (GroupingCode != null)
                sb.Append(",service_exception_grouping_code:" + GroupingCode.ToString());
            if (Message != null)
                sb.Append(",service_exception_message:" + Message);
            Exception ex = this;
            int level = 1;
            while (ex.InnerException != null)
            {
                sb.Append(",level_" + level + ":" + ex.InnerException.Message);
                level++;
                ex = ex.InnerException;
            }

            string finalMessage = sb.ToString();
            return finalMessage;

        }
    }
}
