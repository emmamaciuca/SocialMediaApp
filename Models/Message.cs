using System.ComponentModel.DataAnnotations;

namespace SocialMediaApp.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Continutul mesajului este obligatoriu")]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        //un mesag apartine unui grup
        public int GroupId { get; set; }

        //un mesaj este postat de un user
        public string? UserId {  get; set; }

        // proprietatea virtuala un mesaj este postat de catre un user
        public virtual ApplicationUser? User { get; set; }

        // proprietatea virtuala un mesaj apartine unui grup
        public virtual Group? Group { get; set; }
    }

}
