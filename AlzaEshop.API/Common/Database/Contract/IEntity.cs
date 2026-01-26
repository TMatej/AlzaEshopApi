namespace AlzaEshop.API.Common.Database.Contract;

/// <summary>
/// Interface defining base domain entity properties.
/// </summary>
public interface IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}
