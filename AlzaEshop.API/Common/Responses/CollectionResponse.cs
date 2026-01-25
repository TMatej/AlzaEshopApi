namespace AlzaEshop.API.Common.Responses;

public record CollectionResponse<TEntity>
{
    public IList<TEntity> Items { get; set; } = new List<TEntity>();
    public int Count => Items.Count;
}
