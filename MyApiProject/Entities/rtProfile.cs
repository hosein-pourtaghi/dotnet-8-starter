using AutoMapper;
public class rtProfile : Profile
{
    public rtProfile()
    {
        CreateMap<rt, rt>()
            .ForMember(x => x.Id, opt => opt.MapFrom(src => src.Id));
        CreateMap<rt, rt>()
            .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name));
    }
}
