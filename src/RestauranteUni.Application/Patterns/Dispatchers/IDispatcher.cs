using RestauranteUni.Domain.ValuesObjects;

namespace RestauranteUni.Application.Patterns.Dispatchers;

public interface IDispatcher<T, TK>
{
    Result Handle(T parameter1, TK parameter2);
}