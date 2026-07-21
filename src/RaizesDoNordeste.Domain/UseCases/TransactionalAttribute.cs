using System;

namespace RaizesDoNordeste.Domain.UseCases
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TransactionalAttribute : Attribute
    {
    }
}
