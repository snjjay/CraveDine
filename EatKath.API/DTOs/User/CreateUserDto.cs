namespace EatKath.API.DTOs.User
{
    public class CreateUserDto
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}