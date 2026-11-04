namespace practise.Models
{
    // Supplies the error page with a request identifier for locating the corresponding diagnostics.
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
