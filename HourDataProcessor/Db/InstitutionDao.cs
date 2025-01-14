using System.Data;
using HourDataProcessor.Entity;
using HourDataProcessor.utils;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace HourDataProcessor.Db;

public class InstitutionDao
{
    /// <summary>
    /// "암호화요양기호" 또는 이름과 위치가 일치하는 레코드를 찾습니다.
    /// InstitutionHour Table과 Join 합니다.
    /// </summary>
    public List<Institution> FindByNameAndLocation(Institution institution)
    {
        var query = BuildQuery(institution);
        var command = CreateCommand(query);
        AddParameters(command, institution);

        using var reader = command.ExecuteReader();
        var result = CreateInstitutionFromReader(reader);

        return result;
    }

    private string BuildQuery(Institution institution)
    {
        var distanceQuery = string.Empty;
        var additionalConditionQuery = string.Empty;
        if (institution is { Longitude: not null, Latitude: not null })
        {
            distanceQuery = ", Location.STDistance(geography::Point(@latitudeVar, @longitudeVar, 4326)) as Distance";
            additionalConditionQuery = @"
            OR(
                Name = @Name
                AND Location IS NOT NULL 
                AND Location.STDistance(geography::Point(@latitudeVar, @longitudeVar, 4326)) <= 2000
            )";
        }

        var declareQuery = @"
        DECLARE @latitudeVar FLOAT = @Latitude;
        DECLARE @longitudeVar FLOAT = @Longitude;
        ";

        var selectQuery = $@"
        SELECT 
            Institution.Id AS Id,
            Code,
            Name, 
            Address, 
            InstitutionType,
            PhoneNumber,
            Location.STAsText() AS LocationText,
            InstitutionHour.Id AS HourId,
            MonStart, MonEnd, TuesStart, TuesEnd, WedStart, WedEnd, ThursStart, ThursEnd, FriStart, FriEnd, SatStart, SatEnd, SunStart, SunEnd
            {distanceQuery}
        FROM 
            Institution
        LEFT JOIN InstitutionHour
            ON Institution.Id = InstitutionHour.InstitutionId
        WHERE 
            (@Code IS NOT NULL AND Code = @Code)
            {additionalConditionQuery}
        ;";


        return declareQuery + selectQuery;
    }

    private void AddParameters(SqlCommand command, Institution institution)
    {
        command.Parameters.AddWithValue("@InstitutionType", institution.InstitutionType.ToString());
        var sqlParameter = command.Parameters.Add("@Code", SqlDbType.VarChar, 100);
        sqlParameter.Value = institution.Code ?? (object)DBNull.Value;
        command.Parameters.AddWithValue("@Name", institution.Name);
        command.Parameters.AddWithValue("@Latitude", institution.Latitude ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Longitude", institution.Longitude ?? (object)DBNull.Value);
    }

    public List<Institution> CreateInstitutionFromReader(SqlDataReader reader)
    {
        var institutions = new List<Institution>();

        if (!reader.HasRows) return institutions;

        while (reader.Read())
        {
            var point = ParseLocation(reader["LocationText"] as string);
            var institution = new Institution
            {
                Id = reader.GetInt32(0),
                Code = reader["Code"] as string,
                Name = reader["Name"] as string,
                Address = reader["Address"] as string,
                PhoneNumber = reader["PhoneNumber"] as string,
                InstitutionType = InstitutionTypeHelper.TryParse(reader["InstitutionType"] as string),
                Latitude = Convert.ToDouble(point.Latitude),
                Longitude = Convert.ToDouble(point.Longitude),
            };
            
            var hourId = reader.GetNullableInt32("HourId");
            if (hourId.HasValue)
            {
                institution.BusinessHours = CreateBusinessHours(reader);
            }
            institutions.Add(institution);
        }

        return institutions;
    }
    
    private List<BusinessHour> CreateBusinessHours(SqlDataReader reader)
    {
        var businessHours = new List<BusinessHour>();
        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            var dayPrefix = day.GetDayPrefix();
            var startTime = reader[$"{dayPrefix}Start"] as string;
            var endTime = reader[$"{dayPrefix}End"] as string;
            businessHours.Add(new BusinessHour
            {
                DayOfWeek = day,
                StartHour = startTime,
                EndHour = endTime
            });
        }

        return businessHours;
    }

    private (double? Latitude, double? Longitude) ParseLocation(string? locationText)
    {
        if (locationText.IsNullOrEmpty())
        {
            return (null, null);
        }

        var coordinates = locationText.Substring(7, locationText.Length - 8).Split(' ');
        var longitude = coordinates[0];
        var latitude = coordinates[1];

        return (Convert.ToDouble(longitude), Convert.ToDouble(latitude));
    }

    /// <summary>
    /// Institution 인스턴스를 활용해 Insert Query를 수행합니다.
    /// </summary>
    /// <param name="institution">Id 값이 없는 병원/약국 정보</param>
    public void Save(Institution institution)
    {
        var query = @"
            INSERT INTO Institution (Code, Name, Address, PhoneNumber, InstitutionType, Location)
            OUTPUT INSERTED.Id
            VALUES (@Code, @Name, @Address, @PhoneNumber, @InstitutionType,
            CASE 
                WHEN @Latitude IS NULL OR @Longitude IS NULL THEN NULL
                ELSE geography::Point(@Latitude, @Longitude, 4326)
            END
        );";

        using var command = CreateCommand(query);
        command.Parameters.AddWithValue("@Code", institution.Code ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Name", institution.Name);
        command.Parameters.AddWithValue("@Address", institution.Address ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@PhoneNumber", institution.PhoneNumber ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@InstitutionType", institution.InstitutionType.ToString());
        command.Parameters.AddWithValue("@Latitude", institution.Latitude ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Longitude", institution.Longitude ?? (object)DBNull.Value);

        var id = Convert.ToInt32(command.ExecuteScalar());
        institution.Id = id;

        SaveBusinessHours(institution);
    }

    public void SaveBusinessHours(Institution institution)
    {
        if (institution.BusinessHours == null) return;
        if (institution.Id == 11800)
        {
            Console.Out.WriteLine("hi");
        }

        var query = @"
                    INSERT INTO InstitutionHour (
                            InstitutionId, 
                            MonStart, MonEnd, TuesStart, TuesEnd, WedStart, WedEnd, ThursStart, ThursEnd, FriStart, FriEnd, SatStart, SatEnd, SunStart, SunEnd
                    ) 
                    VALUES (@InstitutionId, @MonStart, @MonEnd, @TuesStart, @TuesEnd, @WedStart, @WedEnd, @ThursStart, @ThursEnd, @FriStart, @FriEnd, @SatStart, @SatEnd, @SunStart, @SunEnd);
            ";

        var command = CreateCommand(query);
        command.Parameters.AddWithValue("@InstitutionId", institution.Id);
        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            var prefix = day.GetDayPrefix();
            var businessHour = institution.BusinessHours.FirstOrDefault(hour => hour.DayOfWeek == day);
            command.Parameters.AddWithValue($"@{prefix}Start", businessHour?.StartHour ?? (object)DBNull.Value);
            command.Parameters.AddWithValue($"@{prefix}End", businessHour?.EndHour ?? (object)DBNull.Value);
        }

        command.ExecuteNonQuery();
    }

    public void Update(Institution institution)
    {
        var query = @"
            UPDATE Institution
            SET 
                Address = @Address,
                PhoneNumber = @PhoneNumber
            WHERE Id = @Id;";

        using var command = CreateCommand(query);
        command.Parameters.AddWithValue("@Id", institution.Id);
        command.Parameters.AddWithValue("@PhoneNumber", institution.PhoneNumber ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Address", institution.Address ?? (object)DBNull.Value);

        command.ExecuteNonQuery();
    }
    
    public void UpdateBusinessHour(Institution institution)
    {
        if (institution.BusinessHours == null || institution.BusinessHours.Count == 0)
            return;

        var query = @"
                UPDATE InstitutionHour
                SET 
                    MonStart = @MonStart, MonEnd = @MonEnd, 
                    TuesStart = @TuesStart, TuesEnd = @TuesEnd, 
                    WedStart = @WedStart, WedEnd = @WedEnd, 
                    ThursStart = @ThursStart, ThursEnd = @ThursEnd, 
                    FriStart = @FriStart, FriEnd = @FriEnd, 
                    SatStart = @SatStart, SatEnd = @SatEnd, 
                    SunStart = @SunStart, SunEnd = @SunEnd 
                WHERE InstitutionId = @FK;";

        using var command = CreateCommand(query);
        command.Parameters.AddWithValue("@FK", institution.Id);
        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            var prefix = day.GetDayPrefix();
            var businessHour = institution.BusinessHours.FirstOrDefault(hour => hour.DayOfWeek == day);
            command.Parameters.AddWithValue($"@{prefix}Start", businessHour?.StartHour ?? (object)DBNull.Value);
            command.Parameters.AddWithValue($"@{prefix}End", businessHour?.EndHour ?? (object)DBNull.Value);
        }


        command.ExecuteNonQuery();
    }

    private SqlCommand CreateCommand(string query)
    {
        return new SqlCommand(query, TransactionHolder.Connection.Value, TransactionHolder.Transaction.Value);
    }
}

public static class SqlDataReaderExtensions
{
    public static int? GetNullableInt32(this SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    public static int? GetNullableInt32(this SqlDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    public static double? GetNullableDouble(this SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetDouble(ordinal);
    }
}