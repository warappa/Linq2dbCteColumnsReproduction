public class PartExternalIdReadModel
{
    public Guid Id { get; set; }

    public Guid PartId { get; set; }

    public Guid ScopedToId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }

    public string ExternalIdKind { get; set; }

    public string ExternalId { get; set; }
}
