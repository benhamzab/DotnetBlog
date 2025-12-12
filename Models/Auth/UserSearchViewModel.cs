namespace BLOGAURA.Models.Auth
{
    public class UserSummary
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? About { get; set; }
    }

    public class UserSearchViewModel
    {
        public string? Query { get; set; }
        public List<UserSummary> Results { get; set; } = new();
    }
}
