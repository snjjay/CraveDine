public interface ICurrentUserService
{
    int UserId { get; }
    string Role { get; }
    bool IsAdmin { get; }
}