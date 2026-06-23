namespace RestauranteUni.Domain.UseCases;

public class Error
{
    public string Message { get; init; }
    public object? Details { get; init; }

    public Error()
    {
    }

    public Error(string message)
    {
        Message = message;
    }

    public Error(string message, object? details)
    {
        Message = message;
        Details = details;
    }
}