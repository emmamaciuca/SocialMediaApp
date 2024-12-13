using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SocialMediaApp.Models;

public class Post
{
    [Key]
    public int Id { get; set; }

    //validare pt required ?? si min/max length
    public string Content { get; set; }

    public DateTime Date { get; set; }

    //imagine
    public string? Image { get; set; }

    //video

    // cheie externa user - o postare e asociata unui user
    public string? UserId { get; set; }
    
    public virtual ApplicationUser? User { get; set; }


    // o postare are o colectie de comantarii 
    public virtual ICollection<Comment>? Comments { get; set; }
}