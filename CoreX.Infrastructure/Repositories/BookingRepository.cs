using CoreX.Domain.Entities;
using CoreX.Domain.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreX.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Booking?> GetByIdAsync(Guid id)
        {
            return await _context.Bookings
                .Include(x => x.Club)
                .Include(x => x.Subscription)
                .Include(x => x.Discount)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(x => x.Club)
                .Include(x => x.Subscription)
                .Include(x => x.Discount)
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Bookings
                .Where(x => x.UserId == userId)
                .Include(x => x.Club)
                .Include(x => x.Subscription)
                .Include(x => x.Discount)
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetByClubIdAsync(Guid clubId)
        {
            return await _context.Bookings
                .Where(x => x.ClubId == clubId)
                .Include(x => x.Subscription)
                .Include(x => x.Discount)
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetActiveBookingsAsync()
        {
            return await _context.Bookings
                .Where(x =>
                    x.Status == BookingStatus.New ||
                    x.Status == BookingStatus.Confirmed)
                .Include(x => x.Club)
                .Include(x => x.Subscription)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
        }

        public void Update(Booking booking)
        {
            _context.Bookings.Update(booking);
        }

        public void Delete(Booking booking)
        {
            _context.Bookings.Remove(booking);
        }
    }
}
