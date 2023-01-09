# MZBase
A general base classes to be used in enterprise projects, specially projects with microservice architecture. This library widely uses MZSimpleDynamicLinq (https://github.com/mzand111/MZSimpleDynamicLinq)


You can find the nuget packages here 


https://www.nuget.org/packages/MZBase.Domain/

A simple library for base classes of common usabilities needed in enterprise software. 
This library contains classes that are used as base for domain and entity classes.


https://www.nuget.org/packages/MZBase.Infrastructure/

Contains:

1- Common interfaces such as:

  -IRepositoryAsync<ModelItem, T>, IPSFRepositoryAsync <ModelItem, T> and ILDRCompatibleRepositoryAsync<ModelItem, T>
  
  -IUnitOfWorkAsync and IDynamicTestableUnitOfWorkAsync

2- Base classes to be used for service layers:

  -BusinessService class
  
  -IStorageBusinessService<T, PrimarykeyType> and StorageBusinessService implementation
  
  -Service exception classes


https://www.nuget.org/packages/MZBase.EntityFrameworkCore/

Contains EFCore implementations of MZBase.Infrastructure interfaces including

  -RepositoryAsync<DBModelEntity,DomainModelEntity, PrimaryKeyType>
  
  -LDRCompatibleRepositoryAsync<DBModelEntity,DomainModelEntity, PrimaryKeyType>
  
  -UnitOfWorkAsync<T>


https://www.nuget.org/packages/MZBase.Microservices/

Some classes to faciliate microservice communications:

  -ServiceMediatorBase<T>: Wraps HttpClient and do Gets,Posts,Puts and Deletes with exception handling, serialization, logging and authentication. 

https://www.nuget.org/packages/MZBase.EntityFrameworkCore.Sql/

Contains:

  -A helper to transfer description attributes of entity classes to extended properties (MS_Description) of related table/column of databse for any ef core DbContext. This changes could be used by any third party tool like Redgate SqlDoc to generate Database documentation.
