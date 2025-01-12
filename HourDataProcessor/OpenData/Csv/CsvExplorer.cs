using Microsoft.IdentityModel.Tokens;

namespace HourDataProcessor.OpenData.Csv;

public class CsvExplorer
{
    private const string FilePostfix = ".csv";
    private Queue<string> FilePaths { get; } = new(); 

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
        
        var basePath = csvSourceBasePath != null ? Path.Combine(rootPath, csvSourceBasePath) : rootPath;
        
        foreach (var fileName in csvSource.FileNames)
        {
            var filePath = Path.Combine(basePath, fileName + FilePostfix);
            if (File.Exists(filePath))
            {
                FilePaths.Enqueue(filePath);
            }
            else throw new FileNotFoundException("CsvSource File not found", filePath);
        }
    }
    public string GetCsvFilePath()
    {
        return FilePaths.Dequeue();
    }

    public bool HasNext()
    {
        return !FilePaths.IsNullOrEmpty();
    }
}