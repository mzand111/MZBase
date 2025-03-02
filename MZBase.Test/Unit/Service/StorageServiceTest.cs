using AutoBogus;
using Microsoft.Extensions.Logging;
using Moq;
using MZBase.Domain;
using MZBase.Infrastructure;
using MZBase.Infrastructure.Service;
using MZBase.Infrastructure.Service.Exceptions;
using MZSimpleDynamicLinq.Core;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace MZBase.Test.Unit.Service
{
    public abstract class StorageServiceTest<StorageService, TUnitOfWork, TModel, TDBModel, PrimaryKeyType>
        where StorageService : IStorageBusinessService<TModel, PrimaryKeyType>
       where TUnitOfWork : class, IDynamicTestableUnitOfWorkAsync
        where TModel : Model<PrimaryKeyType>
          where TDBModel : TModel, IConvertibleDBModelEntity<TModel>, new()
       where PrimaryKeyType : struct

    {
        protected Moq.Mock<ILogger<TModel>> serviceLogger;
        protected StorageService service;
        protected Moq.Mock<TUnitOfWork> modelUoWMock;

        public StorageServiceTest()
        {

        }


        public abstract BaseStorageBusinessService<TModel, PrimaryKeyType> GetBusinessService(IDynamicTestableUnitOfWorkAsync uof);

        [Fact]
        public virtual async Task Base_LogBaseIDShouldBeInitialized()
        {
            int obj = service.LogBaseID;
            Assert.NotEqual(0, obj);//,"Retrieved object should not be null"
        }

        #region Retrieve
        [Fact]
        public virtual async Task Base_Get_Success()
        {
            //arrange
            var fakeID = AutoFaker.Generate<PrimaryKeyType>();
            Mock<TDBModel> dbObjectMock = new Mock<TDBModel>() { };
            dbObjectMock.SetupProperty(uu => uu.ID, fakeID);


            TDBModel dbObject = dbObjectMock.Object;
            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            uofm.Setup(uu => uu.GetRepo<TModel, TDBModel, PrimaryKeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, TDBModel, PrimaryKeyType>>();

                //item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<Model, bool>>>()))
                //    .ReturnsAsync(dbObject);
                item.Setup(gg => gg.GetByIdAsync(It.Is<PrimaryKeyType>(uu => uu.Equals(fakeID))))
                 .ReturnsAsync(dbObject);

                return item.Object;
            });
            var localService = GetBusinessService(uofm.Object);
            //act
            TModel obj = await localService.RetrieveByIdAsync(fakeID);

            Assert.NotEqual(null, obj);//,"Retrieved object should not be null"
            Assert.Equal(fakeID, obj.ID);
            //assert
            serviceLogger.Verify(
               x => x.Log(
               It.Is<LogLevel>(l => l == LogLevel.Information),
               It.Is<EventId>(l => l == service.LogBaseID + 2),
               It.Is<It.IsAnyType>((v, t) => true),
               It.Is<Exception>(uu => uu == null),
               It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            serviceLogger.VerifyNoOtherCalls();
        }
        [Fact]
        public virtual async Task Base_Get_FailNotFound()
        {
            //arrange
            var fakeID1 = AutoFaker.Generate<PrimaryKeyType>();
            var fakeID2 = AutoFaker.Generate<PrimaryKeyType>();

            Mock<TDBModel> dbObjectMock = new Mock<TDBModel>() { };
            dbObjectMock.SetupProperty(uu => uu.ID, fakeID1);


            TDBModel dbObject = dbObjectMock.Object;
            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            uofm.Setup(uu => uu.GetRepo<TModel, TDBModel, PrimaryKeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, TDBModel, PrimaryKeyType>>();

                //item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<Model, bool>>>()))
                //    .ReturnsAsync(dbObject);
                item.Setup(gg => gg.GetByIdAsync(It.Is<PrimaryKeyType>(uu => uu.Equals(fakeID1))))
                 .ReturnsAsync(dbObject);

                return item.Object;
            });
            var localService = GetBusinessService(uofm.Object);
            //act
            Func<Task> act = () => localService.RetrieveByIdAsync(fakeID2);
            //assert
            ServiceObjectNotFoundException exception = await Assert.ThrowsAsync<ServiceObjectNotFoundException>(act);

            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Error),
             It.Is<EventId>(l => l == service.LogBaseID + 2),
             It.Is<It.IsAnyType>((v, t) => true),
             It.IsAny<ServiceObjectNotFoundException>(),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            serviceLogger.VerifyNoOtherCalls();
        }
        [Fact]
        public virtual async Task Base_Get_FailStorageError()
        {
            //arrange
            var fakeID1 = AutoFaker.Generate<PrimaryKeyType>();
            var fakeID2 = AutoFaker.Generate<PrimaryKeyType>();

            Mock<TDBModel> dbObjectMock = new Mock<TDBModel>() { };
            dbObjectMock.SetupProperty(uu => uu.ID, fakeID1);


            TModel dbObject = dbObjectMock.Object;
            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            uofm.Setup(uu => uu.GetRepo<TModel, TDBModel, PrimaryKeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, TDBModel, PrimaryKeyType>>();

                //item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<Model, bool>>>()))
                //    .ReturnsAsync(dbObject);
                item.Setup(gg => gg.GetByIdAsync(It.IsAny<PrimaryKeyType>()))
                 .ThrowsAsync(new Exception());

                return item.Object;
            });
            var localService = GetBusinessService(uofm.Object);
            //act
            Func<Task> act = () => localService.RetrieveByIdAsync(fakeID2);
            //assert
            ServiceStorageException exception = await Assert.ThrowsAsync<ServiceStorageException>(act);

            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Error),
             It.Is<EventId>(l => l == service.LogBaseID + 2),
             It.Is<It.IsAnyType>((v, t) => true),
             It.IsAny<ServiceStorageException>(),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            serviceLogger.VerifyNoOtherCalls();
        }
        #endregion


        #region Retrieve
        [Fact]
        public virtual async Task Base_GetMultiple_Success()
        {
            //arrange           
            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            uofm.Setup(uu => uu.GetRepo<TModel, TDBModel, PrimaryKeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, TDBModel, PrimaryKeyType>>();

                item.Setup(gg => gg.AllItemsAsync(It.IsAny<LinqDataRequest>()))
                 .ReturnsAsync(new LinqDataResult<TModel>());

                return item.Object;
            });
            var localService = GetBusinessService(uofm.Object);
            //act
            var obj = await localService.ItemsAsync(new LinqDataRequest());

            Assert.NotEqual(null, obj);//,"Retrieved object should not be null"

            //assert
            serviceLogger.Verify(
               x => x.Log(
               It.Is<LogLevel>(l => l == LogLevel.Information),
               It.Is<EventId>(l => l == service.LogBaseID + 5),
               It.Is<It.IsAnyType>((v, t) => true),
               It.Is<Exception>(uu => uu == null),
               It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            serviceLogger.VerifyNoOtherCalls();
        }
        [Fact]
        public virtual async Task Base_GetMultiple_StorageError()
        {
            //arrange           
            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            uofm.Setup(uu => uu.GetRepo<TModel, TDBModel, PrimaryKeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, TDBModel, PrimaryKeyType>>();

                item.Setup(gg => gg.AllItemsAsync(It.IsAny<LinqDataRequest>()))
                 .Throws(new Exception());

                return item.Object;
            });
            var localService = GetBusinessService(uofm.Object);
            //act
            Func<Task> act = () => localService.ItemsAsync(new LinqDataRequest());
            //assert
            ServiceStorageException exception = await Assert.ThrowsAsync<ServiceStorageException>(act);

            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Error),
             It.Is<EventId>(l => l == service.LogBaseID + 5),
             It.Is<It.IsAnyType>((v, t) => true),
             It.IsAny<ServiceStorageException>(),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            serviceLogger.VerifyNoOtherCalls();
        }
        #endregion
        #region Add
        [Fact]
        public virtual async Task Base_Add_ShouldRaise_ServiceArgumentNullException_OnNullInput()
        {
            //arrange
            TModel entity = default(TModel);
            //act
            Func<Task> act = () => service.AddAsync(entity);
            //assert
            ServiceArgumentNullException exception = await Assert.ThrowsAsync<ServiceArgumentNullException>(act);
            exception.Message.StartsWith("Input parameter was null");
            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Error),
             It.Is<EventId>(l => l == service.LogBaseID + 1),
             It.Is<It.IsAnyType>((v, t) => true),
             It.IsAny<ServiceArgumentNullException>(),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            serviceLogger.VerifyNoOtherCalls();
        }
        protected virtual async Task Base_Add_ShouldSuccess_WhenValid(TModel item)
        {
            await service.AddAsync(item);
            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Information),
             It.Is<EventId>(l => l == service.LogBaseID + 1),
             It.Is<It.IsAnyType>((v, t) => true),
             It.Is<Exception>(uu => uu == null),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            modelUoWMock.Verify(uu => uu.CommitAsync(), Times.Once, "Save method from UoW expected to be called once");
        }
        protected virtual async Task Base_Add_ShouldReturnServiceStorageException_WhenErrorOnDB(TModel item)
        {
            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            uofm.Setup(uu => uu.GetRepo<TModel, TDBModel, PrimaryKeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, TDBModel, PrimaryKeyType>>();
                return item.Object;
            });
            uofm.Setup(uu => uu.CommitAsync()).Throws<Exception>();
            var localService = GetBusinessService(uofm.Object);
            //act
            Func<Task> act = () => localService.AddAsync(item);
            //assert
            ServiceStorageException exception = await Assert.ThrowsAsync<ServiceStorageException>(act);
            serviceLogger.Verify(
            x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Error),
            It.Is<EventId>(l => l == service.LogBaseID + 1),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            uofm.Verify(uu => uu.CommitAsync(), Times.Once, "Save method from UoW expected to be called once");

        }
        #endregion

        #region Modify
        [Fact]
        public virtual async Task Base_Modify_ShouldRaise_ServiceArgumentNullException_OnNullInput()
        {
            //arrange
            TModel entity = default(TModel);
            //act
            Func<Task> act = () => service.ModifyAsync(entity);
            //assert
            ServiceArgumentNullException exception = await Assert.ThrowsAsync<ServiceArgumentNullException>(act);
            exception.Message.StartsWith("Input parameter was null");
            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Error),
             It.Is<EventId>(l => l == service.LogBaseID + 3),
             It.Is<It.IsAnyType>((v, t) => true),
             It.IsAny<ServiceArgumentNullException>(),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            serviceLogger.VerifyNoOtherCalls();
        }
        [Fact]
        public virtual async Task Base_Modify_ShouldRaise_ServiceObjectNotFoundException_WhenIDNotFound_BeforeAnyValidation()
        {
            //arrange           
            Mock<TModel> incommingObjectMock = new Mock<TModel>();
            TModel incommingObject = incommingObjectMock.Object;
            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            uofm.Setup(uu => uu.GetRepo<TModel, TDBModel, PrimaryKeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, TDBModel, PrimaryKeyType>>();

                item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<TDBModel, bool>>>()))
                    .ReturnsAsync(default(TDBModel));
                item.Setup(gg => gg.GetByIdAsync(It.IsAny<PrimaryKeyType>()))
                 .ReturnsAsync(default(TDBModel));

                return item.Object;
            });
            var localService = GetBusinessService(uofm.Object);
            //act
            Func<Task> act = () => localService.ModifyAsync(incommingObject);
            //assert
            ServiceObjectNotFoundException exception = await Assert.ThrowsAsync<ServiceObjectNotFoundException>(act);

            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Error),
             It.Is<EventId>(l => l == service.LogBaseID + 3),
             It.Is<It.IsAnyType>((v, t) => true),
             It.IsAny<ServiceObjectNotFoundException>(),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            serviceLogger.VerifyNoOtherCalls();

        }
        protected virtual async Task Base_Modify_ShouldSuccess_WhenValid(TModel incommingObject, TDBModel storageObject)
        {
            //arrange
            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            uofm.Setup(uu => uu.GetRepo<TModel, TDBModel, PrimaryKeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, TDBModel, PrimaryKeyType>>();

                item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<TDBModel, bool>>>()))
                    .ReturnsAsync(storageObject);
                item.Setup(gg => gg.GetByIdAsync(It.IsAny<PrimaryKeyType>()))
                 .ReturnsAsync(storageObject);

                return item.Object;
            });
            var localService = GetBusinessService(uofm.Object);
            //act
            await localService.ModifyAsync(incommingObject);
            //assert
            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Information),
             It.Is<EventId>(l => l == service.LogBaseID + 3),
             It.Is<It.IsAnyType>((v, t) => true),
             It.Is<Exception>(uu => uu == null),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            uofm.Verify(uu => uu.CommitAsync(), Times.Once, "Save method from UoW expected to be called once");
        }
        protected virtual async Task Base_Modify_ShouldReturnServiceStorageException_WhenErrorOnDB(TModel incommingObject, TDBModel storageObject)
        {
            //arrange
            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            uofm.Setup(uu => uu.GetRepo<TModel, TDBModel, PrimaryKeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, TDBModel, PrimaryKeyType>>();

                item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<TDBModel, bool>>>()))
                    .ReturnsAsync(storageObject);
                item.Setup(gg => gg.GetByIdAsync(It.IsAny<PrimaryKeyType>()))
                 .ReturnsAsync(storageObject);

                return item.Object;
            });
            //Inner object (UoW or deeper ones) throws any exception on save
            uofm.Setup(uu => uu.CommitAsync()).Throws<Exception>();
            var localService = GetBusinessService(uofm.Object);
            //act
            Func<Task> act = () => localService.ModifyAsync(incommingObject);
            //assert
            ServiceStorageException exception = await Assert.ThrowsAsync<ServiceStorageException>(act);
            //assert
            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Error),
             It.Is<EventId>(l => l == service.LogBaseID + 3),
             It.Is<It.IsAnyType>((v, t) => true),
             It.Is<Exception>(uu => uu == null),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            uofm.Verify(uu => uu.CommitAsync(), Times.Once, "Save method from UoW expected to be called once");

        }
        #endregion

        #region Remove
        [Fact]
        public virtual async Task Base_Remove_ShouldRaise_ServiceObjectNotFoundException_WhenIDNotFound_BeforeAnyValidation()
        {
            //arrange           

            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            uofm.Setup(uu => uu.GetRepo<TModel, TDBModel, PrimaryKeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, TDBModel, PrimaryKeyType>>();

                item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<TDBModel, bool>>>()))
                    .ReturnsAsync(default(TDBModel));
                item.Setup(gg => gg.GetByIdAsync(It.IsAny<PrimaryKeyType>()))
                 .ReturnsAsync(default(TDBModel));

                return item.Object;
            });
            var localService = GetBusinessService(uofm.Object);
            //act
            Func<Task> act = () => localService.RemoveByIdAsync(default(PrimaryKeyType));
            //assert
            ServiceObjectNotFoundException exception = await Assert.ThrowsAsync<ServiceObjectNotFoundException>(act);

            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Error),
             It.Is<EventId>(l => l == service.LogBaseID + 4),
             It.Is<It.IsAnyType>((v, t) => true),
             It.IsAny<ServiceObjectNotFoundException>(),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            serviceLogger.VerifyNoOtherCalls();

        }
        [Fact]
        public virtual async Task Base_Remove_ShouldSuccess_WhenValid()
        {
            //arrange
            Mock<TDBModel> dbObjectMock = new Mock<TDBModel>();
            TDBModel dbObject = dbObjectMock.Object;

            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            uofm.Setup(uu => uu.GetRepo<TModel, TDBModel, PrimaryKeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, TDBModel, PrimaryKeyType>>();

                item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<TDBModel, bool>>>()))
                    .ReturnsAsync(dbObject);
                item.Setup(gg => gg.GetByIdAsync(It.IsAny<PrimaryKeyType>()))
                 .ReturnsAsync(dbObject);

                return item.Object;
            });
            var localService = GetBusinessService(uofm.Object);
            //act
            await localService.RemoveByIdAsync(default(PrimaryKeyType));
            //assert
            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Information),
             It.Is<EventId>(l => l == service.LogBaseID + 4),
             It.Is<It.IsAnyType>((v, t) => true),
             It.Is<Exception>(uu => uu == null),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            uofm.Verify(uu => uu.CommitAsync(), Times.Once, "Save method from UoW expected to be called once");
        }
        [Fact]
        public virtual async Task Base_ModifyRemove_ShouldReturnServiceStorageException_WhenErrorOnDB()
        {
            //arrange
            Mock<TDBModel> dbObjectMock = new Mock<TDBModel>();
            TDBModel dbObject = dbObjectMock.Object;
            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            uofm.Setup(uu => uu.GetRepo<TModel, TDBModel, PrimaryKeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, TDBModel, PrimaryKeyType>>();

                item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<TDBModel, bool>>>()))
                    .ReturnsAsync(dbObject);
                item.Setup(gg => gg.GetByIdAsync(It.IsAny<PrimaryKeyType>()))
                 .ReturnsAsync(dbObject);

                return item.Object;
            });
            //Inner object (UoW or deeper ones) throws any exception on save
            uofm.Setup(uu => uu.CommitAsync()).Throws<Exception>();
            var localService = GetBusinessService(uofm.Object);
            //act
            Func<Task> act = () => localService.RemoveByIdAsync(default(PrimaryKeyType));
            //assert
            ServiceStorageException exception = await Assert.ThrowsAsync<ServiceStorageException>(act);
            //assert
            serviceLogger.Verify(
             x => x.Log(
             It.Is<LogLevel>(l => l == LogLevel.Error),
             It.Is<EventId>(l => l == service.LogBaseID + 4),
             It.Is<It.IsAnyType>((v, t) => true),
             It.Is<Exception>(uu => uu == null),
             It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
            uofm.Verify(uu => uu.CommitAsync(), Times.Once, "Save method from UoW expected to be called once");

        }
        #endregion

    }
}
