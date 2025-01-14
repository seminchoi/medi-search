using HourDataProcessor.Entity;
using Microsoft.IdentityModel.Tokens;

namespace HourDataProcessor.OpenData.Csv;

public class CsvExplorer
{
    private const string FilePostfix = ".csv";
    private Queue<(InstitutionType, string)> FilePaths { get; } = new();

    /// <summary>
    /// CsvSource 설정 값으로부터 CsvFilePath를 불러옵니다.
    /// </summary>
    /// <exception cref="FileNotFoundException">
    /// 설정한 값으로 파일을 찾을 수 없는 경우
    /// </exception>
    // TODO: 환경변수로 파일 경로를 받을 수 있게 추가 구현, 환경 변수가 있는 경우 Config를 덮어쓰도록 구현 
    public CsvExplorer()
    {
        var csvSource = ConfigHolder.AppConfig.CsvSource;
        var csvSourceBasePath = csvSource.BasePath;

        var rootPath = AppContext.BaseDirectory;

        var basePath = Path.Combine(rootPath, csvSourceBasePath);

        foreach (var fileName in csvSource.FileNames)
        {
            var type = InstitutionType.Unknown;
            var originFileName = fileName;
            
            if (fileName.StartsWith("DS/"))
            {
                type = InstitutionType.DrugStore;
                originFileName = fileName.Substring(3);
            }

            var filePath = Path.Combine(basePath, originFileName + FilePostfix);
            if (File.Exists(filePath))
            {
                FilePaths.Enqueue((type, filePath));
            }
            else throw new FileNotFoundException("CsvSource File not found", filePath);
        }
    }

    public (InstitutionType type, string filePath) GetCsvFilePath()
    {
        return FilePaths.Dequeue();
    }

    public bool HasNext()
    {
        return !FilePaths.IsNullOrEmpty();
    }
}