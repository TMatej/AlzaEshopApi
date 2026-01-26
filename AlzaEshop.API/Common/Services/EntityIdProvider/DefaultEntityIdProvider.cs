
namespace AlzaEshop.API.Common.Services.EntityIdProvider;

public class DefaultEntityIdProvider : IEntityIdProvider
{
    public Guid CreateNewId()
    {
        return Guid.NewGuid();
    }
}
