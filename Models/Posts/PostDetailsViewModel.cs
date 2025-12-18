namespace BLOGAURA.Models.Posts
{
    public class PostDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string CreatedAtFormatted { get; set; } = string.Empty;

        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorPhotoUrl { get; set; } = string.Empty;

        public int LikeCount { get; set; }
        public List<string> ImagePaths { get; set; } = new();

        public List<CommentItem> Comments { get; set; } = new();

        public class CommentItem
        {
            public string AuthorId { get; set; } = string.Empty;
            public string AuthorName { get; set; } = string.Empty;
            public string AuthorPhotoUrl { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public string CreatedAtFormatted { get; set; } = string.Empty;
        }
    }
}
