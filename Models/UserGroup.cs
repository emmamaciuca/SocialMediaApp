using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaApp.Models;

public class UserGroup
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    // cheie primara compusa (Id, UserId, GroupId)
    public int Id { get; set; }
    public string? UserId { get; set; }
    public int? GroupId { get; set; }
    //statusul cererii - Pending, Accepted, Rejected
    public string Status { get; set; } = "Pending";  //default pending

    public virtual ApplicationUser? User { get; set; }
    public virtual Group? Group { get; set; }
}

