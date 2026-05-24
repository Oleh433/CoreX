using System.ComponentModel.DataAnnotations;

namespace CoreX.Pages.Admin.Clubs.Models;

public class ClubInput
{
    [Required(ErrorMessage = "Введіть назву клубу.")]
    [StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть місто.")]
    [StringLength(50)]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть адресу.")]
    [StringLength(250)]
    public string Address { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Phone(ErrorMessage = "Введіть коректний номер.")]
    [StringLength(30)]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Введіть коректний email.")]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(150)]
    public string? WorkingHours { get; set; }

    [Url(ErrorMessage = "Введіть коректне посилання.")]
    [StringLength(500)]
    public string? PhotoUrl { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
