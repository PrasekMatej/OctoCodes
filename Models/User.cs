using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OctoCodes.Models
{
    public class User
    {
        [Key]
        [Display(Name = "Uživatelské jméno")]
        [Required(ErrorMessage = "Zadejte uživatelské jméno")]
        public string Username { get; set; }

        [Display(Name = "Heslo")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Zadejte heslo")]
        public string Password { get; set; }

        [Display(Name = "Ověření hesla")]
        [NotMapped]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Hesla se musí shodovat")]
        public string ConfirmPassword { get; set; }
    }
}
