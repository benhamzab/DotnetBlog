using System;
using System.ComponentModel.DataAnnotations;

namespace BLOGAURA.Models.Content
{
    public class ContentCalendarItem
    {
        public int Id { get; set; }

        public int? EventId { get; set; }
        public int? PostId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime PlannedPublishDate { get; set; }

        [StringLength(200)]
        public string TargetAudience { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Planned";

        [StringLength(1000)]
        public string? Notes { get; set; }

        public int EditorUserId { get; set; }
    }
}
