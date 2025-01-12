using HourDataProcessor.Entity;
using HourDataProcessor.utils;
using Microsoft.Data.SqlClient;

namespace HourDataProcessor.Db;

public class InstitutionDao
{
    // 이름과 위치를 기반으로 병원/약국을 검색하는 쿼리
    // 병원, 약국을 식별할 수 있는 유니크한 키값이 공공데이터 제공자에 따라서 다르기 때문에 이름과 위치를 기반으로 데이터를 검색한다. 
    // 좌표또한 공공데이터 제공자에 따라서 완벽하게 일치하지 않는 경우가 있다.
    // 따라서 다음과 같은 두가지 조건으로 동일한 병원, 약국인지 확인한다.
    //      1. 병원/약국 명이 일치한다.
    //      2. STDistance 함수로 거리를 계산했을 때 거의 동일한 위치(10m 이하의 오차)라고 판단된다.  
    // 만약 위 조건에 따라서 검색했을 때 검색 결과가 2건 이상이면 잘못된 결과로 판단하여 예외를 발생한다. 
    public Institution? FindByNameAndLocation(Institution institution)
    {
        var tableName = institution.InstitutionType.ToString();
        var hourTableName = tableName + "Hour";
        var fkName = tableName + "Id";
        var declareQuery = @"
            DECLARE @latitudeVar DOUBLE = @Latitude;
            DECLARE @longitudeVar DOUBLE = @Longitude;";

        var selectQuery = $@"
            SELECT 
                {tableName}.Id AS Id,
                Code,
                Name, 
                Address, 
                Location.STAsText() AS LocationText,
                {hourTableName}.Id AS HourId,
                MonStart, MonEnd, TuesStart, TuesEnd, WedStart, WedEnd, ThursStart, ThursEnd, FriStart, FriEnd, SatStart, SatEnd, SunStart, SunEnd
            FROM 
                {tableName}
            LEFT JOIN {hourTableName} 
                ON {tableName}.Id = {hourTableName}.{fkName}
            WHERE 
                (Code IS NOT NULL AND Code = @Code) 
                OR 
                (Name = @Name AND Location.STDistance(geography::Point(@latitudeVar, @longitudeVar, 4326)) <== 10);
            ";

        var command = CreateCommand(declareQuery + selectQuery);
        command.Parameters.AddWithValue("@Code", institution.Code);
        command.Parameters.AddWithValue("@Name", institution.Name);
        command.Parameters.AddWithValue("@Latitude", Convert.ToDouble(institution.Latitude));
        command.Parameters.AddWithValue("@Longitude", Convert.ToDouble(institution.Longitude));

        Institution? origin = null;
        int? hourId = null;

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var point = ParseLocation(reader["LocationText"] as string);
            origin = new Institution
            {
                Code = reader["Code"] as string,
                Name = reader["Name"] as string,
                Address = reader["Address"] as string,
                Latitude = point.Latitude,
                Longitude = point.Latitude,
            };
            var tempHourId = reader["HourId"];
            if (tempHourId != DBNull.Value)
            {
                hourId = Convert.ToInt32(tempHourId);
            }

            if (hourId.HasValue)
            {
                var businessHours = new List<BusinessHours>();

                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    var startTime = reader[$"{day.GetDayPrefix()}+Start"] as string;
                    var endTime = reader[day.GetDayPrefix() + "End"] as string;
                    businessHours.Add(new BusinessHours
                    {
                        DayOfWeek = day,
                        StartHour = startTime,
                        EndHour = endTime
                    });
                }

                institution.BusinessHours = businessHours;
            }
        }

        if (reader.Read())
        {
            throw new ApplicationException("한 개 이상의 결과를 찾았습니다.");
        }

        return origin;
    }

    private (string Latitude, string Longitude) ParseLocation(string locationText)
    {
        var coordinates = locationText.Replace("POINT(", "").Replace(")", "").Split(' ');
        var longitude = coordinates[0];
        var latitude = coordinates[1];

        return (latitude, longitude);
    }

    /// <summary>
    /// Institution 인스턴스를 활용해 Insert Query를 수행합니다.
    /// </summary>
    /// <param name="institution">Id 값이 없는 병원/약국 정보</param>
    public void Save(Institution institution)
    {
        var tableName = institution.InstitutionType.ToString();
        var query = $@"
                INSERT INTO {tableName} (Code, Name, Address, Location)
                    VALUES (@Code, @Name, @Address, geography::Point(@Latitude, @Longitude, 4326));";

        using var command = CreateCommand(query);
        command.Parameters.AddWithValue("@Code", (object?)institution.Code ?? DBNull.Value);
        command.Parameters.AddWithValue("@Name", institution.Name);
        command.Parameters.AddWithValue("@Address", (object?)institution.Address ?? DBNull.Value);
        command.Parameters.AddWithValue("@Latitude", Convert.ToDouble(institution.Latitude));
        command.Parameters.AddWithValue("@Longitude", Convert.ToDouble(institution.Longitude));

        command.ExecuteNonQuery();
        SaveBusinessHours(institution);
    }

    public void SaveBusinessHours(Institution institution)
    {
        if (institution.BusinessHours == null) return;
        var tableName = institution.InstitutionType.ToString() + "Hour";
        var fkName = institution.InstitutionType.ToString() + "Id";
        var query = $@"
                    INSERT INTO {tableName} (
                            {fkName}, 
                            MonStart, MonEnd, TuesStart, TuesEnd, WedStart, WedEnd, ThursStart, ThursEnd, FriStart, FriEnd, SatStart, SatEnd, SunStart, SunEnd
                    ) 
                    VALUES (@InstitutionId, @MonStart, @MonEnd, @TuesStart, @TuesEnd, @WedStart, @WedEnd, @ThursStart, @ThursEnd, @FriStart, @FriEnd, @SatStart, @SatEnd, @SunStart, @SunEnd);
            ";

        var command = CreateCommand(query);
        command.Parameters.AddWithValue("@InstitutionId", institution.Id);
        foreach (var businessHour in institution.BusinessHours)
        {
            var prefix = businessHour.DayOfWeek.GetDayPrefix();
            command.Parameters.AddWithValue($"@{prefix}Start", (object?)businessHour.StartHour ?? DBNull.Value);
            command.Parameters.AddWithValue($"@{prefix}End", (object?)businessHour.EndHour ?? DBNull.Value);
        }
    }

    public void Update(Institution institution)
    {
        var tableName = institution.InstitutionType.ToString();
        var query = $@"
            UPDATE {tableName}
            SET 
                Code = @Code, 
                Address = @Address, 
            WHERE Id = @Id;";

        using var command = CreateCommand(query);
        command.Parameters.AddWithValue("@Id", institution.Id);
        command.Parameters.AddWithValue("@Code", (object?)institution.Code ?? DBNull.Value);
        command.Parameters.AddWithValue("@Address", (object?)institution.Address ?? DBNull.Value);

        command.ExecuteNonQuery();
    }

    public void UpdateBusinessHour(Institution institution)
    {
        if (institution.BusinessHours == null || institution.BusinessHours.Count == 0)
            return;

        var hourTableName = institution.InstitutionType + "Hour";
        var fkName = institution.InstitutionType + "Id";

        var query = $@"
                UPDATE {hourTableName} 
                SET 
                    MonStart = @MonStart, MonEnd = @MonEnd, 
                    TuesStart = @TuesStart, TuesEnd = @TuesEnd, 
                    WedStart = @WedStart, WedEnd = @WedEnd, 
                    ThursStart = @ThursStart, ThursEnd = @ThursEnd, 
                    FriStart = @FriStart, FriEnd = @FriEnd, 
                    SatStart = @SatStart, SatEnd = @SatEnd, 
                    SunStart = @SunStart, SunEnd = @SunEnd 
                WHERE {fkName} = @FK;";

        using var command = CreateCommand(query);
        command.Parameters.AddWithValue("@FK", institution.Id);
        foreach (var businessHour in institution.BusinessHours)
        {
            var prefix = businessHour.DayOfWeek.GetDayPrefix();
            command.Parameters.AddWithValue($"@{prefix}Start",
                (object?)businessHour.StartHour ?? DBNull.Value);
            command.Parameters.AddWithValue($"@{prefix}End", (object?)businessHour.EndHour ?? DBNull.Value);
        }

        command.ExecuteNonQuery();
    }

    private SqlCommand CreateCommand(string query)
    {
        return new SqlCommand(query, TransactionHolder.Connection.Value, TransactionHolder.Transaction.Value);
    }
}