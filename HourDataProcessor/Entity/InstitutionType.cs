namespace HourDataProcessor.Entity;

public enum InstitutionType
{
    // 병, 의원
    Hospital,

    // 약국
    DrugStore,

    // 보건시설
    HealthCenter,

    // 조산원
    MidwifeCenter,

    // 그 외
    Unknown
}

public static class InstitutionTypeHelper
{
    public static InstitutionType ValueOf(string? institutionName)
    {
        if (string.IsNullOrWhiteSpace(institutionName))
            return InstitutionType.Unknown;

        return institutionName switch
        {
            "병원" or "상급종합" or "요양병원" or "종합병원" or "의원"
                or "정신병원" or "치과병원" or "치과의원" or "한방병원" or "한의원" => InstitutionType.Hospital,
            "약국" => InstitutionType.DrugStore,
            "보건소" or "보건의료원" or "보건지소" or "보건진료소" => InstitutionType.HealthCenter,
            "조산원" => InstitutionType.MidwifeCenter,
            _ => ProcessOtherInstitutionName(institutionName)
        };
    }

    private static InstitutionType ProcessOtherInstitutionName(string institutionName)
    {

        if (institutionName.Contains("병원") || institutionName.Contains("의원"))
            return InstitutionType.Hospital;

        if (institutionName.Contains("약국"))
            return InstitutionType.DrugStore;

        if (institutionName.Contains("보건"))
            return InstitutionType.HealthCenter;

        if (institutionName.Contains("조산"))
            return InstitutionType.MidwifeCenter;

        return InstitutionType.Unknown;
    }
}