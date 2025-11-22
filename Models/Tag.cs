namespace BLOGAURA.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public required ICollection<PostTag> PostTags { get; set; }

    }
}
