using Microsoft.Extensions.Logging;
using MZBase.Domain;
using MZBase.Infrastructure.Service.Exceptions;
using MZSimpleDynamicLinq.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MZBase.Infrastructure.Service
{
    public abstract class BaseStorageBusinessService<Model, PrimaryKeyType> : BaseBusinessService<Model>, IStorageBusinessService<Model, PrimaryKeyType>
      where Model : Model<PrimaryKeyType>
      where PrimaryKeyType : struct
    {

        protected readonly IDateTimeProviderService _dateTimeProvider;
        protected readonly int _logBaseID;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dateTimeProvider"></param>
        /// <param name="logBaseID">
        /// logBaseID+1 =Add
        /// logBaseID+2 =Retrieve single
        /// logBaseID+3 =Modify
        /// logBaseID+4= Remove
        /// logBaseID+5 =Retrieve multiple
        /// </param>
        public BaseStorageBusinessService(ILogger<Model> logger, IDateTimeProviderService dateTimeProvider, int logBaseID) : base(logger)
        {

            _dateTimeProvider = dateTimeProvider;
            _logBaseID = logBaseID;
        }


        public abstract Task<PrimaryKeyType> AddAsync(Model item);
        public abstract Task<LinqDataResult<Model>> ItemsAsync(LinqDataRequest request);
        public abstract Task ModifyAsync(Model item);
        public abstract Task RemoveByIdAsync(PrimaryKeyType ID);
        public abstract Task<Model> RetrieveByIdAsync(PrimaryKeyType ID);


        #region logging
        /// <summary>
        /// logBaseID+1 =Add
        /// logBaseID+2 =Retrieve single
        /// logBaseID+3 =Modify
        /// logBaseID+4= Remove
        /// logBaseID+5 =Retrieve multiple
        /// </summary>
        public int LogBaseID { get { return _logBaseID; } }
        protected void LogAdd(Model model, string objectDescriptor = "", Exception? ex = null)
        {
            LogTypeEnum logType = LogTypeEnum.SuccessLog;
            string s = "";
            if (ex != null)
            {
                s = "Error in adding " + typeof(Model).Name + $" ,exception: {ex.Message}";
                logType = LogTypeEnum.ErrorLog;
            }
            else
            {
                s += "Add " + typeof(Model).Name;
                s += " ,ID:" + model.ID.ToString();
            }

            if (!string.IsNullOrWhiteSpace(objectDescriptor))
            {
                s += " ," + objectDescriptor;
            }
            Log(new EventId(_logBaseID + 1, "Add"), s, model?.ID.ToString(), logType);
        }
        protected void LogRetrieveSingle(PrimaryKeyType requestedID, Exception? ex = null)
        {
            LogTypeEnum logType = LogTypeEnum.SuccessLog;
            string s = "";
            if (ex != null)
            {
                s = "Error in Retrieve Single " + typeof(Model).Name;
                logType = LogTypeEnum.ErrorLog;
            }
            else
            {
                s += "Retrieve Single " + typeof(Model).Name;

            }
            s += " ,ID:" + requestedID.ToString();


            Log(new EventId(_logBaseID + 2, "RetrieveSingle"), s, requestedID.ToString(), logType);
        }
        protected void LogRetrieveSingle(string requestedDetails, Exception? ex = null)
        {
            LogTypeEnum logType = LogTypeEnum.SuccessLog;
            string s = "";
            if (ex != null)
            {
                s = "Error in Retrieve Single " + typeof(Model).Name;
                logType = LogTypeEnum.ErrorLog;
            }
            else
            {
                s += "Retrieve Single " + typeof(Model).Name;

            }
            s += " ,requestedDetails:" + requestedDetails.ToString();


            Log(new EventId(_logBaseID + 2, "RetrieveSingle"), s, null, logType);
        }
        protected void LogModify(Model model, string? objectDescriptor = null, Exception? ex = null)
        {
            LogTypeEnum logType = LogTypeEnum.SuccessLog;
            string s = typeof(Model).Name + " updated";
            if (ex != null)
            {
                s = "Error in updating " + typeof(Model).Name;
                logType = LogTypeEnum.ErrorLog;
            }

            if (model != null)
            {
                s += " ,ID:" + model.ID.ToString();
            }
            if (!string.IsNullOrWhiteSpace(objectDescriptor))
            {
                s += " ," + objectDescriptor;
            }
            if (ex != null)
            {
                s += " ,exception:" + GetExceptionMessage(ex);
            }
            Log(new EventId(_logBaseID + 3, "Modify"), s, model?.ID.ToString(), logType);
        }
        protected void LogRemove(PrimaryKeyType itemID, string objectDescriptor = null, Exception ex = null)
        {
            LogTypeEnum logType = LogTypeEnum.SuccessLog;
            string s = typeof(Model).Name + " removed";
            if (ex != null)
            {
                s = "Error in removing " + typeof(Model).Name;
                logType = LogTypeEnum.ErrorLog;
            }

            s += " ,ID:" + itemID.ToString();

            if (!string.IsNullOrWhiteSpace(objectDescriptor))
            {
                s += " ," + objectDescriptor;
            }
            if (ex != null)
            {
                s += " ,exception:" + GetExceptionMessage(ex);
            }
            Log(new EventId(_logBaseID + 4, "Remove"), s, itemID.ToString(), logType);
        }

        protected void LogAndThrowNotFoundOnRemove(PrimaryKeyType requestedID)
        {
            var f = new ServiceObjectNotFoundException("Requested '" + typeof(Model).Name + "' not found");
            LogRemove(requestedID, "", f);
            throw f;
        }
        protected void LogAndThrowNotFoundOnModify(Model requestedModel)
        {
            var f = new ServiceObjectNotFoundException("Requested '" + typeof(Model).Name + "' not found");
            LogModify(requestedModel, "", f);
            throw f;
        }


        protected void LogRetrieveMultiple(Dictionary<string, object> parameters = null, LinqDataRequest request = null, Exception ex = null)
        {
            LogTypeEnum logType = LogTypeEnum.SuccessLog;
            StringBuilder sb = new StringBuilder();
            if (ex == null)
            {
                sb.Append("Retrieve Multiple " + typeof(Model).Name);
            }
            else
            {
                sb.Append("Error in Retrieve Multiple " + typeof(Model).Name);
                logType = LogTypeEnum.ErrorLog;
            }

            if (parameters != null)
            {
                sb.Append(" ,with parameters::");
                foreach (var item in parameters)
                {
                    sb.Append(" ," + item.Key + ":" + item.Value);
                }
            }

            if (request != null)
            {
                sb.Append(",with request parameters:: take:" + request.Take);
                sb.Append(",skip:" + request.Skip);
                if (request.Sort != null && request.Sort.Any())
                {
                    sb.Append(",sort::");
                    foreach (var item in request.Sort)
                    {
                        sb.Append(item.ToExpression() + ",");
                    }
                }
                if (request.Filter != null)
                {

                    sb.Append(",filter::");
                    sb.Append(";;field:" + request.Filter.Field + ",value:" + request.Filter.Value + ",operator:" + request.Filter.Operator + ",logic" + request.Filter.Logic + ";;");
                    var ff = request.Filter.GetFlat();
                    foreach (var f in ff)
                    {
                        sb.Append(";;field:" + f.Field + ",value:" + f.Value + ",operator:" + f.Operator + ",logic" + f.Logic + ";;");

                    }
                }
            }

            Log(_logBaseID + 5, sb.ToString(), null, logType);
        }

        #endregion


        #region validation

        protected abstract Task ValidateOnAddAsync(Model item);
        protected abstract Task ValidateOnModifyAsync(Model receivedItem, Model storageItem);
        protected void ValidateIAuditableOnAdd<Model>(List<ModelFieldValidationResult> errors, Model objectToValidate)
           where Model : IAuditable<PrimaryKeyType>

        {
            if (string.IsNullOrWhiteSpace(objectToValidate.CreatedBy))
            {
                errors.Add(new ModelFieldValidationResult()
                {
                    FieldName = nameof(objectToValidate.CreatedBy),
                    ValidationMessage = "The value of field '" + nameof(objectToValidate.CreatedBy) + "' must not be empty",
                    Code = (int)ModelFieldValidationResultCode.CreatedBy_IsEmpty
                });
            }
            if (string.IsNullOrWhiteSpace(objectToValidate.LastModifiedBy))
            {
                errors.Add(new ModelFieldValidationResult()
                {
                    FieldName = nameof(objectToValidate.LastModifiedBy),
                    ValidationMessage = "The value of field '" + nameof(objectToValidate.LastModifiedBy) + "' must not be empty",
                    Code = (int)ModelFieldValidationResultCode.LastModifiedBy_IsEmpty
                });
            }
            if (objectToValidate.LastModifiedBy != objectToValidate.CreatedBy)
            {
                errors.Add(new ModelFieldValidationResult()
                {
                    FieldName = nameof(objectToValidate.LastModifiedBy),
                    ValidationMessage = "The value of field '" + nameof(objectToValidate.LastModifiedBy) + "' must be the same as '" + nameof(objectToValidate.CreatedBy) + "' at creation",
                    Code = (int)ModelFieldValidationResultCode.CreatedByAndLastModifiedBy_ShouldBeSameAtStart
                });
            }
            if (objectToValidate.CreationTime == DateTime.MinValue)
            {
                errors.Add(new ModelFieldValidationResult()
                {
                    FieldName = nameof(objectToValidate.CreationTime),
                    ValidationMessage = "The value is invalid",
                    Code = (int)ModelFieldValidationResultCode.CreationTime_ValueIsNotValid
                });
            }
            if (objectToValidate.LastModificationTime == DateTime.MinValue)
            {
                errors.Add(new ModelFieldValidationResult()
                {
                    FieldName = nameof(objectToValidate.LastModificationTime),
                    ValidationMessage = "The value is invalid",
                    Code = (int)ModelFieldValidationResultCode.LastModificationTime_ValueIsNotValid
                });
            }
            if (objectToValidate.CreationTime != objectToValidate.LastModificationTime)
            {
                errors.Add(new ModelFieldValidationResult()
                {
                    FieldName = nameof(objectToValidate.LastModificationTime),
                    ValidationMessage = "The value of field '" + nameof(objectToValidate.CreationTime) + "' must be the same as '" + nameof(objectToValidate.LastModificationTime) + "' at creation",
                    Code = (int)ModelFieldValidationResultCode.CreationTimeAndLastModificationTime_ShouldBeSameAtStart
                });
            }
        }
        protected void ValidateIAuditableOnModify<Model>(List<ModelFieldValidationResult> errors, Model objectToValidate, Model objectFromStorage)
           where Model : IAuditable<PrimaryKeyType>

        {
            if (string.IsNullOrEmpty(objectToValidate.LastModifiedBy))
            {
                errors.Add(new ModelFieldValidationResult()
                {
                    FieldName = nameof(objectToValidate.LastModifiedBy),
                    ValidationMessage = "The value of field '" + nameof(objectToValidate.LastModifiedBy) + "' must not be empty",
                    Code = (int)ModelFieldValidationResultCode.LastModifiedBy_IsEmpty
                });
            }
            if (objectToValidate.LastModificationTime == DateTime.MinValue)
            {
                errors.Add(new ModelFieldValidationResult()
                {
                    FieldName = nameof(objectToValidate.LastModificationTime),
                    ValidationMessage = "The value is invalid",
                    Code = (int)ModelFieldValidationResultCode.LastModificationTime_ValueIsNotValid
                });
            }
            if (objectToValidate.LastModificationTime < objectFromStorage.CreationTime)
            {
                errors.Add(new ModelFieldValidationResult()
                {
                    FieldName = nameof(objectToValidate.LastModificationTime),
                    ValidationMessage = "The value for '" + nameof(objectToValidate.LastModificationTime) + "' of incomming object should not be before '" + nameof(objectFromStorage.CreationTime) + "' of object in DB",
                    Code = (int)ModelFieldValidationResultCode.LastModificationTime_CanNotBeBeforeCreationOnModify
                });
            }
            if (objectToValidate.LastModificationTime < objectFromStorage.LastModificationTime)
            {
                errors.Add(new ModelFieldValidationResult()
                {
                    FieldName = nameof(objectToValidate.LastModificationTime),
                    ValidationMessage = "The value for '" + nameof(objectToValidate.LastModificationTime) + "' of incomming object should not be before '" + nameof(objectFromStorage.LastModificationTime) + "' of object in DB",
                    Code = (int)ModelFieldValidationResultCode.LastModificationTime_CanNotBeBeforeLastModificationOfDBObjectOnModify
                });
            }
        }
        #endregion
    }
}
