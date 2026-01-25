namespace AlzaEshop.API.Common.Database.Contract;

/// <summary>
/// Database abstraction.
/// </summary>
public interface IDatabaseContext
{
    /// <summary>
    /// Products repository.
    /// </summary>
    IRepository<Product> Products { get; set; }
}
