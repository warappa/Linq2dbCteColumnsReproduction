public class PartDataReferenceReadModel : ITemporalEntity
{
    public Guid Id { get; set; }

    public Guid ParentId { get; set; }

    public Guid ReferenceId { get; set; }

    public DataReferenceType Type { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }
}
