using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShopDomain.Models
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Поле повного імені є обов’язковим")]
        [StringLength(100, ErrorMessage = "Ім’я не може бути довшим за 100 символів")]
        public string FullName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Невірний формат номера телефону")]
        public string PhoneNumber { get; set; }

        public IList<string> Roles { get; set; }
    }
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введіть email")]
        [EmailAddress(ErrorMessage = "Невірний формат email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введіть повне ім’я")]
        [StringLength(100, ErrorMessage = "Ім’я не може бути довшим за 100 символів")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Введіть пароль")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль має бути від 6 до 100 символів")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Паролі не збігаються")]
        public string ConfirmPassword { get; set; }
    }
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введіть email")]
        [EmailAddress(ErrorMessage = "Невірний формат email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введіть пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Введіть поточний пароль")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Введіть новий пароль")]
        [StringLength(100, ErrorMessage = "Пароль має бути від {2} до {1} символів", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Паролі не збігаються")]
        public string ConfirmPassword { get; set; }
    }
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
    }
}