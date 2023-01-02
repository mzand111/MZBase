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

using AutoBogus;
using Bogus;
using MZSimpleDynamicLinq.Core;

namespace MZBase.Test.Unit.Service
{
    public abstract class StorageServiceTest<StorageService, TUnitOfWork, TModel, PrimarykeyType>
        where StorageService : IStorageBusinessService<TModel, PrimarykeyType>
       where TUnitOfWork : class, IDynamicTestableUnitOfWorkAsync
        where TModel : Model<PrimarykeyType>
       where PrimarykeyType : struct

    {
        protected Moq.Mock<ILogger<TModel>> serviceLogger;
        protected StorageService service;
        protected Moq.Mock<TUnitOfWork> modelUoWMock;

        public StorageServiceTest()
        {

        }


        public abstract StorageBusinessService<TModel, PrimarykeyType> GetBusinessService(IDynamicTestableUnitOfWorkAsync uof);

        [Fact]
        public virtual async Task Base_LogBaseIDShouldBeInitialized()
        {
            int obj = service.LogBaseID;
            Assert.NotEqual(0, obj);//,"Retrived object should not be null"
        }

        #region Retrieve
        [Fact]
        public virtual async Task Base_Get_Success()
        {
            //arrange
            var fakeID = AutoFaker.Generate<PrimarykeyType>();
            Mock<TModel> dbObjectMock = new Mock<TModel>() { };
            dbObjectMock.SetupProperty(uu => uu.ID, fakeID);


            TModel dbObject = dbObjectMock.Object;
            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            uofm.Setup(uu => uu.GetRepo<TModel, PrimarykeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, PrimarykeyType>>();

                //item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<Model, bool>>>()))
                //    .ReturnsAsync(dbObject);
                item.Setup(gg => gg.GetByIdAsync(It.Is<PrimarykeyType>(uu => uu.Equals(fakeID))))
                 .ReturnsAsync(dbObject);

                return item.Object;
            });
            var localService = GetBusinessService(uofm.Object);
            //act
            TModel obj = await localService.RetrieveByIdAsync(fakeID);

            Assert.NotEqual(null, obj);//,"Retrived object should not be null"
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
            var fakeID1 = AutoFaker.Generate<PrimarykeyType>();
            var fakeID2 = AutoFaker.Generate<PrimarykeyType>();

            Mock<TModel> dbObjectMock = new Mock<TModel>() { };
            dbObjectMock.SetupProperty(uu => uu.ID, fakeID1);


            TModel dbObject = dbObjectMock.Object;
            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            uofm.Setup(uu => uu.GetRepo<TModel, PrimarykeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, PrimarykeyType>>();

                //item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<Model, bool>>>()))
                //    .ReturnsAsync(dbObject);
                item.Setup(gg => gg.GetByIdAsync(It.Is<PrimarykeyType>(uu => uu.Equals(fakeID1))))
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
            var fakeID1 = AutoFaker.Generate<PrimarykeyType>();
            var fakeID2 = AutoFaker.Generate<PrimarykeyType>();

            Mock<TModel> dbObjectMock = new Mock<TModel>() { };
            dbObjectMock.SetupProperty(uu => uu.ID, fakeID1);


            TModel dbObject = dbObjectMock.Object;
            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            uofm.Setup(uu => uu.GetRepo<TModel, PrimarykeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, PrimarykeyType>>();

                //item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<Model, bool>>>()))
                //    .ReturnsAsync(dbObject);
                item.Setup(gg => gg.GetByIdAsync(It.IsAny<PrimarykeyType>()))
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
            uofm.Setup(uu => uu.GetRepo<TModel, PrimarykeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, PrimarykeyType>>();

                item.Setup(gg => gg.AllItemsAsync(It.IsAny<LinqDataRequest>()))
                 .ReturnsAsync(new LinqDataResult<TModel>());

                return item.Object;
            });
            var localService = GetBusinessService(uofm.Object);
            //act
            var obj = await localService.ItemsAsync(new LinqDataRequest());

            Assert.NotEqual(null, obj);//,"Retrived object should not be null"

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
            uofm.Setup(uu => uu.GetRepo<TModel, PrimarykeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, PrimarykeyType>>();

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
            uofm.Setup(uu => uu.GetRepo<TModel, PrimarykeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, PrimarykeyType>>();
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
            uofm.Setup(uu => uu.GetRepo<TModel, PrimarykeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, PrimarykeyType>>();

                item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<TModel, bool>>>()))
                    .ReturnsAsync(default(TModel));
                item.Setup(gg => gg.GetByIdAsync(It.IsAny<PrimarykeyType>()))
                 .ReturnsAsync(default(TModel));

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
        protected virtual async Task Base_Modify_ShouldSuccess_WhenValid(TModel incommingObject, TModel storageObject)
        {
            //arrange
            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            uofm.Setup(uu => uu.GetRepo<TModel, PrimarykeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, PrimarykeyType>>();

                item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<TModel, bool>>>()))
                    .ReturnsAsync(storageObject);
                item.Setup(gg => gg.GetByIdAsync(It.IsAny<PrimarykeyType>()))
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
        protected virtual async Task Base_Modify_ShouldReturnServiceStorageException_WhenErrorOnDB(TModel incommingObject, TModel storageObject)
        {
            //arrange
            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            uofm.Setup(uu => uu.GetRepo<TModel, PrimarykeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, PrimarykeyType>>();

                item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<TModel, bool>>>()))
                    .ReturnsAsync(storageObject);
                item.Setup(gg => gg.GetByIdAsync(It.IsAny<PrimarykeyType>()))
                 .ReturnsAsync(storageObject);

                return item.Object;
            });
            //Inner object (UoW or deeper ones) throws any axception on save
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
            uofm.Setup(uu => uu.GetRepo<TModel, PrimarykeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, PrimarykeyType>>();

                item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<TModel, bool>>>()))
                    .ReturnsAsync(default(TModel));
                item.Setup(gg => gg.GetByIdAsync(It.IsAny<PrimarykeyType>()))
                 .ReturnsAsync(default(TModel));

                return item.Object;
            });
            var localService = GetBusinessService(uofm.Object);
            //act
            Func<Task> act = () => localService.RemoveByIdAsync(default(PrimarykeyType));
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
            Mock<TModel> dbObjectMock = new Mock<TModel>();
            TModel dbObject = dbObjectMock.Object;

            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            uofm.Setup(uu => uu.GetRepo<TModel, PrimarykeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, PrimarykeyType>>();

                item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<TModel, bool>>>()))
                    .ReturnsAsync(dbObject);
                item.Setup(gg => gg.GetByIdAsync(It.IsAny<PrimarykeyType>()))
                 .ReturnsAsync(dbObject);

                return item.Object;
            });
            var localService = GetBusinessService(uofm.Object);
            //act
            await localService.RemoveByIdAsync(default(PrimarykeyType));
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
            Mock<TModel> dbObjectMock = new Mock<TModel>();
            TModel dbObject = dbObjectMock.Object;
            Moq.Mock<TUnitOfWork> uofm = new Mock<TUnitOfWork>() { };
            uofm.Setup(uu => uu.GetRepo<TModel, PrimarykeyType>()).Returns(() =>
            {
                var item = new Mock<ILDRCompatibleRepositoryAsync<TModel, PrimarykeyType>>();

                item.Setup(gg => gg.FirstOrDefaultAsync(It.IsAny<Expression<Func<TModel, bool>>>()))
                    .ReturnsAsync(dbObject);
                item.Setup(gg => gg.GetByIdAsync(It.IsAny<PrimarykeyType>()))
                 .ReturnsAsync(dbObject);

                return item.Object;
            });
            //Inner object (UoW or deeper ones) throws any axception on save
            uofm.Setup(uu => uu.CommitAsync()).Throws<Exception>();
            var localService = GetBusinessService(uofm.Object);
            //act
            Func<Task> act = () => localService.RemoveByIdAsync(default(PrimarykeyType));
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
