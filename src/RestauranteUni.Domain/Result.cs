namespace RestauranteUni.Domain
{
    public class Result<T>
    {
        public T? Data { get; } 
        public bool IsSuccess { get; }
        public List<Validation> Validations { get; } = [];

        public Result(T? data, bool isSuccess)
        {
            Data = data;
            IsSuccess = isSuccess;
        }

        public Result(List<Validation> validations)
        {
            IsSuccess = false;
            Validations = validations;
        }

        public static Result<T> Success(T data) => new(data, true);
        public static Result<T> Failure(List<Validation> validations) => new(validations);

    }

    public class Result
    {
        public bool IsSuccess { get; }
        public List<Validation> Validations { get; } = [];

        public Result()
        {
            IsSuccess = true;
        }

        public Result(List<Validation> validations)
        {
            IsSuccess = false;
            Validations = validations;
        }

        public static Result Success() => new();
        public static Result Failure(List<Validation> validations) => new(validations);

    }

    public class Validation
    {
        public Validation(string property, List<string> errorMessage)
        {
            Property = property;
            ErrorsMessage = errorMessage;
        }
        public string Property { get; set; }
        public List<string> ErrorsMessage { get; set; }

    }
}
