using AutoMapperTest.Core.Data;
using AutoMapperTest.Core.Domain;

namespace AutoMapperTest.Core.Mapping;

public class MappingProfile : TimeZoneAwareProfile
{
    public MappingProfile()
    {
        // Data (Central) -> Domain (UTC)
        CreateMap<PersonDto, Person>();
        CreateMap<AppointmentDto, Appointment>();

        // Domain (UTC) -> Data (Central)
        CreateMap<Person, PersonDto>();
        CreateMap<Appointment, AppointmentDto>();
    }
}
