namespace BLOGAURA.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Content { get; set; } = default!;

        public DateTime CreatedAt { get; set; }
    }
}
