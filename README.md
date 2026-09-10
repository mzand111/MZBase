
# MZBase
In data-driven applications, there are common requirements that need to be met in the code. One of the key requirements is to separate the domain logic from data storage and retrieval (abstraction of the data layer). To achieve this, we must define the classes of each layer to align with SOLID principles.

Moreover, patterns such as the repository pattern and Unit of Work (UoW) are frequently employed in these applications. The MZBase libraries offer a straightforward solution to these challenges. We have strived to keep it simple and applicable throughout the years, and this principle has been maintained even in version 2. Consequently, you should be able to use MZBase to implement anything from a simple data-driven application to a sophisticated multi-microservice system.

This library widely uses MZSimpleDynamicLinq (https://github.com/mzand111/MZSimpleDynamicLinq)


# Related nuget packages
## MZBase.Domain
https://www.nuget.org/packages/MZBase.Domain/

[![Nuget package](https://img.shields.io/nuget/vpre/MZBase.Domain)](https://www.nuget.org/packages/MZBase.Domain/)

A simple library for base classes of common usabilities needed in enterprise software. 
This library contains classes that are used as base for domain and entity classes.

## MZBase.Infrastructure
https://www.nuget.org/packages/MZBase.Infrastructure/

[![Nuget package](https://img.shields.io/nuget/vpre/MZBase.Infrastructure)](https://www.nuget.org/packages/MZBase.Infrastructure/)

Contains:

1- Common interfaces such as:

  - Interfaces to implement repository pattern in your code in an integrated manner: `IRepositoryAsync<ModelItem, DBModelEntity, T>`, `IPSFRepositoryAsync <ModelItem, DBModelEntity, T>` and `ILDRCompatibleRepositoryAsync<ModelItem, DBModelEntity, T>`
  
  - Interfaces to implement Unit of Work pattern and testing them easily: `IUnitOfWorkAsync` and `IDynamicTestableUnitOfWorkAsync`

2- Base classes to be used for service layers:

  - `BaseBusinessService` An abstract base class designed to be used as the root of every business service class . 

  - `BaseStorageBusinessService<Model, PrimaryKeyType>` Base class for storage business services, providing common functionality for adding, retrieving, modifying, and removing entities.
  
  - `IStorageBusinessService<DomainModel, PrimaryKeyType>` interface which is used to shape business services and their relation with the storages like databases.
  
  - Service exception classes

## MZBase.EntityFrameworkCore
https://www.nuget.org/packages/MZBase.EntityFrameworkCore/

[![Nuget package](https://img.shields.io/nuget/vpre/MZBase.EntityFrameworkCore)](https://www.nuget.org/packages/MZBase.EntityFrameworkCore/)

Contains EFCore implementations of MZBase.Infrastructure interfaces including

  - `RepositoryAsync<DBModelEntity,DomainModelEntity, PrimaryKeyType>`
  
  - `LDRCompatibleRepositoryAsync<DBModelEntity,DomainModelEntity, PrimaryKeyType>`
  
  - `UnitOfWorkAsync<T>`

## MZBase.Microservices
https://www.nuget.org/packages/MZBase.Microservices/

[![Nuget package](https://img.shields.io/nuget/vpre/MZBase.Microservices)](https://www.nuget.org/packages/MZBase.Microservices/)

Some classes to faciliate microservice communications:

  - `ServiceMediatorBase<T>`: Wraps HttpClient and do Gets,Posts,Puts and Deletes with exception handling, serialization, logging and authentication. 

### Usage rules for `Try*` methods
When using `TryGetAsync`, `TryPostAsync`, `TryPutAsync`, or `TryPostFormFileAsync`, keep the following in mind:

- These methods return an `HttpResponseMessage` alongside the deserialized result.
- The returned `HttpResponseMessage` must be disposed by the caller when it is no longer needed.
- If you inspect the response status, headers, or content, wrap it in `try/finally` and call `response.Dispose()` in the `finally` block.
- This is important for memory/resource management and avoids leaking response objects over time.

Example:

```csharp
var (response, result) = await mediator.TryPostAsync(payload, "/api/example");
try
{
    if (response.IsSuccessStatusCode)
    {
        // use result or response content
    }
}
finally
{
    response.Dispose();
}
```

## MZBase.EntityFrameworkCore.Sql

https://www.nuget.org/packages/MZBase.EntityFrameworkCore.Sql/

[![Nuget package](https://img.shields.io/nuget/vpre/MZBase.EntityFrameworkCore.Sql)](https://www.nuget.org/packages/MZBase.EntityFrameworkCore.Sql/)

Contains:

  -A helper to transfer description attributes of entity classes to extended properties (MS_Description) of related table/column of databse for any ef core DbContext. This changes could be used by any third party tool like Redgate SqlDoc to generate Database documentation. (Based on the code provided by Mohamood Dehghan here: https://stackoverflow.com/questions/10080601/how-to-add-description-to-columns-in-entity-framework-4-3-code-first-using-migra)


# Breaking Change in version 2
By getting feedbacks from version 1, the version 2 main goal is to minimize the need to add boiler-plate codes. At the first revisions this movement is mostly seen in BaseStorageBusinessService class. By using decedents of this class (Such as EFCoreStorageBusinessService), you are now able to implement a storage service business class with a few lines of code. 
## MZBase.Infrastructure
1-	`IRepositoryAsync`: Now acts as the root of repository interfaces in this package.
a.	Converted not-needed async methods to sync methods.
The operations inside these methods are usually not effected by any time-taking action that require to be async. The time-taking action usually takes place when the storage (usually a data context) is requested to save the changes

b.	The interface is now also aware of data layer entity. So the declaration is changed from 

```cs
public interface IRepositoryAsync<ModelItem, T>
```

To 

```cs
public interface IRepositoryAsync<ModelItem, DBModelEntity, T>
```
This change allows service layer classes to use data layer entities without type casting. 
c. Added non-async methods:
```cs
DBModelEntity? FirstOrDefault(Expression<Func<DBModelEntity, bool>> predicate);

DBModelEntity GetById(T id);

IReadOnlyList<ModelItem> AllItems();
```
2- `IRepositoryAsync`: With the introduction of `IBaseRepositoryAsync` in version 2.x, this interface should be used to implement the repository pattern in scenarios where the Unit of Work (UoW) pattern is unnecessary. Consequently, this interface now includes the method signatures for `SaveChangeAsync()` and `SaveChanges()`, which must be implemented by any inheriting class."
3-	Other interfaces inheriting `IRepositoryAsync`: `IPSFRepositoryAsync`, `ILDRCompatibleRepositoryAsync`:
Changes made to reflect changes to the base interface
4-	The class `BaseBusinessService`  has been added as a root for all business service classes
5-	A new base for storage service classes has been added: `BaseStorageBusinessService`
Decedents of this class such as `EFCoreStorageBusinessService` are now containing the most common boiler plate codes needed to implement a storage service class. The only methods needed to be implemented by the user are `ValidateOnAddAsync` and `ValidateOnModifyAsync`. By this change you will need to develope much less code than before. 


## MZBase.EntityFrameworkCore

1.	All the data layer entity object should now implement the `IConvertibleDBModelEntity<T>` interface. This is to ensure a centralized conversion from/to domain objects. This interface has two methods:
void `SetFieldsFromDomainModel(DomainModelEntity domainModelEntity)`;
 	Sets the fields of the database model entity from the domain model entity.
`DomainModelEntity GetDomainObject();`
Get the domain model entity instance from the database model entity.
This is a simple sample of domain and data layer classes implemented in this method:
```cs
public class UserProfileImage : Model<Guid>
{
    public Guid UserId { get; set; }
    public byte[]? Image { get; set; }
    public DateTime CreationTime { get; set; }
    public Guid CreatorUserId { get; set; }
}
```

```cs
public class UserProfileImageEntity : UserProfileImage, IConvertibleDBModelEntity<UserProfileImage>
{
    public UserProfileImage GetDomainObject() => this;

    public void SetFieldsFromDomainModel(UserProfileImage domainModelEntity)
    {
        this.ID = domainModelEntity.ID;
        this.UserId = domainModelEntity.UserId;
        this.CreatorUserId = domainModelEntity.CreatorUserId;
        this.CreationTime = domainModelEntity.CreationTime;
        this.Image = domainModelEntity.Image;
    }
}
```

2.	Methods that were not needed to be async were converted to sync version.
3.	Thanks to the change that all the db entities now implement `IConvertibleDBModelEntity`, insert method now works with any instance of a `DomainModelEntity`.
4.  To reflect changes made to `IRepositoryAsync` and `IBaseRepositoryAsync`, this package now contains both `BaseRepositoryAsync` and `RepositoryAsync`. The second could be used when you are not considering to use UoW pattern. 

## MZBase.Domain
1.  `IDto<DomainObject, PrimaryKey>` could be used to implement domain object related Dto classes.


# Special cases

## Defining domain entities with composite keys

Consider a mapping table that records which students are enrolled in which courses. Each enrollment is uniquely identified by the combination of `StudentId` and `CourseId`. Define a dedicated key struct for this combination and use it as the `Model<TPrimaryKey>` type argument.

1. Define the domain model with a typed key:

```cs
public class StudentCourseEnrollment : Model<StudentCourseEnrollmentKey>
{
    public virtual int StudentId { get; set; }
    public virtual int CourseId { get; set; }
    public DateTime EnrollmentDate { get; set; }
}
```

2. Define a key struct containing all key parts. Also define the `==` and `!=` operators:

```cs
public struct StudentCourseEnrollmentKey
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }

    public StudentCourseEnrollmentKey(int studentId, int courseId)
    {
        StudentId = studentId;
        CourseId = courseId;
    }

    public static bool operator ==(StudentCourseEnrollmentKey s1, StudentCourseEnrollmentKey s2)
        => s1.StudentId == s2.StudentId && s1.CourseId == s2.CourseId;

    public static bool operator !=(StudentCourseEnrollmentKey s1, StudentCourseEnrollmentKey s2)
        => s1.StudentId != s2.StudentId || s1.CourseId != s2.CourseId;
}
```

3. In DB entity class, map `ID` to/from key parts and keep `ID` as `[NotMapped]`.
   Also override direct key columns and mark them as `[DatabaseGenerated(DatabaseGeneratedOption.None)]`:

```cs
[NotMapped]
public override StudentCourseEnrollmentKey ID
{
    get => new StudentCourseEnrollmentKey(StudentId, CourseId);
    set
    {
        StudentId = value.StudentId;
        CourseId = value.CourseId;
    }
}

[DatabaseGenerated(DatabaseGeneratedOption.None)]
public override int StudentId { get; set; }

[DatabaseGenerated(DatabaseGeneratedOption.None)]
public override int CourseId { get; set; }
```

4. In `SetFieldsFromDomainModel`, set both direct key columns and `ID`:

```cs
public void SetFieldsFromDomainModel(StudentCourseEnrollment domainModelEntity)
{
    StudentId = domainModelEntity.StudentId;
    CourseId = domainModelEntity.CourseId;
    ID = domainModelEntity.ID;
    EnrollmentDate = domainModelEntity.EnrollmentDate;
}
```

5. Configure composite PK in `DbContext`:

```cs
modelBuilder.Entity<StudentCourseEnrollmentEntity>(p =>
{
    p.ToTable("StudentCourseEnrollments")
     .HasKey(c => new { c.StudentId, c.CourseId });
});
```

6. Use the same key type in repository and UoW:
   - `IBaseLDRCompatibleRepositoryAsync<StudentCourseEnrollment, StudentCourseEnrollmentEntity, StudentCourseEnrollmentKey>`
   - Add repository property in UoW interface/class and initialize it in the UoW constructor.

This keeps domain key semantics explicit while remaining compatible with EF Core and MZBase generic repository/UoW contracts.

## Defining entities with string IDs (Owner pattern)

When the business identifier is a string (like `Owner.OwnerId`), keep a typed key struct and wrap the string in that key.

1. Domain model pattern:

```cs
public class Owner : AuditableEx<OwnerKey>
{
    public string OwnerId { get; set; }
    public string Title { get; set; }
}

public struct OwnerKey
{
    public string OwnerId { get; set; }
    public OwnerKey(string ownerId) => OwnerId = ownerId;
}
```

2. DB entity pattern (`OwnerEntity`):

```cs
[NotMapped]
public override OwnerKey ID
{
    get => new OwnerKey(OwnerId);
    set => OwnerId = value.OwnerId;
}
```

3. `DbContext` pattern:

```cs
modelBuilder.Entity<OwnerEntity>(b =>
{
    b.Property(uu => uu.OwnerId)
     .HasMaxLength(11)
     .HasColumnName("ID");

    b.HasKey(uu => uu.OwnerId);
});
```

4. Repository and UoW pattern:
   - Repository generic args remain strongly typed: `BaseLDRCompatibleRepositoryAsync<Owner, OwnerEntity, OwnerKey>`
   - Override key-based retrieval using the wrapped string key (for example `id.OwnerId`).
   - Expose repository in UoW as `IOwnerRepository Owners { get; }`.

This pattern keeps public APIs strongly typed (`OwnerKey`) while storing and querying by string IDs in EF Core.


# Samples
1. [MZBaseSample](https://github.com/mzand111/MZBaseSample) : A sample company registration service, to show how to setup repository, Unit of Work (UoW), and service design patterns using MZBase. The solution provides an API to perform CRUD operations (Create, Read, Update, Delete) on a sample entity: Company. The Company model includes a collection of branches (CompanyBranch), which are managed and saved alongside their parent Company entity. 
