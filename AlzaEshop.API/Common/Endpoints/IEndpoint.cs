namespace AlzaEshop.API.Common.Endpoints
{
    /// <summary>
    /// Interface for marking endpoints and defining common method for their URI registration.
    /// </summary>
    public interface IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app);
    }
}
