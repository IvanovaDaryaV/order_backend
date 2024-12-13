using AutoMapper;
using Order.Controllers.EntitiesControllers;
using Order.Models;
using Order.Models.DTO;

namespace Order.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<EventDto, Event>();
            CreateMap<ProjectDto, Project>();
            CreateMap<TaskDto, Models.Task>();
            CreateMap<ContextDto, Context>();
        }
    }
}
