using System;
using System.Collections.Generic;
using System.Text;

namespace MZBase.Infrastructure.Service.Exceptions
{
    public class ModelFieldValidationResult
    {
        /// <summary>
        /// Optional code 
        /// </summary>
        public int? Code { get; set; }
        public string FieldName { get; set; }
        public string ValidationMessage { get; set; }

        public string JSONStringFormat
        {
            get
            {
                string s = "";
                if (Code.HasValue) s += nameof(Code) + ":" + Code.ToString() + ",";
                if (!string.IsNullOrWhiteSpace(FieldName)) s += nameof(FieldName) + ":" + FieldName + ",";
                if (!string.IsNullOrWhiteSpace(ValidationMessage)) s += nameof(ValidationMessage) + ":" + ValidationMessage + ",";
                if (!string.IsNullOrWhiteSpace(s))
                {
                    s += "++";
                    s = s.Replace(",++", "");
                }
                s = "{" + s + "}";
                return s;
            }
        }
    }

    public enum ModelFieldValidationResultCode : int
    {
        CreatedBy_IsEmpty = 1,
        LastModifiedBy_IsEmpty = 2,
        CreatedByAndLastModifiedBy_ShouldBeSameAtStart = 3,
        CreationTime_ValueIsNotValid = 4,
        CreationTimeAndLastModificationTime_ShouldBeSameAtStart = 5,
        LastModificationTime_ValueIsNotValid = 6,
        LastModificationTime_CanNotBeBeforeCreationOnModify = 7,
        LastModificationTime_CanNotBeBeforeLastModificationOfDBObjectOnModify = 8,
        Value_IsEmpty = 9,
    }
}
