# MZBase
In data-driven applications, there are common requirements that need to be met in the code. One of the key requirements is to separate the domain logic from data storage and retrieval (abstraction of the data layer). To achieve this, we must define the classes of each layer to align with SOLID principles.

Moreover, patterns such as the repository pattern and Unit of Work (UoW) are frequently employed in these applications. The MZBase libraries offer a straightforward solution to these challenges. We have strived to keep it simple and applicable throughout the years, and this principle has been maintained even in version 2. Consequently, you should be able to use MZBase to implement anything from a simple data-driven application to a sophisticated multi-microservice system.

This library widely uses MZSimpleDynamicLinq (https://github.com/mzand111/MZSimpleDynamicLinq)


# Related nuget packages
## MZBase.Domain
https://www.nuget.org/packages/MZBase.Domain/

A simple library for base classes of common usabilities needed in enterprise software. 
This library contains classes that are used as base for domain and entity classes.

## MZBase.Infrastructure
https://www.nuget.org/packages/MZBase.Infrastructure/

Contains:

1- Common interfaces such as:

  - Interfaces to implement repository pattern in your code in an integrated manner: `IRepositoryAsync<ModelItem, DBModelEntity, T>`, `IPSFRepositoryAsync <ModelItem, DBModelEntity, T>` and `ILDRCompatibleRepositoryAsync<ModelItem, DBModelEntity, T>`
  
  - Interfaces to implement Unit of Work pattern and testing them easily: `IUnitOfWorkAsync` and `IDynamicTestableUnitOfWorkAsync`

2- Base classes to be used for service layers:

  - `BaseBusinessService` An abstract base class designed to be used as the root of every business service class . 

  - `BaseStorageBusinessService<Model, PrimaryKeyType>` class
  
  - `IStorageBusinessService<DomainModel, PrimaryKeyType>` interface
  
  - Service exception classes

## MZBase.EntityFrameworkCore
https://www.nuget.org/packages/MZBase.EntityFrameworkCore/

Contains EFCore implementations of MZBase.Infrastructure interfaces including

  - `RepositoryAsync<DBModelEntity,DomainModelEntity, PrimaryKeyType>`
  
  - `LDRCompatibleRepositoryAsync<DBModelEntity,DomainModelEntity, PrimaryKeyType>`
  
  - `UnitOfWorkAsync<T>`

## MZBase.Microservices
https://www.nuget.org/packages/MZBase.Microservices/

Some classes to faciliate microservice communications:

  - `ServiceMediatorBase<T>`: Wraps HttpClient and do Gets,Posts,Puts and Deletes with exception handling, serialization, logging and authentication. 

## MZBase.EntityFrameworkCore.Sql

https://www.nuget.org/packages/MZBase.EntityFrameworkCore.Sql/

Contains:

  -A helper to transfer description attributes of entity classes to extended properties (MS_Description) of related table/column of databse for any ef core DbContext. This changes could be used by any third party tool like Redgate SqlDoc to generate Database documentation. (Based on the code provided by Mohamood Dehghan here: https://stackoverflow.com/questions/10080601/how-to-add-description-to-columns-in-entity-framework-4-3-code-first-using-migra)
