using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Курсовая_работа_MVC.Models
{
    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "Имя обязательно")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        [PasswordComplexity]
        public string Password { get; set; }

        public bool IsAdmin { get; set; }
    }
    public class PasswordComplexityAttribute : ValidationAttribute
    {
        public PasswordComplexityAttribute()
        {
            ErrorMessage = "Пароль должен содержать минимум 8 символов, хотя бы одну заглавную букву, одну строчную и одну цифру. " +
                           "Допускаются только буквы латинского алфавита.";
        }

        public override bool IsValid(object value)
        {
            if (value == null) return false;

            string password = value.ToString();

            // Проверка условий
            return password.Length >= 8 &&
                   Regex.IsMatch(password, @"[A-Z]") &&
                   Regex.IsMatch(password, @"[a-z]") &&
                   Regex.IsMatch(password, @"\d");
        }
    }
}
