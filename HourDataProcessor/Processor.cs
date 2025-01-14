using HourDataProcessor.Db;
using HourDataProcessor.Entity;
using HourDataProcessor.OpenData.Csv;
using HourDataProcessor.utils;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace HourDataProcessor;

public class Processor
{
    private readonly InstitutionDao _institutionDao;
    private readonly IEntityReader _entityReader;

    public Processor()
    {
        _institutionDao = new InstitutionDao();
        _entityReader = new EntityReaderFromCsv();
    }

    public Processor(IEntityReader entityReader)
    {
        _institutionDao = new InstitutionDao();
        _entityReader = entityReader;
    }

    public void Run()
    {
        while (_entityReader.HasNext())
        {
            var institutions = _entityReader.Read();

            using var connection = new SqlConnection(ConfigHolder.AppConfig.Database.ConnectionString);
            TransactionHolder.Connection.Value = connection;
            connection.Open();

            for (int i = 0; i < institutions.Count; i += 100)
            {
                using var transaction = connection.BeginTransaction();
                TransactionHolder.Transaction.Value = transaction;
                Console.Out.WriteLine($"{i}개 수행 완료");
                for (int j = i; j < i + 100 && j < institutions.Count; j++)
                {
                    HandleRow(institutions[j]);
                }

                transaction.Commit();
            }
        }
    }

    private void HandleRow(Institution institution)
    {
        if (institution.InstitutionType != InstitutionType.Hospital &&
            institution.InstitutionType != InstitutionType.DrugStore &&
            institution.InstitutionType != InstitutionType.Unknown)
        {
            return;
        }

        var originInstitutions = _institutionDao.FindByNameAndLocation(institution);

        if (originInstitutions.IsNullOrEmpty())
        {
            _institutionDao.Save(institution);
        }
        else
        {
            HandleRowWhenFoundExistingInstitutions(institution, originInstitutions);
        }
    }

    private void HandleRowWhenFoundExistingInstitutions(Institution institution, List<Institution> originInstitutions)
    {
        Institution? originInstitution = null;
        try
        {
            originInstitution = FindInstitutionsByUniqueKey(institution, originInstitutions);
            if (originInstitution == null)
            {
                originInstitution = FilterDissimilarInstitutions(institution, originInstitutions);
            }
            
        }
        catch (ApplicationException e)
        {
            LoggerHelper.LogError(e.Message);
            return;
        }

        if (originInstitution == null)
        {
            _institutionDao.Save(institution);
            return;
        }

        if (institution.InstitutionType == InstitutionType.Unknown)
            institution.InstitutionType = originInstitution.InstitutionType;

        if (originInstitution.DirtyCheck(institution)) return;

        // 병원/약국 자체 정보에 업데이트 내역이 있으면 업데이트 한다.
        if (!institution.DirtyCheck(originInstitution))
        {
            institution.CombineWithOriginal(originInstitution);
            _institutionDao.Update(institution);
        }

        // 원본 데이터에 영업시간 데이터가 존재하지 않고, 새로 불러온 데이터에 영업시간 데이터가 존재하면 영업시간 정보를 저장한다.
        if (originInstitution.BusinessHours == null && institution.BusinessHours != null)
        {
            _institutionDao.SaveBusinessHours(institution);
        }

        // 영업 시간 정보가 달라졌다면 영업시간 정보를 업데이트 한다.
        else if (originInstitution.BusinessHours != null
                 && institution.BusinessHours != null
                 && !institution.BusinessHours.SequenceEqual(originInstitution.BusinessHours))
        {
            _institutionDao.UpdateBusinessHour(institution);
        }
    }
    
    private static Institution? FindInstitutionsByUniqueKey(Institution newInstitution, List<Institution> institutions)
    {
        var uniqueKeyEquals = institutions
            .Where(i => i.EqualUniqueCode(newInstitution))
            .ToList();

        if (uniqueKeyEquals.Count == 1)
        {
            return institutions.Single();
        }

        if (uniqueKeyEquals.Count > 1)
        {
            throw new ApplicationException($"Unique key가 중복인 레코드 {institutions.Count} 이상 발견 \n" +
                                           string.Join("\n",
                                               institutions.Select(x =>
                                                   $"Id: {x.Id}, Name: {x.Name}, Address: {x.Address ?? "주소 없음"}")));
        }

        return null;
    }

    private Institution? FilterDissimilarInstitutions(Institution newInstitution, List<Institution> institutions)
    {
        var filteredInstitution = institutions
            .Where(i => i.EqualTo(newInstitution))
            .Where(i => i.IsAddressSimilarTo(newInstitution))
            .ToList();

        if (filteredInstitution.IsNullOrEmpty()) return null;
        if (filteredInstitution.Count > 1)
        {
            throw new ApplicationException($"유사한 컬럼이 {institutions.Count}개가 발견되었습니다.\n" +
                                           string.Join("\n",
                                               institutions.Select(x =>
                                                   $"Id: {x.Id}, Name: {x.Name}, Address: {x.Address ?? "주소 없음"}")));
        }

        return filteredInstitution.Single();
    }
}