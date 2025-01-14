using HourDataProcessor.utils;
using Microsoft.IdentityModel.Logging;

namespace HourDataProcessor.Entity;

public class Institution
{
    public int? Id { get; set; }

    public string Name { get; set; } = string.Empty;

    //경도, x좌표
    public double? Longitude { get; set; }

    //위도, y좌표
    public double? Latitude { get; set; }

    public string? Code { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }

    public InstitutionType InstitutionType { get; set; }
    public List<BusinessHour>? BusinessHours { get; set; }

    /// <summary>
    /// 기존의 데이터와 새로 불러온 데이터를 병합합니다.
    /// </summary>
    /// <remarks>
    /// - 메소드를 호출하는 인스턴스는 새로 불러온 데이터이며, 파라미터가 기존 데이터이어야 합니다.
    /// </remarks>
    public void CombineWithOriginal(Institution original)
    {
        Id ??= original.Id;
        Address ??= original.Address;
        PhoneNumber ??= original.PhoneNumber;
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

    /// <summary>
    /// Uniquekey가 동일한지 확인합니다.
    /// </summary>
    /// 비교 대상 중 하나라도 null이면 false를 반환합니다.
    public bool EqualUniqueCode(Institution other)
    {
        if (string.IsNullOrEmpty(Code) || string.IsNullOrEmpty(other.Code))
            return false;

        return Code == other.Code;
    }
    
    /// <summary>
    /// 확실하진 않지만 어느정도의 동등성을 판단할 수 있는 필드들로 동등성을 판단합니다. 
    /// </summary>
    public bool EqualTo(Institution other)
    {
        // 이름이 다르면 false
        if (Name != other.Name)
            return false;

        // 시설 타입이 다르면 false
        if (InstitutionType != InstitutionType.Unknown && other.InstitutionType != InstitutionType.Unknown
                                                       && InstitutionType != other.InstitutionType)
            return false;

        // PhoneNumber가 존재하고 같으면 true
        if (!string.IsNullOrEmpty(PhoneNumber) && !string.IsNullOrEmpty(other.PhoneNumber)
                                               && PhoneNumber == other.PhoneNumber)
            return true;

        return false;
    }
    
    /// <summary>
    /// Uniquekey가 동일한지 확인합니다.
    /// </summary>
    public bool IsAddressSimilarTo(Institution other)
    {
        // Address가 존재하고 같으면 true
        if (!string.IsNullOrEmpty(Address) && !string.IsNullOrEmpty(other.Address)
                                           && Address == other.Address)
            return true;
        
        // 위의 조건으로 판단이 안되면 문자열 유사도 검사
        var similarity = CustomStringUtils.CalculateSimilarity(Address, other.Address);
        LogHelper.LogInformation($"{Address}와 {other.Address}의 유사도는 {similarity} 입니다.");
        return similarity >= 0.90;
    }
}

public class BusinessHour : IEquatable<BusinessHour>
{
    public DayOfWeek DayOfWeek { get; set; }
    public string? StartHour { get; set; }
    public string? EndHour { get; set; }

    public bool Equals(BusinessHour? other)
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
        return Equals((BusinessHour)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)DayOfWeek, StartHour, EndHour);
    }
}