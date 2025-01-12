using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HourDataProcessor;

// Config 클래스를 static 필드로 홀딩해주는 클래스입니다.  
// ConfigHolder를 이용해서 전역으로 앱 설정 변수에 접근할 수 있습니다.
public class ConfigHolder
{
    public static readonly Config AppConfig;

    static ConfigHolder()
    {
        const string ymlPath = "resources/appsettings.yml";

        if (!File.Exists(ymlPath))
        {
            throw new FileNotFoundException("YML NOT FOUND");
        }
        
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        AppConfig = deserializer.Deserialize<Config>(File.ReadAllText(ymlPath));
    }
}

// 설정 변수를 가지는 클래스 입니다.
// Todo: immutable 객체로 설정하면 더욱 좋겠지만 YamlDotNet을 간편하게 사용하기 위해 일반 클래스로 설정하였습니다.
public class Config
{
    public DatabaseConfig? Database { get; set; }
}

public class DatabaseConfig
{
    public string? InitConnectionString { get; set; }
    public string? ConnectionString { get; set; }
};