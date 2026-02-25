using AutoMapper;
using AutoMapperTest.Core.Data;
using AutoMapperTest.Core.Domain;
using AutoMapperTest.Core.Mapping;
using AutoMapperTest.Core.Services;

namespace AutoMapperTest.Tests;

public class MappingTests
{
    private static readonly TimeZoneInfo CentralTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

    private readonly IMapper _mapper;

    public MappingTests()
    {
        var myService = new MyService();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        config.AssertConfigurationIsValid();
        _mapper = new Mapper(config, type =>
        {
            if (type == typeof(FullNameResolver))
                return new FullNameResolver(myService);
            return null!;
        });
    }

    [Fact]
    public void Configuration_IsValid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        config.AssertConfigurationIsValid();
    }

    // ── Person: Data (Central) -> Domain (UTC) ──────────────────────────

    [Fact]
    public void PersonDto_To_Person_Converts_DateOfBirth_CentralToUtc()
    {
        // Central Standard Time is UTC-6, Central Daylight Time is UTC-5.
        // January 15 is standard time (UTC-6).
        var centralTime = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Unspecified);
        var expectedUtc = TimeZoneInfo.ConvertTimeToUtc(centralTime, CentralTimeZone);

        var dto = new PersonDto { DateOfBirth = centralTime };

        var person = _mapper.Map<Person>(dto);

        Assert.Equal(expectedUtc, person.DateOfBirth);
        Assert.Equal(centralTime.AddHours(6), person.DateOfBirth); // CST is UTC-6
    }

    [Fact]
    public void PersonDto_To_Person_Converts_CreatedDate_CentralToUtc()
    {
        var centralTime = new DateTime(2024, 3, 15, 14, 30, 0, DateTimeKind.Unspecified);
        var expectedUtc = TimeZoneInfo.ConvertTimeToUtc(centralTime, CentralTimeZone);

        var dto = new PersonDto { CreatedDate = centralTime };

        var person = _mapper.Map<Person>(dto);

        Assert.Equal(expectedUtc, person.CreatedDate);
    }

    [Fact]
    public void PersonDto_To_Person_Converts_LastModifiedDate_CentralToUtc()
    {
        var centralTime = new DateTime(2024, 7, 4, 12, 0, 0, DateTimeKind.Unspecified);
        var expectedUtc = TimeZoneInfo.ConvertTimeToUtc(centralTime, CentralTimeZone);

        var dto = new PersonDto { LastModifiedDate = centralTime };

        var person = _mapper.Map<Person>(dto);

        Assert.Equal(expectedUtc, person.LastModifiedDate);
        Assert.Equal(centralTime.AddHours(5), person.LastModifiedDate); // CDT is UTC-5
    }

    // ── Person: Domain (UTC) -> Data (Central) ─────────────────────────

    [Fact]
    public void Person_To_PersonDto_Converts_DateOfBirth_UtcToCentral()
    {
        var utcTime = new DateTime(2024, 1, 15, 16, 0, 0, DateTimeKind.Utc);
        var expectedCentral = TimeZoneInfo.ConvertTimeFromUtc(utcTime, CentralTimeZone);

        var person = new Person { DateOfBirth = utcTime };

        var dto = _mapper.Map<PersonDto>(person);

        Assert.Equal(expectedCentral, dto.DateOfBirth);
        Assert.Equal(utcTime.AddHours(-6), dto.DateOfBirth); // CST is UTC-6
    }

    [Fact]
    public void Person_To_PersonDto_Converts_CreatedDate_UtcToCentral()
    {
        var utcTime = new DateTime(2024, 6, 20, 18, 0, 0, DateTimeKind.Utc);
        var expectedCentral = TimeZoneInfo.ConvertTimeFromUtc(utcTime, CentralTimeZone);

        var person = new Person { CreatedDate = utcTime };

        var dto = _mapper.Map<PersonDto>(person);

        Assert.Equal(expectedCentral, dto.CreatedDate);
        Assert.Equal(utcTime.AddHours(-5), dto.CreatedDate); // CDT is UTC-5
    }

    // ── Appointment: Data (Central) -> Domain (UTC) ─────────────────────

    [Fact]
    public void AppointmentDto_To_Appointment_Converts_CreatedDate_CentralToUtc()
    {
        var centralTime = new DateTime(2024, 2, 10, 9, 0, 0, DateTimeKind.Unspecified);
        var expectedUtc = TimeZoneInfo.ConvertTimeToUtc(centralTime, CentralTimeZone);

        var dto = new AppointmentDto { CreatedDate = centralTime };

        var appointment = _mapper.Map<Appointment>(dto);

        Assert.Equal(expectedUtc, appointment.CreatedDate);
    }

    [Fact]
    public void AppointmentDto_To_Appointment_Converts_ReminderDate_CentralToUtc()
    {
        var centralTime = new DateTime(2024, 8, 1, 15, 30, 0, DateTimeKind.Unspecified);
        var expectedUtc = TimeZoneInfo.ConvertTimeToUtc(centralTime, CentralTimeZone);

        var dto = new AppointmentDto { ReminderDate = centralTime };

        var appointment = _mapper.Map<Appointment>(dto);

        Assert.Equal(expectedUtc, appointment.ReminderDate);
    }

    [Fact]
    public void AppointmentDto_To_Appointment_AppointmentTime_PassesThrough_Unchanged()
    {
        var originalTime = new DateTime(2024, 5, 20, 14, 0, 0, DateTimeKind.Unspecified);

        var dto = new AppointmentDto { AppointmentTime = originalTime };

        var appointment = _mapper.Map<Appointment>(dto);

        Assert.Equal(originalTime, appointment.AppointmentTime);
    }

    // ── Appointment: Domain (UTC) -> Data (Central) ─────────────────────

    [Fact]
    public void Appointment_To_AppointmentDto_Converts_CreatedDate_UtcToCentral()
    {
        var utcTime = new DateTime(2024, 2, 10, 15, 0, 0, DateTimeKind.Utc);
        var expectedCentral = TimeZoneInfo.ConvertTimeFromUtc(utcTime, CentralTimeZone);

        var appointment = new Appointment { CreatedDate = utcTime };

        var dto = _mapper.Map<AppointmentDto>(appointment);

        Assert.Equal(expectedCentral, dto.CreatedDate);
    }

    [Fact]
    public void Appointment_To_AppointmentDto_Converts_ReminderDate_UtcToCentral()
    {
        var utcTime = new DateTime(2024, 8, 1, 20, 30, 0, DateTimeKind.Utc);
        var expectedCentral = TimeZoneInfo.ConvertTimeFromUtc(utcTime, CentralTimeZone);

        var appointment = new Appointment { ReminderDate = utcTime };

        var dto = _mapper.Map<AppointmentDto>(appointment);

        Assert.Equal(expectedCentral, dto.ReminderDate);
    }

    [Fact]
    public void Appointment_To_AppointmentDto_AppointmentTime_PassesThrough_Unchanged()
    {
        var originalTime = new DateTime(2024, 5, 20, 14, 0, 0, DateTimeKind.Unspecified);

        var appointment = new Appointment { AppointmentTime = originalTime };

        var dto = _mapper.Map<AppointmentDto>(appointment);

        Assert.Equal(originalTime, dto.AppointmentTime);
    }

    // ── Round-trip tests ────────────────────────────────────────────────

    [Fact]
    public void Person_RoundTrip_DtoToDomainAndBack_PreservesValues()
    {
        var originalCentralTime = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Unspecified);

        var dto = new PersonDto
        {
            Id = 42,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = originalCentralTime,
            CreatedDate = new DateTime(2024, 3, 1, 8, 0, 0, DateTimeKind.Unspecified),
            LastModifiedDate = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Unspecified)
        };

        // Dto -> Domain (Central -> UTC)
        var person = _mapper.Map<Person>(dto);

        // Domain -> Dto (UTC -> Central)
        var roundTrippedDto = _mapper.Map<PersonDto>(person);

        Assert.Equal(dto.Id, roundTrippedDto.Id);
        Assert.Equal(dto.FirstName, roundTrippedDto.FirstName);
        Assert.Equal(dto.LastName, roundTrippedDto.LastName);
        Assert.Equal(dto.DateOfBirth, roundTrippedDto.DateOfBirth);
        Assert.Equal(dto.CreatedDate, roundTrippedDto.CreatedDate);
        Assert.Equal(dto.LastModifiedDate, roundTrippedDto.LastModifiedDate);
    }

    [Fact]
    public void Appointment_RoundTrip_DtoToDomainAndBack_PreservesValues()
    {
        var dto = new AppointmentDto
        {
            Id = 99,
            PersonId = 42,
            Description = "Annual checkup",
            AppointmentTime = new DateTime(2024, 5, 20, 14, 0, 0, DateTimeKind.Unspecified),
            CreatedDate = new DateTime(2024, 4, 1, 9, 0, 0, DateTimeKind.Unspecified),
            ReminderDate = new DateTime(2024, 5, 19, 14, 0, 0, DateTimeKind.Unspecified)
        };

        var appointment = _mapper.Map<Appointment>(dto);
        var roundTrippedDto = _mapper.Map<AppointmentDto>(appointment);

        Assert.Equal(dto.Id, roundTrippedDto.Id);
        Assert.Equal(dto.PersonId, roundTrippedDto.PersonId);
        Assert.Equal(dto.Description, roundTrippedDto.Description);
        Assert.Equal(dto.AppointmentTime, roundTrippedDto.AppointmentTime);
        Assert.Equal(dto.CreatedDate, roundTrippedDto.CreatedDate);
        Assert.Equal(dto.ReminderDate, roundTrippedDto.ReminderDate);
    }

    [Fact]
    public void Person_RoundTrip_DomainToDtoAndBack_PreservesValues()
    {
        var person = new Person
        {
            Id = 7,
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = new DateTime(1990, 6, 15, 16, 0, 0, DateTimeKind.Utc),
            CreatedDate = new DateTime(2024, 1, 1, 6, 0, 0, DateTimeKind.Utc),
            LastModifiedDate = new DateTime(2024, 7, 1, 18, 0, 0, DateTimeKind.Utc)
        };

        var dto = _mapper.Map<PersonDto>(person);
        var roundTrippedPerson = _mapper.Map<Person>(dto);

        Assert.Equal(person.Id, roundTrippedPerson.Id);
        Assert.Equal(person.FirstName, roundTrippedPerson.FirstName);
        Assert.Equal(person.LastName, roundTrippedPerson.LastName);
        Assert.Equal(person.DateOfBirth, roundTrippedPerson.DateOfBirth);
        Assert.Equal(person.CreatedDate, roundTrippedPerson.CreatedDate);
        Assert.Equal(person.LastModifiedDate, roundTrippedPerson.LastModifiedDate);
    }

    [Fact]
    public void Appointment_RoundTrip_DomainToDtoAndBack_PreservesValues()
    {
        var appointment = new Appointment
        {
            Id = 1,
            PersonId = 7,
            Description = "Follow-up",
            AppointmentTime = new DateTime(2024, 9, 10, 13, 0, 0, DateTimeKind.Unspecified),
            CreatedDate = new DateTime(2024, 8, 1, 20, 0, 0, DateTimeKind.Utc),
            ReminderDate = new DateTime(2024, 9, 9, 20, 0, 0, DateTimeKind.Utc)
        };

        var dto = _mapper.Map<AppointmentDto>(appointment);
        var roundTrippedAppointment = _mapper.Map<Appointment>(dto);

        roundTrippedAppointment.ShouldBeEquivalentExcludingConvertedDates(appointment);
    }

    // ── DST edge case tests ─────────────────────────────────────────────

    [Fact]
    public void PersonDto_To_Person_Handles_DaylightSavingTime_SpringForward()
    {
        // March 10, 2024 at 2:30 AM Central - this is during the "spring forward" gap.
        // Clocks jump from 2:00 AM to 3:00 AM, so 2:30 AM CST doesn't technically exist.
        // ConvertTimeToUtc should still handle it (it treats it as standard time offset).
        var centralTime = new DateTime(2024, 3, 10, 1, 30, 0, DateTimeKind.Unspecified);
        var expectedUtc = TimeZoneInfo.ConvertTimeToUtc(centralTime, CentralTimeZone);

        var dto = new PersonDto { CreatedDate = centralTime };

        var person = _mapper.Map<Person>(dto);

        Assert.Equal(expectedUtc, person.CreatedDate);
    }

    [Fact]
    public void PersonDto_To_Person_Handles_DaylightSavingTime_FallBack()
    {
        // November 3, 2024 at 1:30 AM Central - this is during the "fall back" overlap.
        var centralTime = new DateTime(2024, 11, 3, 1, 30, 0, DateTimeKind.Unspecified);
        var expectedUtc = TimeZoneInfo.ConvertTimeToUtc(centralTime, CentralTimeZone);

        var dto = new PersonDto { CreatedDate = centralTime };

        var person = _mapper.Map<Person>(dto);

        Assert.Equal(expectedUtc, person.CreatedDate);
    }

    // ── Verifying offsets for standard vs daylight time ──────────────────

    [Fact]
    public void PersonDto_To_Person_Uses_Correct_StandardTime_Offset()
    {
        // December is Central Standard Time (UTC-6)
        var centralTime = new DateTime(2024, 12, 25, 12, 0, 0, DateTimeKind.Unspecified);

        var dto = new PersonDto { DateOfBirth = centralTime };
        var person = _mapper.Map<Person>(dto);

        // 12:00 PM CST = 6:00 PM UTC
        Assert.Equal(new DateTime(2024, 12, 25, 18, 0, 0), person.DateOfBirth);
    }

    [Fact]
    public void PersonDto_To_Person_Uses_Correct_DaylightTime_Offset()
    {
        // July is Central Daylight Time (UTC-5)
        var centralTime = new DateTime(2024, 7, 4, 12, 0, 0, DateTimeKind.Unspecified);

        var dto = new PersonDto { DateOfBirth = centralTime };
        var person = _mapper.Map<Person>(dto);

        // 12:00 PM CDT = 5:00 PM UTC
        Assert.Equal(new DateTime(2024, 7, 4, 17, 0, 0), person.DateOfBirth);
    }

    // ── Non-date properties are unaffected ───────────────────────────────

    [Fact]
    public void PersonDto_To_Person_Maps_NonDateProperties_Correctly()
    {
        var dto = new PersonDto
        {
            Id = 123,
            FirstName = "Alice",
            LastName = "Johnson"
        };

        var person = _mapper.Map<Person>(dto);

        Assert.Equal(123, person.Id);
        Assert.Equal("Alice", person.FirstName);
        Assert.Equal("Johnson", person.LastName);
    }

    // ── DI-based mapping via IValueResolver ──────────────────────────────

    [Fact]
    public void PersonDto_To_Person_Resolves_FullName_Via_MyService()
    {
        var dto = new PersonDto
        {
            FirstName = "Alice",
            LastName = "Johnson"
        };

        var person = _mapper.Map<Person>(dto);

        Assert.Equal("Johnson, Alice", person.FullName);
    }

    [Fact]
    public void AppointmentDto_To_Appointment_Maps_NonDateProperties_Correctly()
    {
        var dto = new AppointmentDto
        {
            Id = 456,
            PersonId = 123,
            Description = "Dental cleaning"
        };

        var appointment = _mapper.Map<Appointment>(dto);

        Assert.Equal(456, appointment.Id);
        Assert.Equal(123, appointment.PersonId);
        Assert.Equal("Dental cleaning", appointment.Description);
    }
}
