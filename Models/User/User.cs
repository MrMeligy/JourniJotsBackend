using Backend.Models;
using System.Collections;
using System.ComponentModel.DataAnnotations;
public class User
{
    [Required]
    public int Id { get; set; }
    public byte[]? ProfilePicture { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    [MinLength(8)]
    public string Password { get; set; }
    [Required]
    [MaxLength(30)]
    public string FirstName { get; set; }
    [Required]
    [MaxLength(30)]
    public string LastName { get; set; }
    [Required]
    [MaxLength(30)]
    public string UserName { get; set; }
    [Required]
    public DateOnly DateOfBirth { get; set; }
    [Required]
    public string City { get; set; }
    public string? FavFood { get; set; }


    public ICollection<Interests> Interests { get; set; } = new List<Interests>();
    public ICollection<Follow> Follow { get; set; } = new List<Follow>();
    public ICollection<Follow> Followed { get; set; } = new List<Follow>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();

}

