using RestauranteUni.Domain.ValuesObjects;

namespace RestauranteUni.Domain.UseCases;

    public interface IUseCaseHandler<in TParameter, TResponse>
        where TParameter : IUseCaseRequest
        where TResponse : IUseCaseResponse
    {
        Task<Result<TResponse>> HandleAsync(TParameter parameter, CancellationToken cancellation = default);
    }


    public interface IUseCaseHandler<TResponse>
        where TResponse : IUseCaseResponse
    {
        Task<Result<TResponse>> HandleAsync(CancellationToken cancellation = default);
    }

public interface IUseCaseRequest;
    public interface IUseCaseResponse;


