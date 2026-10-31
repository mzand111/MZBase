# MZBase

In data-driven applications, there are common requirements that need to be met in the code. One of the key requirements is to separate the domain logic from data storage and retrieval (abstraction of the data layer). To achieve this, we must define the classes of each layer to align with SOLID principles.

Moreover, patterns such as the repository pattern and Unit of Work (UoW) are frequently employed in these applications. The MZBase libraries offer a straightforward solution to these challenges. We have strived to keep it simple and applicable throughout the years, and this principle has been maintained even in version 2. Consequently, you should be able to use MZBase to implement anything from a simple data-driven application to a sophisticated multi-microservice system.

This library widely uses MZSimpleDynamicLinq (https://github.com/mzand111/MZSimpleDynamicLinq)

# Related nuget packages

## MZBase.Domain

https://www.nuget.org/packages/MZBase.Domain/

[![Nuget package](https://img.shields.io/nuget/vpre/MZBase.Domain)](https://www.nuget.org/packages/MZBase.Domain/)

A simple library for base classes of common usabilities needed in enterprise software. This library contains classes that are used as base for domain and entity classes.

## MZBase.Infrastructure

https://www.nuget.org/packages/MZBase.Infrastructure/

[![Nuget package](https://img.shields.io/nuget/vpre/MZBase.Infrastructure)](https://www.nuget.org/packages/MZBase.Infrastructure/)

Contains:

1- Common interfaces such as:

-   Interfaces to implement repository pattern in your code in an integrated manner: `IRepositoryAsync<ModelItem, DBModelEntity, T>`, `IPSFRepositoryAsync <ModelItem, DBModelEntity, T>` and `ILDRCompatibleRepositoryAsync<ModelItem, DBModelEntity, T>`

-   Interfaces to implement Unit of Work pattern and testing them easily: `IUnitOfWorkAsync` and `IDynamicTestableUnitOfWorkAsync`

2- Base classes to be used for service layers:

-   `BaseBusinessService` An abstract base class designed to be used as the root of every business service class .
-   `BaseStorageBusinessService<Model, PrimaryKeyType>` Base class for storage business services, providing common functionality for adding, retrieving, modifying, and removing entities.
-   `IStorageBusinessService<DomainModel, PrimaryKeyType>` interface which is used to shape business services and their relation with the storages like databases.
-   Service exception classes (e.g. `ServiceObjectNotFoundException` in `MZBase.Infrastructure.Service.Exceptions`)

## MZBase.EntityFrameworkCore

https://www.nuget.org/packages/MZBase.EntityFrameworkCore/

[![Nuget package](https://img.shields.io/nuget/vpre/MZBase.EntityFrameworkCore)](https://www.nuget.org/packages/MZBase.EntityFrameworkCore/)

Contains EFCore implementations of MZBase.Infrastructure interfaces including

-   `RepositoryAsync<DBModelEntity,DomainModelEntity, PrimaryKeyType>`

-   `LDRCompatibleRepositoryAsync<DBModelEntity,DomainModelEntity, PrimaryKeyType>`

-   `UnitOfWorkAsync<T>`

-   `EFCoreStorageBusinessService<TDomain, TEntity, TKey, TUnitOfWork, TDataContext>`: a ready-to-use storage business service for EF Core.

## MZBase.Microservices

https://www.nuget.org/packages/MZBase.Microservices/

[![Nuget package](https://img.shields.io/nuget/vpre/MZBase.Microservices)](https://www.nuget.org/packages/MZBase.Microservices/)

Some classes to faciliate microservice communications:

-   `ServiceMediatorBase<T>`: Wraps HttpClient and do Gets,Posts,Puts and Deletes with exception handling, serialization, logging and authentication.

### Usage rules for `Try*` methods

When using `TryGetAsync`, `TryPostAsync`, `TryPutAsync`, or `TryPostFormFileAsync`, keep the following in mind:

-   These methods return an `HttpResponseMessage` alongside the deserialized result.
-   The returned `HttpResponseMessage` must be disposed by the caller when it is no longer needed.
-   If you inspect the response status, headers, or content, wrap it in `try/finally` and call `response.Dispose()` in the `finally` block.
-   This is important for memory/resource management and avoids leaking response objects over time.

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

\-A helper to transfer description attributes of entity classes to extended properties (MS_Description) of related table/column of databse for any ef core DbContext. This changes could be used by any third party tool like Redgate SqlDoc to generate Database documentation. (Based on the code provided by Mohamood Dehghan here: https://stackoverflow.com/questions/10080601/how-to-add-description-to-columns-in-entity-framework-4-3-code-first-using-migra)

# Breaking Change in version 2

By getting feedbacks from version 1, the version 2 main goal is to minimize the need to add boiler-plate codes. At the first revisions this movement is mostly seen in BaseStorageBusinessService class. By using decedents of this class (Such as EFCoreStorageBusinessService), you are now able to implement a storage service business class with a few lines of code.

## MZBase.Infrastructure

1- `IRepositoryAsync`: Now acts as the root of repository interfaces in this package. a. Converted not-needed async methods to sync methods. The operations inside these methods are usually not effected by any time-taking action that require to be async. The time-taking action usually takes place when the storage (usually a data context) is requested to save the changes

b. The interface is now also aware of data layer entity. So the declaration is changed from

public interface IRepositoryAsync<ModelItem, T\>

To

public interface IRepositoryAsync<ModelItem, DBModelEntity, T\>

This change allows service layer classes to use data layer entities without type casting. c. Added non-async methods:

DBModelEntity? FirstOrDefault(Expression<Func<DBModelEntity, bool\>\> predicate);

DBModelEntity GetById(T id);

IReadOnlyList<ModelItem\> AllItems();

2- `IRepositoryAsync`: With the introduction of `IBaseRepositoryAsync` in version 2.x, this interface should be used to implement the repository pattern in scenarios where the Unit of Work (UoW) pattern is unnecessary. Consequently, this interface now includes the method signatures for `SaveChangeAsync()` and `SaveChanges()`, which must be implemented by any inheriting class." 3- Other interfaces inheriting `IRepositoryAsync`: `IPSFRepositoryAsync`, `ILDRCompatibleRepositoryAsync`: Changes made to reflect changes to the base interface 4- The class `BaseBusinessService` has been added as a root for all business service classes 5- A new base for storage service classes has been added: `BaseStorageBusinessService` Decedents of this class such as `EFCoreStorageBusinessService` are now containing the most common boiler plate codes needed to implement a storage service class. The only methods needed to be implemented by the user are `ValidateOnAddAsync` and `ValidateOnModifyAsync`. By this change you will need to develope much less code than before.

## MZBase.EntityFrameworkCore

1.  All the data layer entity object should now implement the `IConvertibleDBModelEntity<T>` interface. This is to ensure a centralized conversion from/to domain objects. This interface has two methods: void `SetFieldsFromDomainModel(DomainModelEntity domainModelEntity)`; Sets the fields of the database model entity from the domain model entity. `DomainModelEntity GetDomainObject();` Get the domain model entity instance from the database model entity. This is a simple sample of domain and data layer classes implemented in this method:

```csharp
public class UserProfileImage : Model<Guid>
{
    public Guid UserId { get; set; }
    public byte[]? Image { get; set; }
    public DateTime CreationTime { get; set; }
    public Guid CreatorUserId { get; set; }
}

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

2.  Methods that were not needed to be async were converted to sync version. 3. Thanks to the change that all the db entities now implement `IConvertibleDBModelEntity`, insert method now works with any instance of a `DomainModelEntity`. 4. To reflect changes made to `IRepositoryAsync` and `IBaseRepositoryAsync`, this package now contains both `BaseRepositoryAsync` and `RepositoryAsync`. The second could be used when you are not considering to use UoW pattern.

## MZBase.Domain

1.  `IDto<DomainObject, PrimaryKey>` could be used to implement domain object related Dto classes.

# Important usage notes and pitfalls

These notes are collected from real production usage. They describe behaviors that are easy to get wrong because problems surface at runtime (or at first use), not at compile time.

## `IDateTimeProviderService` must be implemented by the application

MZBase ships the interface (single member: `DateTime GetNow()`) but **no implementation**. Every storage service constructor requires it, so if it is not registered in the DI container, resolving **any** storage service throws `InvalidOperationException` at first resolve — even though DI registration itself succeeds silently at startup.

```csharp
public class DateTimeProviderService : IDateTimeProviderService
{
    public DateTime GetNow() => DateTime.Now;
}

// Registration
services.AddSingleton<IDateTimeProviderService, DateTimeProviderService>();
```

## Register the concrete Unit of Work, not only its interface

Storage service constructors take the **concrete** unit of work type, because the base-class generic constraint is `TUnitOfWork : UnitOfWorkAsync<TDataContext>`; an interface does not satisfy it. Register the concrete type and forward the interface to it so both resolve to the same instance per scope:

```csharp
services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
services.AddScoped<AppUnitOfWork>();
services.AddScoped<IAppUnitOfWork>(sp => sp.GetRequiredService<AppUnitOfWork>());
services.AddScoped<StudentCourseEnrollmentStorageService>();
```

An interface-only registration produces: *"Unable to resolve service for type 'AppUnitOfWork' while attempting to activate ..."* the first time a storage service is resolved.

## Every model needs a `GetRepo` case in the unit of work

`EFCoreStorageBusinessService` resolves its repository through `IDynamicTestableUnitOfWorkAsync.GetRepo<TModel, TDBModel, TKey>()`. Add a `typeof(TModel) == typeof(<Model>)` case returning the typed repository for **every** persisted model. A missing case does not fail at startup — it fails at the first service call for that model.

## Validation hooks

`EFCoreStorageBusinessService` descendants implement validation as **`protected override`** (not `public override`) methods that return `Task<bool>`, where `true` means validation **passed**:

```csharp
protected override Task<bool> ValidateOnAddAsync(TDomain item);
protected override Task<bool> ValidateOnModifyAsync(TDomain item, TDomain oldItem);
```

`ValidateOnModifyAsync` receives both the new and the old item, so rules that depend on unchanged keys or field deltas can be implemented.

## `AddAsync` returns the key but does not set it on the passed object

`AddAsync` returns the generated key of type `TKey`; it does **not** assign it to `item.ID`. Assign it yourself when the caller keeps using the instance (for example a grid row):

```csharp
item.ID = await service.AddAsync(item);
```

## `ModifyAsync` looks the row up by `item.ID`

`ModifyAsync` fetches the currently persisted entity via `GetByIdAsync(item.ID)` and throws `ServiceObjectNotFoundException` ("... Not Found") when nothing matches. Always make sure `item.ID` carries the real key before calling `ModifyAsync` — this is especially important for composite keys (see below).

## Loading all items

Full-list reads go through `ItemsAsync`; set an explicit `Take`, because the default value does not mean "all":

```csharp
var result = await service.ItemsAsync(new LinqDataRequest { Take = int.MaxValue });
foreach (var item in result.Data) { /* ... */ }
```

`RecordsTotal` is the unfiltered count, `RecordsFiltered` the post-filter count. The `LinqDataRequest`/`LinqDataResult<T>` types come from MZSimpleDynamicLinq.Core, which arrives transitively through the MZBase packages; add an explicit `PackageReference` when application code uses these types directly.

## Saving a dependent row with unknown foreign key values

If a dependent entity is saved while one of its foreign-key values is still the CLR default (0) and the principal entity is not tracked, EF Core throws: *"The value of '<Entity>.<Property>' is unknown when attempting to save changes. This is because the property is also part of a foreign key for which the principal entity in the relationship is not known."* Always set the FK values explicitly before saving (and see the composite-key conversion rule below, where a naive `SetFieldsFromDomainModel` can silently zero them out).

## SQL Server allows only one cascade path per table

A mapping table with **two** foreign keys to the **same** parent (a very common shape: `FromSiteId`/`ToSiteId`, `SenderId`/`ReceiverId`, ...) cannot have `ON DELETE CASCADE` on both — SQL Server rejects the migration/DDL with error 1785: *"may cause cycles or multiple cascade paths"*. Configure exactly one cascading FK and use `DeleteBehavior.Restrict` (NO ACTION) for the other(s), or delete dependent rows explicitly in application code before removing the principal.

# Special cases

## Defining domain entities with composite keys

Consider a mapping table that records which students are enrolled in which courses. Each enrollment is uniquely identified by the combination of `StudentId` and `CourseId`. Define a dedicated key struct for this combination and use it as the `Model<TPrimaryKey>` type argument.

1.  Define the domain model with a typed key:

```csharp
public class StudentCourseEnrollment : Model<StudentCourseEnrollmentKey>
{
    public virtual int StudentId { get; set; }
    public virtual int CourseId { get; set; }
    public DateTime EnrollmentDate { get; set; }
}
```

2.  Define a key struct containing all key parts. Also define the `==` and `!=` operators (and `Equals`/`GetHashCode`, so the struct behaves correctly in collections and LINQ):

```csharp
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

    public override bool Equals(object? obj)
        => obj is StudentCourseEnrollmentKey other && this == other;

    public override int GetHashCode() => HashCode.Combine(StudentId, CourseId);
}
```

3.  In the DB entity class, map `ID` to/from the key parts and keep `ID` as `[NotMapped]`. Also override the direct key columns and mark them as `[DatabaseGenerated(DatabaseGeneratedOption.None)]` (this is why the domain key properties must be `virtual`):

```csharp
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

4.  In `SetFieldsFromDomainModel`, copy the key parts and then **derive `ID` from the key parts you just copied — do not copy `domainModelEntity.ID`**:

```csharp
public void SetFieldsFromDomainModel(StudentCourseEnrollment domainModelEntity)
{
    StudentId = domainModelEntity.StudentId;
    CourseId = domainModelEntity.CourseId;

    // Derive ID from the key parts that were just copied.
    // In the normal Add flow the caller sets only the key parts and
    // domainModelEntity.ID is still the default (0, 0) struct — copying it
    // here would overwrite the values above with zeros, and EF Core would
    // then fail to save with:
    // "The value of '<Entity>.<KeyPart>' is unknown when attempting to save changes.
    //  This is because the property is also part of a foreign key for which the
    //  principal entity in the relationship is not known."
    ID = new StudentCourseEnrollmentKey(StudentId, CourseId);

    EnrollmentDate = domainModelEntity.EnrollmentDate;
}
```

`GetDomainObject()` should symmetrically rebuild `ID` from the key parts, so a domain object coming back from storage always carries its full key.

5.  Configure the composite PK in `DbContext` (and the foreign keys):

```csharp
modelBuilder.Entity<StudentCourseEnrollmentEntity>(p =>
{
    p.ToTable("StudentCourseEnrollments")
     .HasKey(c => new { c.StudentId, c.CourseId });

    p.HasOne<StudentEntity>()
     .WithMany()
     .HasForeignKey(c => c.StudentId)
     .OnDelete(DeleteBehavior.Cascade);

    p.HasOne<CourseEntity>()
     .WithMany()
     .HasForeignKey(c => c.CourseId)
     .OnDelete(DeleteBehavior.Restrict);
});
```

Remember the SQL Server cascade-path rule above when a mapping table references the same principal more than once.

6.  Use the same key type in the repository — and **override `GetByIdAsync`**, because the base implementation passes the whole key as a single boxed value to `DbSet.FindAsync(object[])`, which throws for composite primary keys:

```csharp
public class StudentCourseEnrollmentRepository
    : BaseLDRCompatibleRepositoryAsync<StudentCourseEnrollment, StudentCourseEnrollmentEntity, StudentCourseEnrollmentKey>,
      IStudentCourseEnrollmentRepository
{
    private readonly AppDbContext _context;

    public StudentCourseEnrollmentRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<StudentCourseEnrollmentEntity?> GetByIdAsync(StudentCourseEnrollmentKey id)
        => await _context.StudentCourseEnrollments
            .FirstOrDefaultAsync(e => e.StudentId == id.StudentId && e.CourseId == id.CourseId);
}
```

The synchronous `GetById(TKey)` is **not virtual** — do not try to `override` it (compiler error CS0506). The EF Core storage services only use the async path, so overriding `GetByIdAsync` is sufficient.

7.  Add the repository property in the UoW interface/class, initialize it in the UoW constructor, and add a `typeof(TModel) == typeof(StudentCourseEnrollment)` case in `GetRepo<TModel, TDBModel, PrimKey>()` (the key struct satisfies the `PrimKey : struct` constraint).

8.  In the storage service (`EFCoreStorageBusinessService<TDomain, TEntity, TKey, TUnitOfWork, TDataContext>`), implement the two validation hooks, and **override `RemoveByIdAsync`**: the base implementation builds the predicate `x => x.ID.Equals((object)id)` via expression trees, which EF Core cannot translate because `ID` is `[NotMapped]` ("could not be translated ... member is unmapped"). Mirror the base flow (lookup → not-found → delete → commit → log) using the key-part repository lookup:

```csharp
public class StudentCourseEnrollmentStorageService
    : EFCoreStorageBusinessService<StudentCourseEnrollment, StudentCourseEnrollmentEntity,
        StudentCourseEnrollmentKey, AppUnitOfWork, AppDbContext>
{
    public StudentCourseEnrollmentStorageService(
        AppUnitOfWork unitOfWork,
        IDateTimeProviderService dateTimeProvider,
        ILogger<StudentCourseEnrollment> logger)
        : base(unitOfWork, dateTimeProvider, logger)
    {
    }

    protected override Task<bool> ValidateOnAddAsync(StudentCourseEnrollment item)
    {
        // return true when valid
    }

    protected override Task<bool> ValidateOnModifyAsync(StudentCourseEnrollment item, StudentCourseEnrollment oldItem)
    {
        // return true when valid
    }

    public override async Task RemoveByIdAsync(StudentCourseEnrollmentKey ID)
    {
        var entity = await _unitOfWork.StudentCourseEnrollments.GetByIdAsync(ID);

        if (entity == null)
        {
            var ex = new ServiceObjectNotFoundException($"{nameof(StudentCourseEnrollment)} not found");
            LogRemove(ID, "Item With This Id Not Found", ex);
            throw ex;
        }

        _unitOfWork.StudentCourseEnrollments.Delete(entity);
        await _unitOfWork.CommitAsync();
        LogRemove(ID, "Item Deleted Successfully", null);
    }
}
```

(`_unitOfWork` and `LogRemove` are protected members of the base classes; `ServiceObjectNotFoundException` lives in `MZBase.Infrastructure.Service.Exceptions`.)

9.  Caller contract for composite-key services:

```csharp
var item = new StudentCourseEnrollment { StudentId = 1, CourseId = 2, EnrollmentDate = DateTime.Now };

var key = await service.AddAsync(item);   // returns the key; does NOT set item.ID
item.ID = key;                            // set it before modifying/removing

item.EnrollmentDate = DateTime.Now.AddDays(1);
await service.ModifyAsync(item);          // looks the row up by item.ID

await service.RemoveByIdAsync(key);       // takes the full key struct
```

`AddAsync` itself works with composite keys without any special handling: the base service converts the domain model to the entity (the key parts flow through `SetFieldsFromDomainModel`), commits, and returns the entity's `ID` — which for `DatabaseGenerated(None)` keys is exactly the client-supplied key struct.

This keeps domain key semantics explicit while remaining compatible with EF Core and MZBase generic repository/UoW contracts.

## Defining entities with string IDs (Owner pattern)

When the business identifier is a string (like `Owner.OwnerId`), keep a typed key struct and wrap the string in that key.

1.  Domain model pattern:

```csharp
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

2.  DB entity pattern (`OwnerEntity`):

```csharp
[NotMapped]
public override OwnerKey ID
{
    get => new OwnerKey(OwnerId);
    set => OwnerId = value.OwnerId;
}
```

3.  `DbContext` pattern:

```csharp
modelBuilder.Entity<OwnerEntity>(b =>
{
    b.Property(uu => uu.OwnerId)
     .HasMaxLength(11)
     .HasColumnName("ID");

    b.HasKey(uu => uu.OwnerId);
});
```

4.  Repository and UoW pattern:

-   Repository generic args remain strongly typed: `BaseLDRCompatibleRepositoryAsync<Owner, OwnerEntity, OwnerKey>`
-   Override key-based retrieval using the wrapped string key (for example `id.OwnerId`).
-   Expose repository in UoW as `IOwnerRepository Owners { get; }`.

This pattern keeps public APIs strongly typed (`OwnerKey`) while storing and querying by string IDs in EF Core.

# `EntityHelper.SyncDomainAndEntityModelSubCollections` (recently fixed)

Versions up to and including **2.0.11** contained a defect in `EntityHelper.SyncDomainAndEntityModelSubCollections`: the helper matched child entities by `ID`, and every newly created child entity carries `ID == 0` (identity key, not yet saved). As a result, when an aggregate was saved with more than one new child, the second and all subsequent new children matched the first created entity and only overwrote its fields — silently collapsing the whole submitted collection into a single database row.

The defect is fixed in the latest version of `EntityHelper.cs`. If you are pinned to version 2.0.11 or older, do **not** use the helper for aggregates with identity-keyed children and multiple new items; reconcile the child collection explicitly inside the root entity's `SetFieldsFromDomainModel` instead:

-   replace previously added unsaved children,
-   remove tracked children that are missing from the submitted collection,
-   update tracked children by their real ID,
-   create a new entity for every submitted child with `ID == 0`.

# Samples

1.  [MZBaseSample](https://github.com/mzand111/MZBaseSample) : A sample company registration service, to show how to setup repository, Unit of Work (UoW), and service design patterns using MZBase. The solution provides an API to perform CRUD operations (Create, Read, Update, Delete) on a sample entity: Company. The Company model includes a collection of branches (CompanyBranch), which are managed and saved alongside their parent Company entity.
