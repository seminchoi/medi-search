using LogServer.Data;
using LogServer.Dtos;
using LogServer.Models;
using LogServer.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace LogServer.Services;

public class InstitutionService
{
    private readonly IServiceProvider _serviceProvider;

    public InstitutionService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public List<InstitutionDto> SearchOpenHospitalsInRange(
        double latitude,
        double longitude,
        double radiusInMeters,
        string institutionType
    )
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var currentLocation = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));

        var currentTime = DateTime.Now;
        var currentHour = currentTime.ToString("HHmm");

        var hourColumn = currentTime.DayOfWeek.GetBusinessHourColumns();

        var institutions = context.Institutions
            .Include(institution => institution.InstitutionHour)
            .Where(institution =>
                institution.InstitutionType.Equals(institutionType) &&
                institution.Location != null &&
                institution.Location.Distance(currentLocation) <= radiusInMeters &&
                institution.InstitutionHour != null &&
                EF.Property<string?>(institution.InstitutionHour, hourColumn.startColumn) != null &&
                EF.Property<string?>(institution.InstitutionHour, hourColumn.endColumn) != null &&
                EF.Property<string?>(institution.InstitutionHour, hourColumn.startColumn).CompareTo(currentHour) <= 0 &&
                EF.Property<string?>(institution.InstitutionHour, hourColumn.endColumn).CompareTo(currentHour) >= 0)
            .ToList();
        var institutionDtos = institutions.Select(h => 
        {
            var (todayOpen, todayClose) = GetTodayBusinessHours(h.InstitutionHour, currentTime.DayOfWeek);
            return new InstitutionDto
            {
                Id = h.Id,
                Name = h.Name,
                Latitude = h.Location.Coordinate.Y,
                Longitude = h.Location.Coordinate.X,
                Address = h.Address,
                PhoneNumber = h.PhoneNumber,
                TodayOpen = todayOpen,
                TodayClose = todayClose
            };
        }).ToList();

        return institutionDtos;
    }
    
    
    private (string? start, string? end) GetTodayBusinessHours(InstitutionHour hours, DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Monday => (hours.MonStart, hours.MonEnd),
            DayOfWeek.Tuesday => (hours.TuesStart, hours.TuesEnd),
            DayOfWeek.Wednesday => (hours.WedStart, hours.WedEnd),
            DayOfWeek.Thursday => (hours.ThursStart, hours.ThursEnd),
            DayOfWeek.Friday => (hours.FriStart, hours.FriEnd),
            DayOfWeek.Saturday => (hours.SatStart, hours.SatEnd),
            DayOfWeek.Sunday => (hours.SunStart, hours.SunEnd),
            _ => throw new ArgumentException("Invalid day of week")
        };
    }
}