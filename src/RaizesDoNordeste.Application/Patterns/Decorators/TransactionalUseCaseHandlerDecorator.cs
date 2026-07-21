using RaizesDoNordeste.Data;
using RaizesDoNordeste.Domain.UseCases;
using RaizesDoNordeste.Domain.ValuesObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaizesDoNordeste.Application.Patterns.Decorators
{
    public class TransactionalUseCaseHandlerDecorator<TRequest, TResponse> : IUseCaseHandler<TRequest, TResponse>
        where TRequest : IUseCaseRequest
        where TResponse : IUseCaseResponse
    {
        private readonly IUseCaseHandler<TRequest, TResponse> _inner;
        private readonly ApplicationDbContext _dbContext;

        public TransactionalUseCaseHandlerDecorator(IUseCaseHandler<TRequest, TResponse> inner, ApplicationDbContext dbContext)
        {
            _inner = inner;
            _dbContext = dbContext;
        }

        public async Task<Result<TResponse>> HandleAsync(TRequest parameter, CancellationToken cancellation = default)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellation);
            try
            {
                var result = await _inner.HandleAsync(parameter, cancellation);
                
                if (result.IsSuccess)
                {
                    await transaction.CommitAsync(cancellation);
                }
                else
                {
                    await transaction.RollbackAsync(cancellation);
                }
                
                return result;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellation);
                throw;
            }
        }
    }

    public class TransactionalUseCaseHandlerDecorator<TResponse> : IUseCaseHandler<TResponse>
        where TResponse : IUseCaseResponse
    {
        private readonly IUseCaseHandler<TResponse> _inner;
        private readonly ApplicationDbContext _dbContext;

        public TransactionalUseCaseHandlerDecorator(IUseCaseHandler<TResponse> inner, ApplicationDbContext dbContext)
        {
            _inner = inner;
            _dbContext = dbContext;
        }

        public async Task<Result<TResponse>> HandleAsync(CancellationToken cancellation = default)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellation);
            try
            {
                var result = await _inner.HandleAsync(cancellation);
                
                if (result.IsSuccess)
                {
                    await transaction.CommitAsync(cancellation);
                }
                else
                {
                    await transaction.RollbackAsync(cancellation);
                }
                
                return result;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellation);
                throw;
            }
        }
    }
}
