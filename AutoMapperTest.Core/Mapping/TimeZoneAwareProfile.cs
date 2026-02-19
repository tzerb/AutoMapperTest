using System.Reflection;
using AutoMapper;

namespace AutoMapperTest.Core.Mapping;

/// <summary>
/// Base AutoMapper profile that automatically converts DateTime properties between
/// Central Time (data layer) and UTC (domain layer) during mapping.
///
/// Any profile that inherits from this class gets the conversion for free on every
/// CreateMap call — no need to manually chain AfterMap on each one.
///
/// Convention: types in a ".Data" namespace are Central Time, types in a ".Domain"
/// namespace are UTC. Properties listed in PassthroughProperties are left unchanged.
///
/// DST handling: TimeZoneInfo.ConvertTimeToUtc/ConvertTimeFromUtc automatically
/// account for Daylight Saving Time. The "Central Standard Time" zone ID covers
/// both CST (UTC-6) and CDT (UTC-5). By forcing DateTimeKind.Unspecified before
/// converting, we let TimeZoneInfo determine the correct offset based on the date.
/// During the spring-forward gap (e.g. 2:00-3:00 AM), .NET throws an
/// InvalidTimeZoneException for truly ambiguous times. During the fall-back overlap,
/// it defaults to the standard time offset.
/// </summary>
public abstract class TimeZoneAwareProfile : Profile
{
    private static readonly TimeZoneInfo CentralTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

    /// <summary>
    /// DateTime property names that should pass through without conversion.
    /// </summary>
    protected virtual HashSet<string> PassthroughProperties { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "AppointmentTime"
    };

    /// <summary>
    /// Shadows Profile.CreateMap to automatically apply Central/UTC DateTime
    /// conversion based on source and destination namespace conventions.
    /// </summary>
    public new IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
    {
        return base.CreateMap<TSource, TDestination>()
            .AfterMap((src, dest) => ConvertDateTimes(typeof(TSource), typeof(TDestination), dest!));
    }

    /// <summary>
    /// Shadows Profile.CreateMap (MemberList overload) to automatically apply
    /// Central/UTC DateTime conversion.
    /// </summary>
    public new IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList)
    {
        return base.CreateMap<TSource, TDestination>(memberList)
            .AfterMap((src, dest) => ConvertDateTimes(typeof(TSource), typeof(TDestination), dest!));
    }

    private void ConvertDateTimes(Type sourceType, Type destType, object destination)
    {
        bool isDataToDomain = IsDataType(sourceType) && IsDomainType(destType);
        bool isDomainToData = IsDomainType(sourceType) && IsDataType(destType);

        if (!isDataToDomain && !isDomainToData)
            return;

        foreach (var prop in destType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.PropertyType != typeof(DateTime) || !prop.CanRead || !prop.CanWrite)
                continue;

            if (PassthroughProperties.Contains(prop.Name))
                continue;

            var value = (DateTime)prop.GetValue(destination)!;

            var converted = isDataToDomain
                ? TimeZoneInfo.ConvertTimeToUtc(
                    DateTime.SpecifyKind(value, DateTimeKind.Unspecified), CentralTimeZone)
                : TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.SpecifyKind(value, DateTimeKind.Utc), CentralTimeZone);

            prop.SetValue(destination, converted);
        }
    }

    private static bool IsDataType(Type type) => type.Namespace?.Contains(".Data") == true;
    private static bool IsDomainType(Type type) => type.Namespace?.Contains(".Domain") == true;
}
