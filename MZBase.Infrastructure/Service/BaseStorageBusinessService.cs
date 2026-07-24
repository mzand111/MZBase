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
    /// <summary>
    /// Base class for storage business services, providing common functionality for adding, retrieving, modifying, and removing entities.
    /// </summary>
    /// <typeparam name="Model">The type of the model entity.</typeparam>
    /// <typeparam name="PrimaryKeyType">The type of the primary key of the model entity.</typeparam>
    public abstract class BaseStorageBusinessService<Model, PrimaryKeyType> : BaseBusinessService<Model>, IStorageBusinessService<Model, PrimaryKeyType>
      where Model : Model<PrimaryKeyType>
      where PrimaryKeyType : struct
    {
        protected readonly IDateTimeProviderService _dateTimeProvider;
        protected readonly int _logBaseID;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseStorageBusinessService{Model, PrimaryKeyType}"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="dateTimeProvider">The date time provider service.</param>
        /// <param name="logBaseID">
        /// The base ID for logging. 
        /// logBaseID+1 = Add, 
        /// logBaseID+2 = Retrieve single, 
        /// logBaseID+3 = Modify, 
        /// logBaseID+4 = Remove, 
        /// logBaseID+5 = Retrieve multiple.
        /// </param>
        public BaseStorageBusinessService(ILogger<Model> logger, IDateTimeProviderService dateTimeProvider, int logBaseID) : base(logger)
        {
            _dateTimeProvider = dateTimeProvider;
            _logBaseID = logBaseID;
        }

        /// <summary>
        /// Adds an entity asynchronously.
        /// </summary>
        /// <param name="item">The entity to add.</param>
        /// <returns>The primary key of the added entity.</returns>
        public abstract Task<PrimaryKeyType> AddAsync(Model item);

        /// <summary>
        /// Retrieves multiple entities based on the specified Linq data request asynchronously.
        /// </summary>
        /// <param name="request">The Linq data request containing query parameters.</param>
        /// <returns>A LinqDataResult containing the collection of entities.</returns>
        public abstract Task<LinqDataResult<Model>> ItemsAsync(LinqDataRequest request);

        /// <summary>
        /// Modifies an existing entity asynchronously.
        /// </summary>
        /// <param name="item">The entity with updated values.</param>
        public abstract Task ModifyAsync(Model item);

        /// <summary>
        /// Removes an entity by its primary key asynchronously.
        /// </summary>
        /// <param name="ID">The primary key of the entity to remove.</param>
        public abstract Task RemoveByIdAsync(PrimaryKeyType ID);

        /// <summary>
        /// Retrieves an entity by its primary key asynchronously.
        /// </summary>
        /// <param name="ID">The primary key of the entity to retrieve.</param>
        /// <returns>The entity if found, otherwise null.</returns>
        public abstract Task<Model> RetrieveByIdAsync(PrimaryKeyType ID);

        #region logging
        /// <summary>
        /// Gets the base ID for logging.
        /// </summary>
        public int LogBaseID { get { return _logBaseID; } }

        /// <summary>
        /// Logs the addition of an entity.
        /// </summary>
        /// <param name="model">The entity being added.</param>
        /// <param name="objectDescriptor">Optional descriptor of the object.</param>
        /// <param name="ex">Optional exception if an error occurred.</param>
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

        /// <summary>
        /// Logs the retrieval of a single entity.
        /// </summary>
        /// <param name="requestedID">The primary key of the entity being retrieved.</param>
        /// <param name="ex">Optional exception if an error occurred.</param>
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

        /// <summary>
        /// Logs the retrieval of a single entity with details.
        /// </summary>
        /// <param name="requestedDetails">The details of the entity being retrieved.</param>
        /// <param name="ex">Optional exception if an error occurred.</param>
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

        /// <summary>
        /// Logs the modification of an entity.
        /// </summary>
        /// <param name="model">The entity being modified.</param>
        /// <param name="objectDescriptor">Optional descriptor of the object.</param>
        /// <param name="ex">Optional exception if an error occurred.</param>
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

        protected void LogModify(string modelId, string? objectDescriptor = null, Exception? ex = null)
        {
            LogTypeEnum logType = LogTypeEnum.SuccessLog;
            string s = typeof(Model).Name + " updated";
            if (ex != null)
            {
                s = "Error in updating " + typeof(Model).Name;
                logType = LogTypeEnum.ErrorLog;
            }

            if (modelId != null)
            {
                s += " ,ID:" + modelId;
            }
            if (!string.IsNullOrWhiteSpace(objectDescriptor))
            {
                s += " ," + objectDescriptor;
            }
            if (ex != null)
            {
                s += " ,exception:" + GetExceptionMessage(ex);
            }
            Log(new EventId(_logBaseID + 3, "Modify"), s, modelId, logType);
        }

        /// <summary>
        /// Logs the removal of an entity.
        /// </summary>
        /// <param name="itemID">The primary key of the entity being removed.</param>
        /// <param name="objectDescriptor">Optional descriptor of the object.</param>
        /// <param name="ex">Optional exception if an error occurred.</param>
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

        /// <summary>
        /// Logs and throws a not found exception when removing an entity.
        /// </summary>
        /// <param name="requestedID">The primary key of the entity being removed.</param>
        protected void LogAndThrowNotFoundOnRemove(PrimaryKeyType requestedID)
        {
            var f = new ServiceObjectNotFoundException("Requested '" + typeof(Model).Name + "' not found");
            LogRemove(requestedID, "", f);
            throw f;
        }

        /// <summary>
        /// Logs and throws a not found exception when modifying an entity.
        /// </summary>
        /// <param name="requestedModel">The entity being modified.</param>
        protected void LogAndThrowNotFoundOnModify(Model requestedModel)
        {
            var f = new ServiceObjectNotFoundException("Requested '" + typeof(Model).Name + "' not found");
            LogModify(requestedModel, "", f);
            throw f;
        }

        /// <summary>
        /// Logs the retrieval of multiple entities.
        /// </summary>
        /// <param name="parameters">Optional parameters for the retrieval.</param>
        /// <param name="request">The Linq data request containing query parameters.</param>
        /// <param name="ex">Optional exception if an error occurred.</param>
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
        /// <summary>
        /// Validates an entity before adding it.
        /// </summary>
        /// <param name="item">The entity to validate.</param>
        protected abstract Task ValidateOnAddAsync(Model item);

        /// <summary>
        /// Validates an entity before modifying it.
        /// </summary>
        /// <param name="receivedItem">The entity with updated values.</param>
        /// <param name="storageItem">The existing entity from storage.</param>
        protected abstract Task ValidateOnModifyAsync(Model receivedItem, Model storageItem);

        /// <summary>
        /// Validates an auditable entity before adding it.
        /// </summary>
        /// <typeparam name="Model">The type of the auditable entity.</typeparam>
        /// <param name="errors">The list of validation errors.</param>
        /// <param name="objectToValidate">The entity to validate.</param>
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

        /// <summary>
        /// Validates an auditable entity before modifying it.
        /// </summary>
        /// <typeparam name="Model">The type of the auditable entity.</typeparam>
        /// <param name="errors">The list of validation errors.</param>
        /// <param name="objectToValidate">The entity to validate.</param>
        /// <param name="objectFromStorage">The existing entity from storage.</param>
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
                    ValidationMessage = "The value for '" + nameof(objectToValidate.LastModificationTime) + "' of incoming object should not be before '" + nameof(objectFromStorage.CreationTime) + "' of object in DB",
                    Code = (int)ModelFieldValidationResultCode.LastModificationTime_CanNotBeBeforeCreationOnModify
                });
            }
            if (objectToValidate.LastModificationTime < objectFromStorage.LastModificationTime)
            {
                errors.Add(new ModelFieldValidationResult()
                {
                    FieldName = nameof(objectToValidate.LastModificationTime),
                    ValidationMessage = "The value for '" + nameof(objectToValidate.LastModificationTime) + "' of incoming object should not be before '" + nameof(objectFromStorage.LastModificationTime) + "' of object in DB",
                    Code = (int)ModelFieldValidationResultCode.LastModificationTime_CanNotBeBeforeLastModificationOfDBObjectOnModify
                });
            }
        }
        #endregion
    }
}
