using CsvHelper.Configuration;

namespace HourDataProcessor.Entity;

public class Institution
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    //경도, x좌표
    public string Longitude { get; set; } = string.Empty;

    //위도, y좌표
    public string Latitude { get; set; } = string.Empty;

    public string? Code { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }

    public InstitutionType InstitutionType { get; set; }
    public List<BusinessHours>? BusinessHours { get; set; }

    /// <summary>
    /// 기존의 데이터와 새로 불러온 데이터를 병합합니다.
    /// </summary>
    /// <remarks>
    /// - 메소드를 호출하는 인스턴스는 새로 불러온 데이터이며, 파라미터가 기존 데이터이어야 합니다.
    /// </remarks>
    public void CombineWithOriginal(Institution original)
    {
        Code ??= original.Code;
        Address ??= original.Address;
        BusinessHours ??= original.BusinessHours;
    }

    /// <summary>
    /// 새로 읽어온 데이터와 DB에 저장된 기존 데이터를 비교해 변경을 감지합니다. 
    /// </summary>
    // 위도, 경도(Longitude, Latitude) 필드는 변경 감지에서 제외합니다.
    // 동등한 병원/약국임에도 공공데이터 제공 기관에 따라 좌표 문자열이 완전히 일치하지 않는 경우가 있기 때문입니다.
    public bool DirtyCheck(Institution other)
    {
        return Code == other.Code && Name == other.Name && Address == other.Address 
               && PhoneNumber == other.PhoneNumber && InstitutionType == other.InstitutionType;
    }

    // 위경도를 이용해 other과의 거리를 계산하고 근접한지 판단해줍니다.
    private bool IsNear(Institution other)
    {
        return CalculateDistance(other) <= 10.0;
    }

    private double CalculateDistance(Institution other)
    {
        const double earthRadius = 6371.0;

        var newLatitude = Convert.ToDouble(Latitude);
        var newLongitude = Convert.ToDouble(Longitude);
        var originLatitude = Convert.ToDouble(other.Latitude);
        var originLongitude = Convert.ToDouble(other.Longitude);


        var distance = earthRadius * Math.Acos(
            Math.Cos(originLatitude) * Math.Cos(newLatitude) * Math.Cos(newLongitude - originLongitude) +
            Math.Sin(originLatitude) * Math.Sin(newLatitude)
        );

        return distance;
    }
}

public class BusinessHours : IEquatable<BusinessHours>
{
    public DayOfWeek DayOfWeek { get; set; }
    public string? StartHour { get; set; }
    public string? EndHour { get; set; }

    public bool Equals(BusinessHours? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return DayOfWeek == other.DayOfWeek && StartHour == other.StartHour && EndHour == other.EndHour;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((BusinessHours)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)DayOfWeek, StartHour, EndHour);
    }
}