using AutoMapper;
using Order.Controllers.EntitiesControllers;
using Order.Models;
using Order.Models.DTO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Order.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            //CreateMap<EventDto, Event>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EventDto, Event>().ForAllMembers(opts =>
                opts.Condition((src, dest, srcMember) =>
                    srcMember != null &&
                    (!(srcMember is IEnumerable<int> list) || list.Any())));

            CreateMap<ProjectDto, Project>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
            CreateMap<TaskDto, Models.Task>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<ContextDto, Context>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        }
    }
}
