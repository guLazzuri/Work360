using System.ComponentModel.DataAnnotations;

namespace Work360.Domain.Entity;

public class User
{
    [Key]
    [Display(Name = "ID do Usuário")]
    [Required(ErrorMessage = "O ID do usuário é obrigatório.")]
    public Guid UserID {get; set;}

    [Display(Name = "Nome do Usuário")]
    [Required(ErrorMessage = "O nome do usuário é obrigatório.")]
    public string Name {get; set;}

    [Display(Name = "Senha do Usuário")]
    [Required(ErrorMessage = "A senha do usuário é obrigatória.")]
    public string Password {get; set;}




}
