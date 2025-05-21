using System.ComponentModel.DataAnnotations;

namespace FYP1_System___Individual.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role {  get; set; } = string.Empty;

        public bool HasRole(string role) => Role.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(role, StringComparer.OrdinalIgnoreCase);

        public void AddRole(string role)
        {
            var roles = Role.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!roles.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                roles.Add(role);
                Role = string.Join(",", roles);
            }
        }

        public void RemoveRole(string role)
        {
            var roles = Role.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            roles.RemoveAll(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
            Role = string.Join(",", roles);
        }
    }
}
