using System.Collections.Generic;

namespace BLOGAURA.Models.Posts
{
    public class PostCardViewModel
    {
        public int Id { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorPhotoUrl { get; set; } = string.Empty;
        public string CreatedAtFormatted { get; set; } = string.Empty;
        public bool CanEditOrDelete { get; set; }
        public bool CanDelete { get; set; }

        public Post Post { get; set; } = null!;
    }
}
