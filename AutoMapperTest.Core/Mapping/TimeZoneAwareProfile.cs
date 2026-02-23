using System.Reflection;
using AutoMapper;
using FluentAssertions;

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
    /// Default set of DateTime property names that pass through without conversion.
    /// </summary>
    public static readonly HashSet<string> DefaultPassthroughProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "AppointmentTime"
    };

    /// <summary>
    /// DateTime property names that should pass through without conversion.
    /// Override in a derived profile to customize.
    /// </summary>
    protected virtual HashSet<string> PassthroughProperties => DefaultPassthroughProperties;

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
            if (!IsDateTimeProperty(prop) || !prop.CanRead || !prop.CanWrite)
                continue;

            if (PassthroughProperties.Contains(prop.Name))
                continue;

            var rawValue = prop.GetValue(destination);
            if (rawValue is not DateTime value)
                continue;

            var converted = isDataToDomain
                ? TimeZoneInfo.ConvertTimeToUtc(
                    DateTime.SpecifyKind(value, DateTimeKind.Unspecified), CentralTimeZone)
                : TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.SpecifyKind(value, DateTimeKind.Utc), CentralTimeZone);

            prop.SetValue(destination, converted);
        }
    }

    internal static bool IsDateTimeProperty(PropertyInfo prop) =>
        prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?);

    private static bool IsDataType(Type type) => type.Namespace?.Contains(".Data") == true;
    private static bool IsDomainType(Type type) => type.Namespace?.Contains(".Domain") == true;
}

public static class TimeZoneAwareAssertionExtensions
{
    /// <summary>
    /// Asserts equivalence while excluding DateTime properties that undergo
    /// timezone conversion (i.e. all DateTime properties except those in
    /// <see cref="TimeZoneAwareProfile.DefaultPassthroughProperties"/>).
    /// The actual and expected objects can be different types as long as they
    /// share the same property names.
    /// </summary>
    public static void ShouldBeEquivalentExcludingConvertedDates<TActual, TExpected>(this TActual actual, TExpected expected)
    {
        var convertedDateProperties = typeof(TActual)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Concat(typeof(TExpected).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .Where(p => TimeZoneAwareProfile.IsDateTimeProperty(p)
                        && !TimeZoneAwareProfile.DefaultPassthroughProperties.Contains(p.Name))
            .Select(p => p.Name)
            .ToHashSet();

        actual.Should().BeEquivalentTo(expected, options => options
            .Excluding(ctx => convertedDateProperties.Contains(ctx.Name)));
    }
}
