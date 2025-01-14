using System.Reflection;
using HourDataProcessor.Db;
using HourDataProcessor.Entity;
using HourDataProcessor.OpenData.Csv;
using Microsoft.Data.SqlClient;
using Moq;

namespace HourDataProcessor.Tests;

[TestFixture]
public class ProcessorIntegrationTests : IDisposable
{
    private Mock<IEntityReader> _mockEntityReader;
    private SqlConnection _connection;
    private InstitutionDao _institutionDao;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        DbInitializer.Initialize();
    }

    [SetUp]
    public void Setup()
    {
        _institutionDao = new InstitutionDao();
        _mockEntityReader = new Mock<IEntityReader>();
        // 트랜잭션 설정
        _connection = new SqlConnection(ConfigHolder.AppConfig.Database.ConnectionString);
        _connection.Open();
    }

    [TearDown]
    public void TearDown()
    {
        using var command = new SqlCommand(
            @"DELETE FROM InstitutionHour;
                  DELETE FROM Institution;",
            _connection);
        command.ExecuteNonQuery();
        _connection?.Close();
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }

    [Test(Description = "이름과 전화번호가 같아도 주소가 다른 경우 서로 다른 데이터로 판단하여 모두 저장한다.")]
    public void ShouldSaveTwoDifferentInstitutionsWithSameNameAndPhoneButDiffrentAddress()
    {
        // Arrange
        var institutions = new List<Institution>
        {
            new()
            {
                Code = null,
                Name = "비움채한의원",
                Address = "서울특별시 강남구 선릉로107길 15, 3층 202호 (역삼동)",
                PhoneNumber = "02-554-8495",
                InstitutionType = InstitutionType.Hospital,
                Longitude = 127.0428335,
                Latitude = 37.50881007,
                BusinessHours = CreateBusinessHours("1000", "1930")
            },
            new()
            {
                Code = null,
                Name = "비움채한의원",
                Address = "서울특별시 강남구 도곡로 419, 4층 (대치동, 쇼핑넷빌딩)",
                PhoneNumber = "02-554-8495",
                InstitutionType = InstitutionType.Hospital,
                Longitude = 127.056316,
                Latitude = 37.49765537,
                BusinessHours = CreateBusinessHours("1000", "1930")
            }
        };

        RunProcessorWithMocking(institutions);

        // Assert
        using var command = new SqlCommand(
            "SELECT COUNT(*) FROM Institution",
            _connection);
        var count = (int)command.ExecuteScalar();

        Assert.That(count, Is.EqualTo(2));
    }


    [Test(Description = "좌표상 멀리 있더라도 같은 데이터로 판단 될 수 있다.")]
    public void ShouldSaveTwoDifferentInstitutionsWithSameNameAndPhone()
    {
        //좌표로 거리를 계산하면 1700m가 나오는 두 개의 데이터
        var institutions = new List<Institution>
        {
            new()
            {
                Code = "JDQ4MTg4MSM1MSMkMiMkNCMkMDAkNDgxMzUxIzUxIyQxIyQ1IyQ5OSQyNjEwMDIjNjEjJDEjJDgjJDgz",
                Name = "청담이든의원",
                Address = "서울특별시 강남구 테헤란로 226, 태왕빌딩 1층,2층 (역삼동)",
                PhoneNumber = "02-515-1131",
                InstitutionType = InstitutionType.Hospital,
                Longitude = 127.0411138,
                Latitude = 37.5017653,
                BusinessHours = null
            },
            new()
            {
                Code = null,
                Name = "청담이든의원",
                Address = "서울특별시 강남구 테헤란로 226, 태왕빌딩 1층,2층 (역삼동)",
                PhoneNumber = "02-515-1131",
                InstitutionType = InstitutionType.Hospital,
                Longitude = 127.0526234,
                Latitude = 37.51496547,
                BusinessHours = null
            }
        };

        RunProcessorWithMocking(institutions);

        // Assert
        using var command = new SqlCommand(
            "SELECT COUNT(*) FROM Institution",
            _connection);
        var count = (int)command.ExecuteScalar();

        Assert.That(count, Is.EqualTo(1));
    }

    [Test(Description = "Code가 같으면 같은 데이터로 간주한다.")]
    public void ShouldHandleDuplicateUniqueCode()
    {
        // Arrange
        var institutions = new List<Institution>
        {
            new()
            {
                Code = "JDQ4MTg4MSM1MSMkMiMkMCMkMDAkNDgxNzAyIzUxIyQxIyQ1IyQ5OSQzNjE0ODEjNTEjJDEjJDYjJDgz",
                Name = "연합의원",
                Address = null,
                PhoneNumber = null,
                InstitutionType = InstitutionType.Hospital,
                Longitude = 127.0230926,
                Latitude = 37.5483634,
                BusinessHours = null
            },
            new()
            {
                Code = "JDQ4MTg4MSM1MSMkMiMkMCMkMDAkNDgxNzAyIzUxIyQxIyQ1IyQ5OSQzNjE0ODEjNTEjJDEjJDYjJDgz",
                Name = "연합의원",
                Address = "서울특별시 성동구 독서당로 307-1, 2층 (금호동3가)",
                PhoneNumber = "02-2235-6991",
                InstitutionType = InstitutionType.Hospital,
                Longitude = 127.0230926,
                Latitude = 37.5483634,
                BusinessHours = null
            }
        };
        
        RunProcessorWithMocking(institutions);
        
        // Assert - 같은 트랜잭션 내에서 조회
        var findInstitutions = ReadInstitutionsFromDb();

        Assert.That(findInstitutions.Count(), Is.EqualTo(1));
        Assert.That(findInstitutions.Single().Address, Is.EqualTo("서울특별시 성동구 독서당로 307-1, 2층 (금호동3가)"));
        Assert.That(findInstitutions.Single().PhoneNumber, Is.EqualTo("02-2235-6991"));
    }
    
    [Test(Description = "같은 데이터라고 판단되면 두 개의 데이터를 병합한다. 상호적으로 null인 부분을 채워서 데이터를 병합한다.")]
    public void TestCombineInstitutions()
    {
        // Arrange
        var institutions = new List<Institution>
        {
            new()
            {
                Code = "JDQ4MTg4MSM1MSMkMiMkMCMkMDAkNDgxNzAyIzUxIyQxIyQ1IyQ5OSQzNjE0ODEjNTEjJDEjJDYjJDgz",
                Name = "연합의원",
                Address = null,
                PhoneNumber = null,
                InstitutionType = InstitutionType.Hospital,
                Longitude = 127.0230926,
                Latitude = 37.5483634,
                BusinessHours = CreateBusinessHours("0900", "1930")
            },
            new()
            {
                Code = "JDQ4MTg4MSM1MSMkMiMkMCMkMDAkNDgxNzAyIzUxIyQxIyQ1IyQ5OSQzNjE0ODEjNTEjJDEjJDYjJDgz",
                Name = "연합의원",
                Address = "서울특별시 성동구 독서당로 307-1, 2층 (금호동3가)",
                PhoneNumber = "02-2235-6991",
                InstitutionType = InstitutionType.Hospital,
                Longitude = 127.0230926,
                Latitude = 37.5483634,
                BusinessHours = null
            }
        };
        
        RunProcessorWithMocking(institutions);
        
        // Assert - 같은 트랜잭션 내에서 조회
        var findInstitutions = ReadInstitutionsFromDb();

        Assert.That(findInstitutions.Count(), Is.EqualTo(1));
        Assert.That(findInstitutions.Single().Address, Is.EqualTo("서울특별시 성동구 독서당로 307-1, 2층 (금호동3가)"));
        Assert.That(findInstitutions.Single().PhoneNumber, Is.EqualTo("02-2235-6991"));
        Assert.That(findInstitutions.Single().BusinessHours.Count(), Is.EqualTo(7));
    }
    
    [Test(Description = "Code가 달라도 같은 데이터로 판단될 수 있다.")]
    public void TestSameDataButDifferentCode()
    {
        // Arrange
        var institutions = new List<Institution>
        {
            new()
            {
                Code = "Code1",
                Name = "비움채한의원",
                Address = "서울특별시 강남구 선릉로107길 15, 3층 202호 (역삼동)",
                PhoneNumber = "02-554-8495",
                InstitutionType = InstitutionType.Hospital,
                Longitude = 127.0428335,
                Latitude = 37.50881007,
                BusinessHours = CreateBusinessHours("1000", "1930")
            },
            new()
            {
                Code = "Cod2",
                Name = "비움채한의원",
                Address = "서울특별시 강남구 선릉로107길 25",
                PhoneNumber = "02-554-8495",
                InstitutionType = InstitutionType.Hospital,
                Longitude = 127.0428335,
                Latitude = 37.50881007,
                BusinessHours = CreateBusinessHours("1000", "1930")
            }
        };
        
        RunProcessorWithMocking(institutions);

        // Assert
        using var command = new SqlCommand(
            "SELECT COUNT(*) FROM Institution",
            _connection);
        var count = (int)command.ExecuteScalar();

        Assert.That(count, Is.EqualTo(1));
    }
    
    [Test(Description = "SeoulCode가 같으면 같은 데이터로 판단한다.")]
    public void TestSameSeoulCode()
    {
        // Arrange
        var institutions = new List<Institution>
        {
            new()
            {
                Code = null,
                SeoulCode = "seoulCode",
                Name = "비움채한의원",
                Address = "서울특별시 강남구 선릉로107길 15, 3층 202호 (역삼동)",
                PhoneNumber = "02-554-8495",
                InstitutionType = InstitutionType.Hospital,
                Longitude = 127.0428335,
                Latitude = 37.50881007,
                BusinessHours = CreateBusinessHours("1000", "1930")
            },
            new()
            {
                Code = "Cod2",
                SeoulCode = "seoulCode",
                Name = "비움채한의원",
                Address = "서울특별시 강남구 선릉로107길 15, 3층 202호 (역삼동)",
                PhoneNumber = "02-554-8495",
                InstitutionType = InstitutionType.Hospital,
                Longitude = 127.0428335,
                Latitude = 37.50881007,
                BusinessHours = CreateBusinessHours("1000", "1930")
            }
        };
        
        RunProcessorWithMocking(institutions);

        // Assert
        using var command = new SqlCommand(
            "SELECT COUNT(*) FROM Institution",
            _connection);
        var count = (int)command.ExecuteScalar();

        Assert.That(count, Is.EqualTo(1));
    }

    private List<BusinessHour> CreateBusinessHours(string start, string end)
    {
        var hours = new List<BusinessHour>();
        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            if (day == DayOfWeek.Sunday) continue;
            hours.Add(new BusinessHour
            {
                DayOfWeek = day,
                StartHour = start,
                EndHour = end
            });
        }

        return hours;
    }
    
    private void RunProcessorWithMocking(List<Institution> institutions)
    {
        _mockEntityReader.SetupSequence(x => x.HasNext())
            .Returns(true)
            .Returns(false);

        _mockEntityReader.Setup(x => x.Read())
            .Returns(institutions);

        var processor = new Processor(_mockEntityReader.Object);

        // Act
        processor.Run();
    }


    private List<Institution> ReadInstitutionsFromDb()
    {
        using var command = new SqlCommand(
            @"SELECT i.Id, i.Code, i.SeoulCode, i.Name, i.Address, i.PhoneNumber, i.InstitutionType,
                 i.Location.STAsText() as LocationText,
                 h.Id as HourId,
                 h.MonStart, h.MonEnd,
                 h.TuesStart, h.TuesEnd,
                 h.WedStart, h.WedEnd,
                 h.ThursStart, h.ThursEnd,
                 h.FriStart, h.FriEnd,
                 h.SatStart, h.SatEnd,
                 h.SunStart, h.SunEnd
          FROM Institution i
          LEFT JOIN InstitutionHour h ON i.Id = h.InstitutionId",
            _connection);

        using var reader = command.ExecuteReader();
        return _institutionDao.CreateInstitutionFromReader(reader);
    }
}