namespace RestauranteUni.Domain.UseCases
{
    public interface IUseCaseResponse<TId> : IUseCaseResponse
    {
        public TId Id { get; set; }
    }
}
