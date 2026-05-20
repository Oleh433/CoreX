using CoreX.Application.DTO;
using CoreX.Application.Mappers;
using CoreX.Application.ServiceInterfaces;
using CoreX.Domain;
using CoreX.Domain.Entities;
using CoreX.Domain.RepositoryInterfaces;

namespace CoreX.Application.Services
{
    public class ClubService : IClubService
    {
        private readonly IClubRepository _clubRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ClubService(
            IClubRepository clubRepository,
            IUnitOfWork unitOfWork)
        {
            _clubRepository = clubRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ClubResponseDto>> GetAllAsync()
        {
            var clubs = await _clubRepository.GetAllAsync();

            return clubs.Select(ClubMapper.ToDto).ToList();
        }

        public async Task<ClubResponseDto?> GetByIdAsync(Guid id)
        {
            var club = await _clubRepository.GetByIdAsync(id);

            if (club == null)
                return null;

            return ClubMapper.ToDto(club);
        }

        public async Task<List<ClubResponseDto>> GetByCityAsync(string city)
        {
            var clubs = await _clubRepository.GetByCityAsync(city);

            return clubs.Select(ClubMapper.ToDto).ToList();
        }

        public async Task<Guid> CreateAsync(CreateClubDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name is required.");

            if (string.IsNullOrWhiteSpace(dto.City))
                throw new ArgumentException("City is required.");

            if (string.IsNullOrWhiteSpace(dto.Address))
                throw new ArgumentException("Address is required.");

            var club = new Club(
                name: dto.Name,
                city: dto.City,
                address: dto.Address,
                latitude: dto.Latitude,
                longitude: dto.Longitude,
                description: dto.Description,
                phone: dto.Phone,
                email: dto.Email,
                workingHours: dto.WorkingHours,
                photoUrl: dto.PhotoUrl
            );

            await _clubRepository.AddAsync(club);

            await _unitOfWork.SaveChangesAsync();

            return club.Id;
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateClubDto dto)
        {
            var club = await _clubRepository.GetByIdAsync(id);

            if (club == null)
                return false;

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name is required.");

            if (string.IsNullOrWhiteSpace(dto.City))
                throw new ArgumentException("City is required.");

            if (string.IsNullOrWhiteSpace(dto.Address))
                throw new ArgumentException("Address is required.");

            club.Update(
                name: dto.Name,
                city: dto.City,
                address: dto.Address,
                latitude: dto.Latitude,
                longitude: dto.Longitude,
                description: dto.Description,
                phone: dto.Phone,
                email: dto.Email,
                workingHours: dto.WorkingHours,
                photoUrl: dto.PhotoUrl
            );

            _clubRepository.Update(club);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var club = await _clubRepository.GetByIdAsync(id);

            if (club == null)
                return false;

            _clubRepository.Delete(club);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
