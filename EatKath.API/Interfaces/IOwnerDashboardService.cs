using EatKath.API.DTOs.Owner;

namespace EatKath.API.Interfaces
{
    public interface IOwnerDashboardService
    {
        Task<OwnerDashboardDto> GetDashboardAsync();
    }
}