namespace DevResourceAPI.Services;

public class ServiceResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; } = string.Empty;

    public static ServiceResult<T> Ok(T data, string message = "")
    {
        return new ServiceResult<T> 
        { 
            Success = true, 
            Data = data, 
            Message = message 
        };
    }

    public static ServiceResult<T> Fail(string message)
    {
        return new ServiceResult<T> 
        {
            Success = false, 
            Data = default,
            Message = message };
    }
}
    public class ServiceResult
    {
        public bool Success {get; set;}
        public string Message {get; set;} = string.Empty;
        public static ServiceResult Ok (string message = "")
        {
            return new ServiceResult 
            {
                Success =true, 
                Message=message
                };
        }
        public static ServiceResult Fail(string message)
        {
            return new ServiceResult 
            {
                Success =false, 
                Message=message
            };
        }
    }  
