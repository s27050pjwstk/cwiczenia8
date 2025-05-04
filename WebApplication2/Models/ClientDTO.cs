using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class ClientDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Phone]
        public string Telephone { get; set; }
        public string Pesel { get; set; }
    }
}
