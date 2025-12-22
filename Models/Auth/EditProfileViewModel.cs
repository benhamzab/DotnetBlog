using System.ComponentModel.DataAnnotations;

namespace BLOGAURA.Models.Auth
{
    public class EditProfileViewModel
    {
        [StringLength(500, ErrorMessage = "La description ne doit pas dépasser 500 caractères")]
        [Display(Name = "À propos")]
        public string? About { get; set; }

        [StringLength(200, ErrorMessage = "Le nom de l'université ne doit pas dépasser 200 caractères")]
        [Display(Name = "Université / École")]
        public string? University { get; set; }

        [StringLength(100, ErrorMessage = "Le niveau ne doit pas dépasser 100 caractères")]
        [Display(Name = "Niveau d'études")]
        public string? Grade { get; set; }

        [StringLength(100, ErrorMessage = "La ville ne doit pas dépasser 100 caractères")]
        [Display(Name = "Ville")]
        public string? City { get; set; }

        [Range(13, 99, ErrorMessage = "L'âge doit être entre 13 et 99 ans")]
        [Display(Name = "Âge")]
        public int? Age { get; set; }

        [StringLength(500, ErrorMessage = "Les hobbies ne doivent pas dépasser 500 caractères")]
        [Display(Name = "Hobbies / Centres d'intérêt")]
        public string? Hobbies { get; set; }

        [StringLength(300, ErrorMessage = "Les types d'événements ne doivent pas dépasser 300 caractères")]
        [Display(Name = "Types d'événements préférés")]
        public string? FavoriteEventTypes { get; set; }

        [StringLength(300, ErrorMessage = "Ce champ ne doit pas dépasser 300 caractères")]
        [Display(Name = "Actuellement en apprentissage")]
        public string? LearningNow { get; set; }

        [StringLength(200, ErrorMessage = "L'URL ne doit pas dépasser 200 caractères")]
        [Url(ErrorMessage = "Veuillez entrer une URL valide")]
        [Display(Name = "GitHub URL")]
        public string? GithubUrl { get; set; }

        [StringLength(200, ErrorMessage = "L'URL ne doit pas dépasser 200 caractères")]
        [Url(ErrorMessage = "Veuillez entrer une URL valide")]
        [Display(Name = "LinkedIn URL")]
        public string? LinkedinUrl { get; set; }

        [StringLength(200, ErrorMessage = "L'URL ne doit pas dépasser 200 caractères")]
        [Url(ErrorMessage = "Veuillez entrer une URL valide")]
        [Display(Name = "Portfolio URL")]
        public string? PortfolioUrl { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = "Photo de profil")]
        public Microsoft.AspNetCore.Http.IFormFile? ProfileImage { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = "Photo de couverture")]
        public Microsoft.AspNetCore.Http.IFormFile? NewCoverImage { get; set; }

        public string? CurrentCoverUrl { get; set; }
    }
}
