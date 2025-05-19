namespace Курсовая_работа_MVC.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; } 
        public bool IsAdmin { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}
