using AutoMapper;
using AutoMapper.QueryableExtensions;
using EatKath.API.Data;
using EatKath.API.DTOs.Reservation;
using EatKath.API.Entities;
using EatKath.API.Enums;
using EatKath.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;

        public ReservationService(
            ApplicationDbContext context,
            IMapper mapper,
            ICurrentUserService currentUser)
        {
            _context = context;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<ReservationDto> CreateAsync(CreateReservationDto dto)
        {
            var deal = await _context.Deals
                .FirstOrDefaultAsync(d => d.Id == dto.DealId);

            if (deal == null)
                throw new Exception("Deal not found.");

            // Check reservation limit
            if (deal.ReservationLimit > 0)
            {
                var reservationCount = await _context.Reservations
                    .CountAsync(r =>
                        r.DealId == dto.DealId &&
                        r.Status != ReservationStatus.Cancelled &&
                        r.Status != ReservationStatus.Rejected &&
                        r.Status != ReservationStatus.NoShow);

                if (reservationCount >= deal.ReservationLimit)
                    throw new Exception("This deal is fully booked.");
            }

            var reservation = _mapper.Map<Reservation>(dto);

            reservation.UserId = _currentUser.UserId;
            _context.Reservations.Add(reservation);

            await _context.SaveChangesAsync();

            // Automatically create redemption
            var redemption = new Redemption
            {
                DealId = reservation.DealId,
                UserId = reservation.UserId,
                ArrivalDate = reservation.ReservationDate,
                ArrivalTime = reservation.ReservationTime,
                GuestCount = reservation.GuestCount,
                Status = RedemptionStatus.Redeemed,
                RedeemedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Redemptions.Add(redemption);

            await _context.SaveChangesAsync();

            return _mapper.Map<ReservationDto>(reservation);
        }

        public async Task<IEnumerable<ReservationDto>> GetAllAsync()
        {
            return await _context.Reservations
                .ProjectTo<ReservationDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<IEnumerable<OwnerReservationDto>> GetOwnerReservationsAsync(int ownerId)
        {
            var reservations = await _context.Reservations
                .Include(r => r.Deal)
                    .ThenInclude(d => d.Restaurant)
                .Where(r => r.Deal.Restaurant.OwnerId == ownerId)
                .ToListAsync();

            var result = new List<OwnerReservationDto>();

            foreach (var reservation in reservations)
            {
                var redemption = await _context.Redemptions
                    .FirstOrDefaultAsync(r =>
                        r.UserId == reservation.UserId &&
                        r.DealId == reservation.DealId &&
                        r.ArrivalDate == reservation.ReservationDate &&
                        r.ArrivalTime == reservation.ReservationTime);

                result.Add(new OwnerReservationDto
                {
                    Id = reservation.Id,
                    RedemptionId = redemption?.Id,
                    DealId = reservation.DealId,
                    DealTitle = reservation.Deal.Title,
                    CustomerName = reservation.CustomerName,
                    PhoneNumber = reservation.PhoneNumber,
                    Email = reservation.Email,
                    ReservationDate = reservation.ReservationDate,
                    ReservationTime = reservation.ReservationTime,
                    GuestCount = reservation.GuestCount,
                    Status = reservation.Status
                });
            }

            return result;
        }


        public async Task<IEnumerable<ReservationDto>> GetMyReservationsAsync(int userId)
        {
            return await _context.Reservations
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ProjectTo<ReservationDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<ReservationDto?> GetByIdAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
                return null;

            return _mapper.Map<ReservationDto>(reservation);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
                return false;

            _context.Reservations.Remove(reservation);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ConfirmReservationAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
                return false;

            reservation.Status = ReservationStatus.Confirmed;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CancelReservationAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
                return false;

            reservation.Status = ReservationStatus.Cancelled;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RejectReservationAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
                return false;

            reservation.Status = ReservationStatus.Rejected;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ArriveReservationAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
                return false;

            reservation.Status = ReservationStatus.Arrived;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CompleteReservationAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
                return false;

            reservation.Status = ReservationStatus.Completed;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> NoShowReservationAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
                return false;

            reservation.Status = ReservationStatus.NoShow;

            await _context.SaveChangesAsync();

            return true;
        }

    }
}