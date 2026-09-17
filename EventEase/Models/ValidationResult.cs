namespace EventEase.Models
{
    /// <summary>
    /// Represents the result of a validation operation.
    /// Used to standardize error responses across the application.
    /// </summary>
    public class ValidationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();

        public static ValidationResult Success(string message = "Operation completed successfully")
        {
            return new ValidationResult
            {
                IsSuccess = true,
                Message = message,
                Errors = new List<string>()
            };
        }

        public static ValidationResult Failure(string message, params string[] errors)
        {
            return new ValidationResult
            {
                IsSuccess = false,
                Message = message,
                Errors = errors.ToList()
            };
        }
    }
}
