using MZBase.Infrastructure.Service;
using Microsoft.Extensions.Logging;
using Moq;
using MZBase.Domain;
using MZBase.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using MZBase.Infrastructure.Service.Exceptions;

namespace MZBase.Test.Unit.Service
{
    public abstract class AuditableStorageServiceTest<StorageService, TUnitOfWork, Model, PrimarykeyType> : StorageServiceTest<StorageService, TUnitOfWork, Model, PrimarykeyType>
        where StorageService : StorageBusinessService<Model, PrimarykeyType>
       where TUnitOfWork : class, IDynamicTestableUnitOfWorkAsync
        where Model : Auditable<PrimarykeyType>
       where PrimarykeyType : struct

    {
        protected virtual void AddMockSetupsCommon(Mock<Model> mock)
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
            uu.FieldName == nameof(entity.CreatedBy)), "Validation error for field '" + nameof(entity.CreatedBy) + "' expected when epmty on add");
            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModifiedBy_IsEmpty &&
            uu.FieldName == nameof(entity.LastModifiedBy)), "Validation error for field '" + nameof(entity.LastModifiedBy) + "' expected when epmty on add");

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
            uu.FieldName == nameof(entity.LastModifiedBy)), "Validation error for field '" + nameof(entity.LastModifiedBy) + "' expected when epmty on add");
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
            uu.FieldName == nameof(entity.CreatedBy)), "Validation error for field '" + nameof(entity.CreatedBy) + "' expected when epmty on add");
            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModifiedBy_IsEmpty &&
            uu.FieldName == nameof(entity.LastModifiedBy)), "Validation error for field '" + nameof(entity.LastModifiedBy) + "' expected when epmty on add");

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
            uu.FieldName == nameof(entity.LastModifiedBy)), "Validation error for field '" + nameof(entity.LastModifiedBy) + "' expected when epmty on add");
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
           uu.FieldName == nameof(entity.CreatedBy)), "Validation error for field '" + nameof(entity.CreatedBy) + "' expected when epmty on add");
            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModifiedBy_IsEmpty &&
            uu.FieldName == nameof(entity.LastModifiedBy)), "Validation error for field '" + nameof(entity.LastModifiedBy) + "' expected when epmty on add");

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
            Mock<Model> dbObjectMock = new Mock<Model>();


            //We assume that the object in database has the data consistensy
            var f = DateTime.Now;
            dbObjectMock.Setup<string>(uu => uu.CreatedBy).Returns("someCreatorUser");
            dbObjectMock.Setup<string>(uu => uu.LastModifiedBy).Returns("someCreatorUser");
            dbObjectMock.Setup<DateTime>(uu => uu.CreationTime).Returns(f);
            dbObjectMock.Setup<DateTime>(uu => uu.LastModificationTime).Returns(f);
            AddMockSetupsCommon(dbObjectMock);
            Model dbObject = dbObjectMock.Object;

            Mock<Model> incommingObjectMock = new Mock<Model>();
            AddMockSetupsCommon(incommingObjectMock);
            Model incommingObject = incommingObjectMock.Object;

            //We set nothing here for the incoming object

            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            AddUoWMockSetupsCommon(uofm);
            uofm.Setup(uu => uu.GetRepo<Model, PrimarykeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<Model, PrimarykeyType>>();

                item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<Model, bool>>>()))
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
            uu.FieldName == nameof(incommingObject.LastModifiedBy)), "Validation error for field '" + nameof(incommingObject.LastModifiedBy) + "' expected when epmty on modify");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_ValueIsNotValid &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' expected when epmty on modify");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_CanNotBeBeforeCreationOnModify &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' of incomming object expected when is before '" + nameof(dbObject.CreationTime) + "' field of dbObject");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_CanNotBeBeforeLastModificationOfDBObjectOnModify &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' of incomming object expected when is before '" + nameof(dbObject.LastModificationTime) + "' field of dbObject");

        }
        [Fact]
        public virtual async Task Auditable_Modify_ShouldRaise_ServiceModelValidationException_WhenJustLastModifiedBySetOnModify()
        {
            //arrange           
            Mock<Model> dbObjectMock = new Mock<Model>();

            //We assume that the object in database has the data consistensy
            var f = DateTime.Now;
            dbObjectMock.Setup<DateTime>(uu => uu.CreationTime).Returns(f);
            dbObjectMock.Setup<DateTime>(uu => uu.LastModificationTime).Returns(f);
            dbObjectMock.Setup<string>(uu => uu.CreatedBy).Returns("someCreatorUser");
            dbObjectMock.Setup<string>(uu => uu.LastModifiedBy).Returns("someCreatorUser");
            Model dbObject = dbObjectMock.Object;


            Mock<Model> incommingObjectMock = new Mock<Model>();
            //We set some fields of the incoming object
            incommingObjectMock.Setup<string>(uu => uu.LastModifiedBy).Returns("someOtherUser");
            Model incommingObject = incommingObjectMock.Object;


            // incommingObject.LastModifiedBy = "someOtherUser";

            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            AddUoWMockSetupsCommon(uofm);
            uofm.Setup(uu => uu.GetRepo<Model, PrimarykeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<Model, PrimarykeyType>>();

                item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<Model, bool>>>()))
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
            uu.FieldName == nameof(incommingObject.LastModifiedBy)), "Validation error for field '" + nameof(incommingObject.LastModifiedBy) + "' expected when epmty on modify");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_ValueIsNotValid &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' was not expected when not epmty on modify");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_CanNotBeBeforeCreationOnModify &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' of incomming object expected when is before '" + nameof(dbObject.CreationTime) + "' field of dbObject");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_CanNotBeBeforeLastModificationOfDBObjectOnModify &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' of incomming object expected when is before '" + nameof(dbObject.LastModificationTime) + "' field of dbObject");

        }
        [Fact]
        public virtual async Task Auditable_Modify_ShouldRaise_ServiceModelValidationException_WhenJustLastModificationTimeSetOnAnOlderThanBothDBTimesOnModify()
        {
            Mock<Model> dbObjectMock = new Mock<Model>();

            //We assume that the object in database has the data consistensy
            var f = DateTime.Now;
            dbObjectMock.Setup<DateTime>(uu => uu.CreationTime).Returns(f);
            dbObjectMock.Setup<DateTime>(uu => uu.LastModificationTime).Returns(f);
            dbObjectMock.Setup<string>(uu => uu.CreatedBy).Returns("someCreatorUser");
            dbObjectMock.Setup<string>(uu => uu.LastModifiedBy).Returns("someCreatorUser");
            Model dbObject = dbObjectMock.Object;


            Mock<Model> incommingObjectMock = new Mock<Model>();
            //We set some fields of the incoming object
            incommingObjectMock.Setup(uu => uu.LastModificationTime).Returns(f.AddDays(-1));
            Model incommingObject = incommingObjectMock.Object;


            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            AddUoWMockSetupsCommon(uofm);
            uofm.Setup(uu => uu.GetRepo<Model, PrimarykeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<Model, PrimarykeyType>>();

                item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<Model, bool>>>()))
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
             uu.FieldName == nameof(incommingObject.LastModifiedBy)), "Validation error for field '" + nameof(incommingObject.LastModifiedBy) + "' expected when epmty on modify");

            Assert.False(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_ValueIsNotValid &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' was not expected when not epmty on modify");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_CanNotBeBeforeCreationOnModify &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' of incomming object expected when is before '" + nameof(dbObject.CreationTime) + "' field of dbObject");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_CanNotBeBeforeLastModificationOfDBObjectOnModify &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' of incomming object expected when is before '" + nameof(dbObject.LastModificationTime) + "' field of dbObject");


        }
        [Fact]
        public virtual async Task Auditable_Modify_ShouldRaise_ServiceModelValidationException_WhenJustLastModificationTimeSetOnAnOlderLastModificationTimeFromDBOnModify()
        {
            //arrange           
            Mock<Model> dbObjectMock = new Mock<Model>();
            //We assume that the object in database has the data consistensy
            dbObjectMock.Setup<string>(uu => uu.CreatedBy).Returns("someCreatorUser");
            dbObjectMock.Setup<string>(uu => uu.LastModifiedBy).Returns("someCreatorUser");
            var f = DateTime.Now;
            dbObjectMock.Setup<DateTime>(uu => uu.CreationTime).Returns(f.AddDays(-2));
            dbObjectMock.Setup<DateTime>(uu => uu.LastModificationTime).Returns(f);

            Model dbObject = dbObjectMock.Object;



            Mock<Model> incommingObjectMock = new Mock<Model>();
            //We set some fields of the incoming object
            incommingObjectMock.Setup<DateTime>(uu => uu.LastModificationTime).Returns(f.AddDays(-1));
            Model incommingObject = incommingObjectMock.Object;




            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            AddUoWMockSetupsCommon(uofm);
            uofm.Setup(uu => uu.GetRepo<Model, PrimarykeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<Model, PrimarykeyType>>();

                item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<Model, bool>>>()))
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
            uu.FieldName == nameof(incommingObject.LastModifiedBy)), "Validation error for field '" + nameof(incommingObject.LastModifiedBy) + "' expected when epmty on modify");

            Assert.False(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_CanNotBeBeforeCreationOnModify &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' of incomming object expected when is before '" + nameof(dbObject.CreationTime) + "' field of dbObject");

            Assert.True(exception.ValidationErrors.Any(uu => uu.Code == (int)ModelFieldValidationResultCode.LastModificationTime_CanNotBeBeforeLastModificationOfDBObjectOnModify &&
            uu.FieldName == nameof(incommingObject.LastModificationTime)), "Validation error for field '" + nameof(incommingObject.LastModificationTime) + "' of incomming object expected when is before '" + nameof(dbObject.LastModificationTime) + "' field of dbObject");


        }
        #endregion



    }
}
