using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Work360.Domain.Entity
{

    public class User
    {

        [Key]
        [Display(Name = "ID do Usuário")]
        [Required(ErrorMessage = "O ID do usuário é obrigatório.")]
        public Guid UserID { get; set; } = Guid.NewGuid();

        [Display(Name = "Nome do Usuário")]
        [Required(ErrorMessage = "O nome do usuário é obrigatório.")]
        public required string Name { get; set; }

        [Display(Name = "Email do Usuário")]
        [Required(ErrorMessage = "O email do usuário é obrigatório.")]
        public required string Email { get; set; }

        [Display(Name = "Senha do Usuário")]
        [Required(ErrorMessage = "A senha do usuário é obrigatória.")]
        public required string Password { get; set; }

    }
}