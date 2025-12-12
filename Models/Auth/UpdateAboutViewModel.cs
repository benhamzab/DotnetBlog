using System.ComponentModel.DataAnnotations;

namespace BLOGAURA.Models.Auth
{
    public class UpdateAboutViewModel
    {
        [StringLength(500, ErrorMessage = "La description ne doit pas dépasser 500 caractères")]
        [Display(Name = "À propos")]
        public string? About { get; set; }
    }
}
