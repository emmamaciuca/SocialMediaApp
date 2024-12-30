using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections;

namespace SocialMediaApp.Models;

public class Group
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Numele grupului este obligatoriu")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Descrierea grupului este obligatoriu")]
    public string Content { get; set; }


    //grupul e creat de un user - moderator
    public string? UserId { get; set; }
    public virtual ApplicationUser? User { get; set; }

    // un grup are o colectie de mesaje 
    public virtual ICollection<Message>? Messages { get; set; }

    //many-to-many dintre grup si user
    public virtual ICollection<UserGroup>? UserGroups { get; set; }

}