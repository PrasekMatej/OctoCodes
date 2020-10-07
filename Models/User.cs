using System.ComponentModel.DataAnnotations;

namespace OctoCodes.Models
{
    public class User
    {
        [Key]
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
