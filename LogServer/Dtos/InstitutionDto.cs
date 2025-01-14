namespace LogServer.Dtos;

public record InstitutionDto
{
    public int Id { get; init; } 

    public string Name { get; init; }

    public double Latitude { get; init; }

    public double Longitude { get; init; }

    public string Address { get; init; } 

    public string PhoneNumber { get; init; }
    
    public string TodayOpen { get; init; }
    
    public string TodayClose { get; init; }
}