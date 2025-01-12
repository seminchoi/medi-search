using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using HourDataProcessor.Entity;

namespace HourDataProcessor.OpenData.Csv;

/// <summary>
/// csv 파일 단위로 엔티티 chunk를 읽습니다.
/// </summary>
/// <remarks>
/// - 병, 의원 csv 파일 하나의 크기가 아무리 커도 100MB를 넘지 않을 것이라 생각하여 OOM 문제가 없을 것이라 예상하고 파일 단위로 읽습니다.
/// </remarks>
public class EntityReaderFromCsv
{
    private readonly CsvConfiguration _configuration = new(CultureInfo.InvariantCulture)
    {
        MissingFieldFound = null, HeaderValidated = null
    };
    private readonly CsvExplorer _csvExplorer = new();

    public EntityReaderFromCsv()
    {
    }

    public EntityReaderFromCsv(CsvExplorer csvExplorer)
    {
        _csvExplorer = csvExplorer;
    }
    
    public EntityReaderFromCsv(CsvExplorer csvExplorer, CsvConfiguration csvConfiguration)
    {
        _csvExplorer = csvExplorer;
        _configuration = csvConfiguration;
    }
    
    public bool HasNext() => _csvExplorer.HasNext();

    public List<Institution> Read()
    {
        if(!HasNext()) throw new ApplicationException("There is no more file to read");
        var csvFilePath = _csvExplorer.GetCsvFilePath();

        using var reader = new StreamReader(csvFilePath);
        using var csv = new CsvReader(reader, _configuration);
        csv.Context.RegisterClassMap<InstitutionMapper>();
        return csv.GetRecords<Institution>().ToList();
    }
}