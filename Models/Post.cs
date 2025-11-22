namespace BLOGAURA.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";

        public required ICollection<PostTag> PostTags { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
