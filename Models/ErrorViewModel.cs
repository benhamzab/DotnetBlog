using System;

namespace BLOGAURA.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        // True if RequestId is not null or empty
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
