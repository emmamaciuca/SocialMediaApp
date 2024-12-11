using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SocialMediaApp.Models;

public class Comment
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Continutul comentariului este obligatoriu")]
    public string Content { get; set; }

    public DateTime Date { get; set; }

    //cheie externa comentariu apartine unei postari
    public int PostId { get; set; }

    //cheie externa comentariu e postat de user

    public string? UserId { get; set; }

    //prop virtuala comentariu e postat de user 
    public virtual ApplicationUser? User { get; set; }

    //prop virtuala comentariu apartine unei postari
    public virtual Post? Post { get; set; }


}