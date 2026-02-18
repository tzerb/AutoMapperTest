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
