using AutoMapper;
using MOE.Archive.Application.Categories.DTOs;
using MOE.Archive.Application.Documents.DTOs;
using MOE.Archive.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateCategoryRequestDto, Category>();
            CreateMap<Category, CategoryResponseDto>();

            CreateMap<UpdateCategoryRequestDto, Category>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));


            CreateMap<Document, DocumentResponseDto>()
                .ForMember(d => d.CreatedBy, opt => opt.MapFrom(s => s.CreatedBy ?? Guid.Empty));
        }
    }
}
