using CsvHelper;
using CsvHelper.Configuration;
using HourDataProcessor.Entity;
using Microsoft.IdentityModel.Tokens;

namespace HourDataProcessor.OpenData.Csv;

public sealed class InstitutionMapper : ClassMap<Institution>
{
    public InstitutionMapper()
    {
        Map(m => m.Code).Name("암호화요양기호");
        Map(m => m.Address).Name("주소");
        Map(m => m.Name).Name("요양기관명");
        Map(m => m.InstitutionType).Convert(args =>
        {
            var kindCodeName = args.Row.GetField("종별코드명");
            return InstitutionTypeHelper.ValueOf(kindCodeName);
        });
        Map(m => m.Longitude).Name("좌표(X)");
        Map(m => m.Latitude).Name("좌표(Y)");
        Map(m => m.BusinessHours).Convert(ConvertToBusinessHours);
    }

    private List<BusinessHours> ConvertToBusinessHours(ConvertFromStringArgs args)
    {
        var businessHours = new List<BusinessHours>();

        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            var startField = $"{day}_start";
            var endField = $"{day}_end";

            var startHour = args.Row.GetField(startField);
            var endHour = args.Row.GetField(endField);

            if (startField.IsNullOrEmpty() || endField.IsNullOrEmpty())
            {
                continue;
            }
            
            businessHours.Add(new BusinessHours
            {
                DayOfWeek = day,
                StartHour = startHour,
                EndHour = endHour
            });
        }

        return businessHours;
    }
}