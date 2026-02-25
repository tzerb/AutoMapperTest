using AutoMapper;
using AutoMapperTest.Core.Data;
using AutoMapperTest.Core.Domain;
using AutoMapperTest.Core.Services;

namespace AutoMapperTest.Core.Mapping;

public class FullNameResolver : IValueResolver<PersonDto, Person, string>
{
    private readonly IMyService _myService;

    public FullNameResolver(IMyService myService)
    {
        _myService = myService;
    }

    public string Resolve(PersonDto source, Person destination, string destMember, ResolutionContext context)
    {
        return _myService.FormatFullName(source.FirstName, source.LastName);
    }
}
