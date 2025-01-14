using CsvHelper;
using CsvHelper.Configuration;
using HourDataProcessor.Entity;
using HourDataProcessor.utils;
using Microsoft.IdentityModel.Tokens;

namespace HourDataProcessor.OpenData.Csv;

public sealed class InstitutionMapper : ClassMap<Institution>
{
    public InstitutionMapper()
    {
        Map(m => m.SeoulCode).Name("서울ID").Default(null);
        Map(m => m.Code).Name("암호화요양기호").Default(null);
        Map(m => m.Address).Name("주소");
        Map(m => m.Name).Name("요양기관명");
        Map(m => m.PhoneNumber).Name("전화번호").Default(null);
        Map(m => m.InstitutionType).Convert(args =>
        {
            var kindCodeName = args.Row.GetField("종별코드명");
            return InstitutionTypeHelper.ValueOf(kindCodeName);
        });
        Map(m => m.Longitude).Name("좌표(X)");
        Map(m => m.Latitude).Name("좌표(Y)");
        Map(m => m.BusinessHours).Convert(ConvertToBusinessHours);
    }

    private List<BusinessHour>? ConvertToBusinessHours(ConvertFromStringArgs args)
    {
        var businessHours = new List<BusinessHour>();

        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
        {
            var startField = $"{day.GetDayPrefix()}Start";
            var endField = $"{day.GetDayPrefix()}End";

            var startHour = args.Row.GetField(startField);
            var endHour = args.Row.GetField(endField);

            if (startHour.IsNullOrEmpty() || startHour.IsNullOrEmpty())
            {
                continue;
            }

            startHour = startHour.PadLeft(4, '0');
            endHour = endHour.PadLeft(4, '0');

            businessHours.Add(new BusinessHour
            {
                DayOfWeek = day,
                StartHour = startHour,
                EndHour = endHour
            });
        }

        return businessHours.IsNullOrEmpty() ? null : businessHours;
    }
}