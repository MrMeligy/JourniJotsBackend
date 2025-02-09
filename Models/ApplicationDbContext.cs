using Microsoft.EntityFrameworkCore;

namespace Backend.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Follow> Followings { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Trip_Activity> Trip_Activities { get; set; }
        public DbSet<Trip_Hotel> Trip_Hotels { get; set; }
        public DbSet<Trip_Restaurant> Trip_Restaurants { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostImage> PostImages { get; set; }
        public DbSet<Like> PostLikes { get; set; }
        public DbSet<Comment> PostComments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Composite Key of Follow Table
            modelBuilder.Entity<Follow>()
                .HasKey(k => new { k.UserId1, k.UserId2 });
            //Trip CompositKeys
            modelBuilder.Entity<Trip_Activity>()
                .HasKey(k => new { k.TripId, k.ActivityId });
            modelBuilder.Entity<Trip_Hotel>()
                .HasKey(k => new { k.TripId, k.HotelId });
            modelBuilder.Entity<Trip_Restaurant>()
                .HasKey(k => new { k.TripId, k.RestaurantId });
            //Post CompositeKeys
            modelBuilder.Entity<Like>()
                .HasKey(k => new { k.PostId, k.UserId });

            //Relations of User
            //Follow
            modelBuilder.Entity<Follow>()
                .HasOne(f => f.User1)
                .WithMany(u => u.Follow)
                .HasForeignKey(f => f.UserId1)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Follow>()
                .HasOne(f => f.User2)
                .WithMany(u => u.Followed)
                .HasForeignKey(f => f.UserId2)
                .OnDelete(DeleteBehavior.Restrict);
            //Trips
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.User)
                .WithMany(u => u.Trips)
                .HasForeignKey(t => t.UserId);
            //Posts
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId);
            //Relations of Trip
            modelBuilder.Entity<Trip_Activity>()
                .HasOne(ta => ta.Trip)
                .WithMany(t => t.Activities)
                .HasForeignKey(ta => ta.TripId);
            modelBuilder.Entity<Trip_Hotel>()
                .HasOne(th => th.Trip)
                .WithMany(t => t.Hotels)
                .HasForeignKey(th => th.TripId);
            modelBuilder.Entity<Trip_Restaurant>()
                .HasOne(tr => tr.Trip)
                .WithMany(t => t.Restaurants)
                .HasForeignKey(tr => tr.TripId);
            //Relations of Posts
            //Images
            modelBuilder.Entity<PostImage>()
                .HasOne(i => i.Post)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.PostId);

            //Likes
            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.PostLikes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.NoAction);
            //Comments
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.PostComments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.NoAction);
            base.OnModelCreating(modelBuilder);
        }
    }
}
