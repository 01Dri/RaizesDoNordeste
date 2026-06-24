using System.Threading.Tasks;
using RestauranteUni.Domain.ValuesObjects;

namespace RestauranteUni.Application.Patterns.Dispatchers;

public interface IDispatcher<T, TK>
{
    Task<Result> HandleAsync(T parameter1, TK parameter2);
}