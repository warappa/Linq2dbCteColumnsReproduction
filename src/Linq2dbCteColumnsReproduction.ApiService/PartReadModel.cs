public class PartReadModel : BaseReadModel<Guid>, ITemporalEntity, IEntityWithDateTimeInfo
{
    public Guid VariantId { get; set; }

    public string Name { get; set; }

    public Guid SchemaId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }

    public virtual ICollection<PartExternalIdReadModel> ExternalIds { get; set; } = [];

    public virtual ICollection<PartDataReferenceReadModel> DataReferences { get; set; } = [];
}
