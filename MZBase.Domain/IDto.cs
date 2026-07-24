namespace MZBase.Domain
{
    public interface IDto<DomainObject, PrimaryKey>
        where DomainObject : Model<PrimaryKey>
        where PrimaryKey : struct
    {
        DomainObject GetDomainObject();
    }
}
