using System.Collections;

namespace Backend.Models
{
    public class User
    {
        public int Id { get; set; }
        public byte[]? ProfilePicture { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string City { get; set; }
        public string FavPlace { get; set; }
        public string FavFood { get; set; }

        public ICollection<Follow> Follow { get; set; } = new List<Follow>();
        public ICollection<Follow> Followed { get; set; } = new List<Follow>();
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();

        /*
                public ICollection<Following> Follow { get; set; }
                public ICollection<Following> Followed { get; set; }
                public ICollection<Post> Posts { get; set; } = new List<Post>();
                public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
                public ICollection<PostComments> Comments { get; set; } = new List<PostComments>();*/
    }
}
