namespace EatKath.API.Entities
{
    public class User : BaseEntity
    {
        

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public bool IsActive { get; set; } = true;

        

        // Navigation Properties
        public Role Role { get; set; } = null!;

        public ICollection<Restaurant> Restaurants { get; set; } = new List<Restaurant>();

        public ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();

        public ICollection<Redemption> Redemptions { get; set; } = new List<Redemption>();

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}