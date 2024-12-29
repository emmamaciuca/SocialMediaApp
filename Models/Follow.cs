using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SocialMediaApp.Models;
namespace SocialMediaApp.Models;

public class Follow
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    // cheie primara compusa (Id, FollowerId, FollowedId)
    public string Id { get; set; }
    public string? FollowerId { get; set; }
    public string? FollowedId { get; set; }

    public virtual ApplicationUser? Follower { get; set; }
    public virtual ApplicationUser? Followed { get; set; }

    [Required]
    public string Status { get; set; } = "Pending"; // Default: cerere în așteptare

}

