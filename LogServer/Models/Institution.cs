using System.ComponentModel.DataAnnotations;
using Point = NetTopologySuite.Geometries.Point;

namespace LogServer.Models;

public class Institution
{
    [Key] public int Id { get; set; }

    [StringLength(100)] public string? Code { get; set; }

    [Required] [StringLength(100)] public string Name { get; set; } = string.Empty;

    [StringLength(200)] public string? Address { get; set; }
    [StringLength(200)] public string? PhoneNumber { get; set; }
    [StringLength(200)] public string? InstitutionType { get; set; }

    public Point? Location { get; set; }

    public virtual InstitutionHour? InstitutionHour { get; set; }
}