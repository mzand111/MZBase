using Microsoft.Extensions.Logging;
using Moq;
using MZBase.Domain;
using MZBase.Infrastructure;
using MZBase.Infrastructure.Service;
using MZBase.Infrastructure.Service.Exceptions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace MZBase.Test.Unit.Service
{
    public abstract class AuditableStorageServiceTest<StorageService, TUnitOfWork, Model, TDBModel, PrimarykeyType> : StorageServiceTest<StorageService, TUnitOfWork, Model, TDBModel, PrimarykeyType>
        where StorageService : BaseStorageBusinessService<Model, PrimarykeyType>
       where TUnitOfWork : class, IDynamicTestableUnitOfWorkAsync
        where Model : Auditable<PrimarykeyType>
          where TDBModel : Model, IConvertibleDBModelEntity<Model>, new()
       where PrimarykeyType : struct

    {
        protected virtual void AddMockSetupsCommon(Mock<Model> mock)
        {

        }
        protected virtual void AddMockSetupsCommon(Mock<TDBModel> mock)
        {

        }
        protected virtual void AddUoWMockSetupsCommon(Mock<TUnitOfWork> uowMock)
        {

        }


        #region Add

        [Fact]
        public async Task Auditable_Add_ShouldRaise_ServiceModelValidationException_WhenNothingSet()
        {
            //arrange
            var modelMock1 = new Moq.Mock<Model>();
            AddMockSetupsCommon(modelMock1);
            Model entity = modelMock1.Object;
            //act
            Func<Task> act = () => service.AddAsync(entity);
            //assert
            ServiceModelValidationException exception = await Assert.ThrowsAsync<ServiceModelValidationException>(act);
            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Error),
             It.Is<EventId>(l => l == service.LogBaseID + 1),
             It.Is<It.IsAnyType>((v, t) => true),
             It.IsAny<ServiceModelValidationException>(),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            serviceLogger.VerifyNoOtherCalls();

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.CreatedBy_IsEmpty &&
            uu.FieldName == nameof(entity.CreatedBy)), "Validation error for field '" + nameof(entity.CreatedBy) + "' expected when empty on add");
            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModifiedBy_IsEmpty &&
            uu.FieldName == nameof(entity.LastModifiedBy)), "Validation error for field '" + nameof(entity.LastModifiedBy) + "' expected when empty on add");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.CreationTime_ValueIsNotValid &&
            uu.FieldName == nameof(entity.CreationTime)), "Validation error for field '" + nameof(entity.CreationTime) + "' expected");
            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_ValueIsNotValid &&
            uu.FieldName == nameof(entity.LastModificationTime)), "Validation error for field '" + nameof(entity.LastModificationTime) + "' expected");

        }
        [Fact]
        public async Task Auditable_Add_ShouldRaise_ServiceModelValidationException_WhenJustCreatedBySet()
        {
            //arrange
            var modelMock1 = new Mock<Model>();
            modelMock1.As<IAuditable<PrimarykeyType>>().Setup(uu => uu.CreatedBy).Returns("someUserName");
            AddMockSetupsCommon(modelMock1);
            Model entity = modelMock1.Object;
            //act
            Func<Task> act = () => service.AddAsync(entity);
            //assert
            ServiceModelValidationException exception = await Assert.ThrowsAsync<ServiceModelValidationException>(act);
            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Error),
             It.Is<EventId>(l => l == service.LogBaseID + 1),
             It.Is<It.IsAnyType>((v, t) => true),
             It.IsAny<ServiceModelValidationException>(),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            Assert.False(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.CreatedBy_IsEmpty &&
           uu.FieldName == nameof(entity.CreatedBy)), "Validation error for field '" + nameof(entity.CreatedBy) + "' was not expected");
            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModifiedBy_IsEmpty &&
            uu.FieldName == nameof(entity.LastModifiedBy)), "Validation error for field '" + nameof(entity.LastModifiedBy) + "' expected when empty on add");
            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.CreatedByAndLastModifiedBy_ShouldBeSameAtStart &&
           uu.FieldName == nameof(entity.LastModifiedBy)), "Validation error for field '" + nameof(entity.LastModifiedBy) + "' expected, when not matched with '" + nameof(entity.CreatedBy) + "' field on add");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.CreationTime_ValueIsNotValid &&
            uu.FieldName == nameof(entity.CreationTime)), "Validation error for field '" + nameof(entity.CreationTime) + "' expected");
            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_ValueIsNotValid &&
            uu.FieldName == nameof(entity.LastModificationTime)), "Validation error for field '" + nameof(entity.LastModificationTime) + "' expected");

        }
        [Fact]
        public async Task Auditable_Add_ShouldRaise_ServiceModelValidationException_WhenJustCreationTimeSet()
        {
            //arrange
            var modelMock1 = new Moq.Mock<Model>();
            modelMock1.As<IAuditable<PrimarykeyType>>().Setup(uu => uu.CreationTime).Returns(DateTime.Now);
            AddMockSetupsCommon(modelMock1);
            Model entity = modelMock1.Object;
            //act
            Func<Task> act = () => service.AddAsync(entity);
            //assert
            ServiceModelValidationException exception = await Assert.ThrowsAsync<ServiceModelValidationException>(act);
            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Error),
             It.Is<EventId>(l => l == service.LogBaseID + 1),
             It.Is<It.IsAnyType>((v, t) => true),
             It.IsAny<ServiceModelValidationException>(),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.CreatedBy_IsEmpty &&
            uu.FieldName == nameof(entity.CreatedBy)), "Validation error for field '" + nameof(entity.CreatedBy) + "' expected when empty on add");
            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModifiedBy_IsEmpty &&
            uu.FieldName == nameof(entity.LastModifiedBy)), "Validation error for field '" + nameof(entity.LastModifiedBy) + "' expected when empty on add");

            Assert.False(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.CreationTime_ValueIsNotValid &&
            uu.FieldName == nameof(entity.CreationTime)), "Validation error for field '" + nameof(entity.CreationTime) + "' was not expected");
            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_ValueIsNotValid &&
            uu.FieldName == nameof(entity.LastModificationTime)), "Validation error for field '" + nameof(entity.LastModificationTime) + "' expected");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.CreationTimeAndLastModificationTime_ShouldBeSameAtStart), "Validation error for field '" + nameof(entity.LastModificationTime) + "' expected for code " + nameof(ModelFieldValidationResultCode.CreationTimeAndLastModificationTime_ShouldBeSameAtStart));

        }
        [Fact]
        public async Task Auditable_Add_ShouldRaise_ServiceModelValidationException_WhenLastModifiedBySet()
        {
            //arrange
            var modelMock1 = new Moq.Mock<Model>();
            modelMock1.As<IAuditable<PrimarykeyType>>().Setup(uu => uu.LastModifiedBy).Returns("someUserName");

            AddMockSetupsCommon(modelMock1);
            Model entity = modelMock1.Object;
            //act
            Func<Task> act = () => service.AddAsync(entity);
            //assert
            ServiceModelValidationException exception = await Assert.ThrowsAsync<ServiceModelValidationException>(act);
            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Error),
             It.Is<EventId>(l => l == service.LogBaseID + 1),
             It.Is<It.IsAnyType>((v, t) => true),
             It.IsAny<ServiceModelValidationException>(),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.CreatedBy_IsEmpty &&
           uu.FieldName == nameof(entity.CreatedBy)), "Validation error for field '" + nameof(entity.CreatedBy) + "' expected");
            Assert.False(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModifiedBy_IsEmpty &&
            uu.FieldName == nameof(entity.LastModifiedBy)), "Validation error for field '" + nameof(entity.LastModifiedBy) + "' expected when empty on add");
            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.CreatedByAndLastModifiedBy_ShouldBeSameAtStart &&
           uu.FieldName == nameof(entity.LastModifiedBy)), "Validation error for field '" + nameof(entity.LastModifiedBy) + "' expected, when not matched with '" + nameof(entity.CreatedBy) + "' field on add");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.CreationTime_ValueIsNotValid &&
            uu.FieldName == nameof(entity.CreationTime)), "Validation error for field '" + nameof(entity.CreationTime) + "' expected");
            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_ValueIsNotValid &&
            uu.FieldName == nameof(entity.LastModificationTime)), "Validation error for field '" + nameof(entity.LastModificationTime) + "' expected");

        }
        [Fact]
        public async Task Auditable_Add_ShouldRaise_ServiceModelValidationException_WhenLastModificationTimeSet()
        {
            //arrange
            var modelMock1 = new Mock<Model>();
            modelMock1.As<IAuditable<PrimarykeyType>>().Setup(uu => uu.LastModificationTime).Returns(DateTime.Now);
            AddMockSetupsCommon(modelMock1);
            Model entity = modelMock1.Object;
            //act
            Func<Task> act = () => service.AddAsync(entity);
            //assert
            ServiceModelValidationException exception = await Assert.ThrowsAsync<ServiceModelValidationException>(act);
            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Error),
             It.Is<EventId>(l => l == service.LogBaseID + 1),
             It.Is<It.IsAnyType>((v, t) => true),
             It.IsAny<ServiceModelValidationException>(),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.CreatedBy_IsEmpty &&
           uu.FieldName == nameof(entity.CreatedBy)), "Validation error for field '" + nameof(entity.CreatedBy) + "' expected when empty on add");
            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModifiedBy_IsEmpty &&
            uu.FieldName == nameof(entity.LastModifiedBy)), "Validation error for field '" + nameof(entity.LastModifiedBy) + "' expected when empty on add");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.CreationTime_ValueIsNotValid &&
            uu.FieldName == nameof(entity.CreationTime)), "Validation error for field '" + nameof(entity.CreationTime) + "' expected");
            Assert.False(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_ValueIsNotValid &&
            uu.FieldName == nameof(entity.LastModificationTime)), "Validation error for field '" + nameof(entity.LastModificationTime) + "' was not expected");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.CreationTimeAndLastModificationTime_ShouldBeSameAtStart), "Validation error for field '" + nameof(entity.LastModificationTime) + "' expected for code " + nameof(ModelFieldValidationResultCode.CreationTimeAndLastModificationTime_ShouldBeSameAtStart));

        }
        #endregion

        #region Modify

        [Fact]
        public virtual async Task Auditable_Modify_ShouldRaise_ServiceModelValidationException_WhenAuditableFieldsNotSetOnModify()
        {
            //arrange           
            Mock<TDBModel> dbObjectMock = new Mock<TDBModel>();


            //We assume that the object in database has the data consistency
            var f = DateTime.Now;
            dbObjectMock.Setup<string>(uu => uu.CreatedBy).Returns("someCreatorUser");
            dbObjectMock.Setup<string>(uu => uu.LastModifiedBy).Returns("someCreatorUser");
            dbObjectMock.Setup<DateTime>(uu => uu.CreationTime).Returns(f);
            dbObjectMock.Setup<DateTime>(uu => uu.LastModificationTime).Returns(f);
            AddMockSetupsCommon(dbObjectMock);
            TDBModel dbObject = dbObjectMock.Object;

            Mock<Model> incommingObjectMock = new Mock<Model>();
            AddMockSetupsCommon(incommingObjectMock);
            Model incommingObject = incommingObjectMock.Object;

            //We set nothing here for the incoming object

            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            AddUoWMockSetupsCommon(uofm);
            uofm.Setup(uu => uu.GetRepo<Model, TDBModel, PrimarykeyType>()).Returns(() =>
            {
                var item = new Mock<IBaseLDRCompatibleRepositoryAsync<Model, TDBModel, PrimarykeyType>>();

                item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<TDBModel, bool>>>()))
                    .ReturnsAsync(dbObject);
                item.Setup(gg => gg.GetByIdAsync(It.IsAny<PrimarykeyType>()))
                 .ReturnsAsync(dbObject);

                return item.Object;
            });
            var localService = GetBusinessService(uofm.Object);
            //act
            Func<Task> act = () => localService.ModifyAsync(incommingObject);
            //assert
            ServiceModelValidationException exception = await Assert.ThrowsAsync<ServiceModelValidationException>(act);

            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Error),
             It.Is<EventId>(l => l == service.LogBaseID + 3),
             It.Is<It.IsAnyType>((v, t) => true),
             It.IsAny<ServiceModelValidationException>(),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            serviceLogger.VerifyNoOtherCalls();


            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModifiedBy_IsEmpty &&
            uu.FieldName == nameof(incommingObject.LastModifiedBy)), "Validation error for field '" + nameof(incommingObject.LastModifiedBy) + "' expected when empty on modify");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_ValueIsNotValid &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' expected when empty on modify");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_CanNotBeBeforeCreationOnModify &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' of incoming object expected when is before '" + nameof(dbObject.CreationTime) + "' field of dbObject");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_CanNotBeBeforeLastModificationOfDBObjectOnModify &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' of incoming object expected when is before '" + nameof(dbObject.LastModificationTime) + "' field of dbObject");

        }
        [Fact]
        public virtual async Task Auditable_Modify_ShouldRaise_ServiceModelValidationException_WhenJustLastModifiedBySetOnModify()
        {
            //arrange           
            Mock<TDBModel> dbObjectMock = new Mock<TDBModel>();

            //We assume that the object in database has the data consistency
            var f = DateTime.Now;
            dbObjectMock.Setup<DateTime>(uu => uu.CreationTime).Returns(f);
            dbObjectMock.Setup<DateTime>(uu => uu.LastModificationTime).Returns(f);
            dbObjectMock.Setup<string>(uu => uu.CreatedBy).Returns("someCreatorUser");
            dbObjectMock.Setup<string>(uu => uu.LastModifiedBy).Returns("someCreatorUser");
            TDBModel dbObject = dbObjectMock.Object;


            Mock<Model> incommingObjectMock = new Mock<Model>();
            //We set some fields of the incoming object
            incommingObjectMock.Setup<string>(uu => uu.LastModifiedBy).Returns("someOtherUser");
            Model incommingObject = incommingObjectMock.Object;


            // incommingObject.LastModifiedBy = "someOtherUser";

            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            AddUoWMockSetupsCommon(uofm);
            uofm.Setup(uu => uu.GetRepo<Model, TDBModel, PrimarykeyType>()).Returns(() =>
            {
                var item = new Mock<IBaseLDRCompatibleRepositoryAsync<Model, TDBModel, PrimarykeyType>>();

                item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<TDBModel, bool>>>()))
                    .ReturnsAsync(dbObject);
                item.Setup(gg => gg.GetByIdAsync(It.IsAny<PrimarykeyType>()))
                 .ReturnsAsync(dbObject);

                return item.Object;
            });
            var localService = GetBusinessService(uofm.Object);
            //act
            Func<Task> act = () => localService.ModifyAsync(incommingObject);
            //assert
            ServiceModelValidationException exception = await Assert.ThrowsAsync<ServiceModelValidationException>(act);

            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Error),
             It.Is<EventId>(l => l == service.LogBaseID + 3),
             It.Is<It.IsAnyType>((v, t) => true),
             It.IsAny<ServiceModelValidationException>(),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            serviceLogger.VerifyNoOtherCalls();


            Assert.False(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModifiedBy_IsEmpty &&
            uu.FieldName == nameof(incommingObject.LastModifiedBy)), "Validation error for field '" + nameof(incommingObject.LastModifiedBy) + "' expected when empty on modify");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_ValueIsNotValid &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' was not expected when not empty on modify");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_CanNotBeBeforeCreationOnModify &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' of incoming object expected when is before '" + nameof(dbObject.CreationTime) + "' field of dbObject");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_CanNotBeBeforeLastModificationOfDBObjectOnModify &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' of incoming object expected when is before '" + nameof(dbObject.LastModificationTime) + "' field of dbObject");

        }
        [Fact]
        public virtual async Task Auditable_Modify_ShouldRaise_ServiceModelValidationException_WhenJustLastModificationTimeSetOnAnOlderThanBothDBTimesOnModify()
        {
            Mock<TDBModel> dbObjectMock = new Mock<TDBModel>();

            //We assume that the object in database has the data consistency
            var f = DateTime.Now;
            dbObjectMock.Setup<DateTime>(uu => uu.CreationTime).Returns(f);
            dbObjectMock.Setup<DateTime>(uu => uu.LastModificationTime).Returns(f);
            dbObjectMock.Setup<string>(uu => uu.CreatedBy).Returns("someCreatorUser");
            dbObjectMock.Setup<string>(uu => uu.LastModifiedBy).Returns("someCreatorUser");
            TDBModel dbObject = dbObjectMock.Object;


            Mock<Model> incommingObjectMock = new Mock<Model>();
            //We set some fields of the incoming object
            incommingObjectMock.Setup(uu => uu.LastModificationTime).Returns(f.AddDays(-1));
            Model incommingObject = incommingObjectMock.Object;


            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            AddUoWMockSetupsCommon(uofm);
            uofm.Setup(uu => uu.GetRepo<Model, TDBModel, PrimarykeyType>()).Returns(() =>
            {
                var item = new Mock<IBaseLDRCompatibleRepositoryAsync<Model, TDBModel, PrimarykeyType>>();

                item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<TDBModel, bool>>>()))
                    .ReturnsAsync(dbObject);
                item.Setup(gg => gg.GetByIdAsync(It.IsAny<PrimarykeyType>()))
                 .ReturnsAsync(dbObject);

                return item.Object;
            });
            var localService = GetBusinessService(uofm.Object);
            //act
            Func<Task> act = () => localService.ModifyAsync(incommingObject);
            //assert
            ServiceModelValidationException exception = await Assert.ThrowsAsync<ServiceModelValidationException>(act);

            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Error),
             It.Is<EventId>(l => l == service.LogBaseID + 3),
             It.Is<It.IsAnyType>((v, t) => true),
             It.IsAny<ServiceModelValidationException>(),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            serviceLogger.VerifyNoOtherCalls();


            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModifiedBy_IsEmpty &&
             uu.FieldName == nameof(incommingObject.LastModifiedBy)), "Validation error for field '" + nameof(incommingObject.LastModifiedBy) + "' expected when empty on modify");

            Assert.False(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_ValueIsNotValid &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' was not expected when not empty on modify");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_CanNotBeBeforeCreationOnModify &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' of incoming object expected when is before '" + nameof(dbObject.CreationTime) + "' field of dbObject");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_CanNotBeBeforeLastModificationOfDBObjectOnModify &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' of incoming object expected when is before '" + nameof(dbObject.LastModificationTime) + "' field of dbObject");


        }
        [Fact]
        public virtual async Task Auditable_Modify_ShouldRaise_ServiceModelValidationException_WhenJustLastModificationTimeSetOnAnOlderLastModificationTimeFromDBOnModify()
        {
            //arrange           
            Mock<TDBModel> dbObjectMock = new Mock<TDBModel>();
            //We assume that the object in database has the data consistency
            dbObjectMock.Setup<string>(uu => uu.CreatedBy).Returns("someCreatorUser");
            dbObjectMock.Setup<string>(uu => uu.LastModifiedBy).Returns("someCreatorUser");
            var f = DateTime.Now;
            dbObjectMock.Setup<DateTime>(uu => uu.CreationTime).Returns(f.AddDays(-2));
            dbObjectMock.Setup<DateTime>(uu => uu.LastModificationTime).Returns(f);

            TDBModel dbObject = dbObjectMock.Object;



            Mock<Model> incommingObjectMock = new Mock<Model>();
            //We set some fields of the incoming object
            incommingObjectMock.Setup<DateTime>(uu => uu.LastModificationTime).Returns(f.AddDays(-1));
            Model incommingObject = incommingObjectMock.Object;




            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            AddUoWMockSetupsCommon(uofm);
            uofm.Setup(uu => uu.GetRepo<Model, TDBModel, PrimarykeyType>()).Returns(() =>
            {
                var item = new Mock<IBaseLDRCompatibleRepositoryAsync<Model, TDBModel, PrimarykeyType>>();

                item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<TDBModel, bool>>>()))
                    .ReturnsAsync(dbObject);
                item.Setup(gg => gg.GetByIdAsync(It.IsAny<PrimarykeyType>()))
                 .ReturnsAsync(dbObject);

                return item.Object;
            });
            var localService = GetBusinessService(uofm.Object);
            //act
            Func<Task> act = () => localService.ModifyAsync(incommingObject);
            //assert
            ServiceModelValidationException exception = await Assert.ThrowsAsync<ServiceModelValidationException>(act);

            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Error),
             It.Is<EventId>(l => l == service.LogBaseID + 3),
             It.Is<It.IsAnyType>((v, t) => true),
             It.IsAny<ServiceModelValidationException>(),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            serviceLogger.VerifyNoOtherCalls();


            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModifiedBy_IsEmpty &&
            uu.FieldName == nameof(incommingObject.LastModifiedBy)), "Validation error for field '" + nameof(incommingObject.LastModifiedBy) + "' expected when empty on modify");

            Assert.False(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_CanNotBeBeforeCreationOnModify &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' of incoming object expected when is before '" + nameof(dbObject.CreationTime) + "' field of dbObject");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_CanNotBeBeforeLastModificationOfDBObjectOnModify &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' of incoming object expected when is before '" + nameof(dbObject.LastModificationTime) + "' field of dbObject");


        }
        #endregion



    }
}
