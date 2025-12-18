namespace BLOGAURA.Models.Reels
{
    public class ReelViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserPhotoUrl { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public string VideoPath { get; set; } = string.Empty;
        public string CreatedAtFormatted { get; set; } = string.Empty;
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public int? EventPostId { get; set; }
        public string EventTitle { get; set; } = string.Empty;
        public string? Category { get; set; }
    }
}
