using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        [Required]
        public string Content { get; set; }
        public Post Post { get; set; }
        public User User { get; set; }
    }
}
