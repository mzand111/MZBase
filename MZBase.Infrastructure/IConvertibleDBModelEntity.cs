namespace MZBase.Infrastructure
{
    public interface IConvertibleDBModelEntity<DomainModelEntity> where DomainModelEntity : class
    {
        /// <summary>
        /// Set the fields of the database model entity from the domain model entity.
        /// </summary>
        /// <param name="domainModelEntity"></param>
        void SetFieldsFromDomainModel(DomainModelEntity domainModelEntity);
        /// <summary>
        /// Get the domain model entity instance from the database model entity.
        /// </summary>
        /// <returns></returns>
        DomainModelEntity GetDomainObject();
    }
}
