namespace AlzaEshop.API.Common.Services.EntityIdProvider;

/// <summary>
/// Entity Id provider interface. 
/// </summary>
public interface IEntityIdProvider
{
    Guid CreateNewId();
}
