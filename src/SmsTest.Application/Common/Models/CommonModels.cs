namespace SmsTest.Application.Common.Models
{
    public class Result<T>
    {
        public bool Success { get; private set; }
        public T? Data { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;

        protected Result(bool success, T? data, string errorMessage)
        {
            Success = success;
            Data = data;
            ErrorMessage = errorMessage;
        }

        public static Result<T> GetSuccess(T data) => new Result<T>(true, data, string.Empty);
        public static Result<T> GetFailure(string errorMessage) => new Result<T>(false, default, errorMessage);
    }

    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
