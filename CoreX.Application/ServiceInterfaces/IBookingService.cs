using CoreX.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreX.Application.ServiceInterfaces
{
    public interface IBookingService
    {
        Task<BookingResponseDto?> GetByIdAsync(Guid id);

        Task<List<BookingResponseDto>> GetByUserIdAsync(Guid userId);

        Task<Guid> CreateAsync(CreateBookingDto dto);

        Task<bool> ConfirmAsync(Guid bookingId);

        Task<bool> CancelAsync(Guid bookingId);
    }
}
