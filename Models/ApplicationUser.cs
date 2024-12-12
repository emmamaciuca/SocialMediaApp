using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMediaApp.Models;

public class ApplicationUser: IdentityUser 
{
    //atribute required user 
    //nume complet
    [Required(ErrorMessage = "Prenumele este obligatorie")]
    public string? FirstName { get; set; }
    [Required(ErrorMessage = "Numele este obligatorie")]
    public string? LastName { get; set; }

    //descriere 
    [Required(ErrorMessage = "Descrierea este obligatoriu")]
    public string Content { get; set; }

    //poza profil
    public string Image { get; set; }

    public string Visibility { get; set; }


    //un user poate posta mai multe comentarii
    public virtual ICollection<Comment>? Comments { get; set; } 

    //un user poate avea mai multe postari
    public virtual ICollection<Post>? Posts { get; set; } 

    //many-to-many dintre grup si user
    public virtual ICollection<UserGroup>? UserGroup { get; set; }

    //many-to-many dintre user si user - follow
    //userii care il urmaresc pe utilizatorul curent
    public virtual ICollection<Follow>? Followers { get; set; }

    //userii pe care utilizatorul curent ii urmareste 
    public virtual ICollection<Follow>? Following { get; set; }



    // variabila in care vom retine rolurile existente in baza de date
    // pentru popularea unui dropdown list
    [NotMapped]
    public IEnumerable<SelectListItem>? AllRoles { get; set; }    

}