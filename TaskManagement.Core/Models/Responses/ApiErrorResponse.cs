namespace TaskManagement.Core.Models.Responses
{
    public class ApiErrorResponse
    {
        public int Status { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public string StackTrace { get; set; }
        public string InnerException { get; set; }
    }
}
