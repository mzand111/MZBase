using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MZBase.Domain;
using MZBase.Infrastructure;
using MZBase.Infrastructure.Service;
using MZBase.Infrastructure.Service.Exceptions;
using MZSimpleDynamicLinq.Core;

namespace MZBase.EntityFrameworkCore
{
    public abstract class EFCoreStorageBusinessService<DomainModel, DBModelEntity, PrimaryKeyType, UnitOfWork, DataContext> :
        BaseStorageBusinessService<DomainModel, PrimaryKeyType>, IStorageBusinessService<DomainModel, PrimaryKeyType>
        where DomainModel : Model<PrimaryKeyType>
        where DBModelEntity : DomainModel, IConvertibleDBModelEntity<DomainModel>, new()
        where PrimaryKeyType : struct
        where UnitOfWork : UnitOfWorkAsync<DataContext>, IDynamicTestableUnitOfWorkAsync
        where DataContext : DbContext
    {
        protected readonly UnitOfWork _unitOfWork;
        protected readonly IBaseLDRCompatibleRepositoryAsync<DomainModel, DBModelEntity, PrimaryKeyType> _baseRepo;

        public EFCoreStorageBusinessService(UnitOfWork unitOfWork,
              IDateTimeProviderService dateTimeProvider,
              ILogger<DomainModel> logger
            ) : base(logger, dateTimeProvider, 600)
        {
            _unitOfWork = unitOfWork;
            _baseRepo = _unitOfWork.GetRepo<DomainModel, DBModelEntity, PrimaryKeyType>();
        }
        public override async Task<PrimaryKeyType> AddAsync(DomainModel item)
        {
            if (item == null)
            {
                var ex = new ServiceArgumentNullException("Input parameter was null:" + nameof(item));
                LogAdd(null, null, ex);
                throw ex;
            }

            await ValidateOnAddAsync(item);

            var g = _baseRepo.Insert(item);
            try
            {
                await _unitOfWork.CommitAsync();

                LogAdd(item, "Successfully add item with ,ID:" +
                  g?.ID.ToString()
                 );
                return g.ID;
            }
            catch (Exception ex)
            {
                LogAdd(item, "", ex);
                throw new ServiceStorageException("Error adding item", ex);
            }
        }
        public override async Task ModifyAsync(DomainModel item)
        {
            if (item == null)
            {
                var exception = new ServiceArgumentNullException(typeof(DomainModel).Name);
                LogModify(item, null, exception);
                throw exception;
            }

            var currentItem = await _baseRepo.GetByIdAsync(item.ID);
            if (currentItem == null)
            {
                var noObj = new ServiceObjectNotFoundException(typeof(DomainModel).Name + " Not Found");
                LogModify(item, null, noObj);
                throw noObj;
            }
            await ValidateOnModifyAsync(item, currentItem);

            if (currentItem is Auditable<PrimaryKeyType> au)
            {
                //This is to keep these fields intact
                var creator = au.CreatedBy;
                var lastModifier = au.LastModifiedBy;
                currentItem.SetFieldsFromDomainModel(item);
                au.CreatedBy = creator;
                au.LastModifiedBy = lastModifier;
            }
            else
            {
                currentItem.SetFieldsFromDomainModel(item);
            }
            try
            {
                await _unitOfWork.CommitAsync();
                LogModify(item, "Successfully modified item with ,ID:" +
                   item.ID.ToString()
                 );
            }

            catch (Exception ex)
            {
                LogModify(item, "", ex);
                throw new ServiceStorageException("Error modifying item", ex);
            }
        }
        public override async Task RemoveByIdAsync(PrimaryKeyType ID)
        {
            var itemToDelete = await _baseRepo.FirstOrDefaultAsync(ss => ss.ID.Equals(ID));

            if (itemToDelete == null)
            {
                var f = new ServiceObjectNotFoundException(typeof(DomainModel).Name + " not found");
                LogRemove(ID, "Item With This Id Not Found", f);
                throw f;
            }
            _baseRepo.Delete(itemToDelete);
            try
            {
                await _unitOfWork.CommitAsync();
                LogRemove(ID, "Item Deleted Successfully", null);
            }

            catch (Exception ex)
            {
                var innerEx = new ServiceStorageException("Error deleting item with id" + ID.ToString(), ex);
                LogRemove(ID, null, ex);
                throw innerEx;
            }
        }
        public override async Task<DomainModel> RetrieveByIdAsync(PrimaryKeyType ID)
        {
            DBModelEntity? item;
            try
            {
                item = await _baseRepo.FirstOrDefaultAsync(ss => ss.ID.Equals(ID));
            }
            catch (Exception ex)
            {
                LogRetrieveSingle(ID, ex);
                throw new ServiceStorageException("Error loading item", ex);
            }
            if (item == null)
            {
                var f = new ServiceObjectNotFoundException(typeof(DomainModel).Name + " not found by id");
                LogRetrieveSingle(ID, f);
                throw f;
            }
            LogRetrieveSingle(ID);
            return item.GetDomainObject();
        }
        public override async Task<LinqDataResult<DomainModel>> ItemsAsync(LinqDataRequest request)
        {
            try
            {
                var f = await _baseRepo.AllItemsAsync(request);
                LogRetrieveMultiple(null, request);
                return f;
            }
            catch (Exception ex)
            {
                LogRetrieveMultiple(null, request, ex);
                throw new ServiceStorageException("Error retrieving items ", ex);
            }
        }


    }
}