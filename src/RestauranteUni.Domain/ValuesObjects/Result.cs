using System.Net;

namespace RestauranteUni.Domain.ValuesObjects
{
    public class Result
    {
        protected Result(
            bool isSuccess,
            IReadOnlyCollection<Validation>? validations = null,
            HttpStatusCode? statusCode = HttpStatusCode.OK)
        {
            IsSuccess = isSuccess;
            Validations = validations ?? [];
            StatusCode = statusCode;
        }

        protected Result(
            bool isSuccess,
            HttpStatusCode? statusCode)
        {
            IsSuccess = isSuccess;
            StatusCode = statusCode;
        }

        public bool IsSuccess { get; }

        public IReadOnlyCollection<Validation> Validations { get; }

        public HttpStatusCode? StatusCode { get; }

        public static Result Success()
            => new(true);

        public static Result Success(HttpStatusCode status)
            => new(true, status);

        public static Result Failure(
            IEnumerable<Validation> validations,
            HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            => new(false, validations.ToList(), statusCode);

        public ErrorResponse ToErrorResponse(string title)
            => new(
                title,
                (int)(StatusCode ?? HttpStatusCode.BadRequest),
                Validations);
    }

    public sealed class Result<T> : Result
    {
        private Result(
            T? data,
            bool isSuccess,
            IReadOnlyCollection<Validation>? validations = null,
            HttpStatusCode? statusCode = HttpStatusCode.OK)
            : base(isSuccess, validations, statusCode)
        {
            Data = data;
        }

        private Result(
            T? data,
            bool isSuccess,
            HttpStatusCode status)
            : base(isSuccess, status)
        {
            Data = data;
        }



        public T? Data { get; }

        public static Result<T> Success(T data)
            => new(data, true);

        public static Result<T> Success(T data, HttpStatusCode status)
            => new(data, true, status);


        public static Result<T> Failure(
            IEnumerable<Validation> validations,
            HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            => new(default, false, validations.ToList(), statusCode);

        public static Result<T> FailureNotFound(string errorMessage)
            => new(default, false, [new Validation(errorMessage)], HttpStatusCode.NotFound);
    }

    public sealed record Validation(
        string Property,
        IReadOnlyCollection<string> Errors)
    {
        public Validation(string property, string error)
            : this(property, [error])
        {
        }

        public Validation(string error)
            : this(string.Empty, [error])
        {
        }
    }

    public sealed class ErrorResponse
    {
        public ErrorResponse(
            string title,
            int status,
            IEnumerable<Validation> errors)
        {
            Title = title;
            Status = status;
            Errors = errors.ToList().AsReadOnly();
            Timestamp = DateTimeOffset.UtcNow;
        }

        public string Title { get; }

        public int Status { get; }

        public DateTimeOffset Timestamp { get; }

        public IReadOnlyCollection<Validation> Errors { get; }
    }
}