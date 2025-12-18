using System.ComponentModel.DataAnnotations;

namespace BLOGAURA.Models.Posts
{
    public class CreateCommentViewModel
    {
        public int PostId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;
    }
}
