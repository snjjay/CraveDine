using AutoMapper;
using AutoMapper.QueryableExtensions;
using EatKath.API.Data;
using EatKath.API.DTOs.Reservation;
using EatKath.API.Entities;
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
            var reservation = _mapper.Map<Reservation>(dto);

            reservation.UserId = _currentUser.UserId;

            _context.Reservations.Add(reservation);

            await _context.SaveChangesAsync();

            return _mapper.Map<ReservationDto>(reservation);
        }

        public async Task<IEnumerable<ReservationDto>> GetAllAsync()
        {
            return await _context.Reservations
                .ProjectTo<ReservationDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservationDto>> GetOwnerReservationsAsync(int ownerId)
        {
            return await _context.Reservations
                .Include(r => r.Deal)
                    .ThenInclude(d => d.Restaurant)
                .Where(r => r.Deal.Restaurant.OwnerId == ownerId)
                .ProjectTo<ReservationDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
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