using HourDataProcessor.Entity;

namespace HourDataProcessor;

public interface ICsvExplorer
{
    CsvInfo GetNextCsvInfo();
    bool HasNext();
}


//TODO: 임시 구현. 다른 방식 채용 필요
public class YmlCsvExplorer : ICsvExplorer
{
    private bool read = false;
    public CsvInfo GetNextCsvInfo()
    {
        return new CsvInfo()
        {
            Path = "/Users/choisemin/workspace/0-job-search/test/GradeHealthCare/hour-datas/hospital-1.csv",
            InstitutionType = InstitutionType.Hospital
        };
    }

    public bool HasNext()
    {
        if (read) return false;
        read = true;
        return true;
    }
}

public struct CsvInfo
{
    public string Path { get; init; }
    public InstitutionType InstitutionType { get; init; }
}

