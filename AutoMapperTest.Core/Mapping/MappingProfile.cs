using System.Reflection;
using AutoMapper;
using AutoMapperTest.Core.Data;
using AutoMapperTest.Core.Domain;

namespace AutoMapperTest.Core.Mapping;

public class MappingProfile : Profile
{
    private static readonly TimeZoneInfo CentralTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

    private static readonly HashSet<string> PassthroughProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "AppointmentTime"
    };

    public MappingProfile()
    {
        // Data (Central) -> Domain (UTC)
        CreateMap<PersonDto, Person>()
            .AfterMap(ConvertDateTimesCentralToUtc);

        CreateMap<AppointmentDto, Appointment>()
            .AfterMap(ConvertDateTimesCentralToUtc);

        // Domain (UTC) -> Data (Central)
        CreateMap<Person, PersonDto>()
            .AfterMap(ConvertDateTimesUtcToCentral);

        CreateMap<Appointment, AppointmentDto>()
            .AfterMap(ConvertDateTimesUtcToCentral);
    }

    // DST handling: TimeZoneInfo.ConvertTimeToUtc/ConvertTimeFromUtc automatically
    // account for Daylight Saving Time. The "Central Standard Time" zone ID covers
    // both CST (UTC-6) and CDT (UTC-5). By forcing DateTimeKind.Unspecified before
    // converting, we let TimeZoneInfo determine the correct offset based on the date.
    // During the spring-forward gap (e.g. 2:00-3:00 AM), .NET throws an
    // InvalidTimeZoneException for truly ambiguous times. During the fall-back overlap,
    // it defaults to the standard time offset.

    private static void ConvertDateTimesCentralToUtc(object source, object destination)
    {
        foreach (var prop in destination.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.PropertyType != typeof(DateTime) || !prop.CanRead || !prop.CanWrite)
                continue;

            if (PassthroughProperties.Contains(prop.Name))
                continue;

            var value = (DateTime)prop.GetValue(destination)!;
            var converted = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(value, DateTimeKind.Unspecified),
                CentralTimeZone);
            prop.SetValue(destination, converted);
        }
    }

    private static void ConvertDateTimesUtcToCentral(object source, object destination)
    {
        foreach (var prop in destination.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.PropertyType != typeof(DateTime) || !prop.CanRead || !prop.CanWrite)
                continue;

            if (PassthroughProperties.Contains(prop.Name))
                continue;

            var value = (DateTime)prop.GetValue(destination)!;
            var converted = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(value, DateTimeKind.Utc),
                CentralTimeZone);
            prop.SetValue(destination, converted);
        }
    }
}
