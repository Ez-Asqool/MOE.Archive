using AutoMapper;
using MOE.Archive.Application.Categories.DTOs;
using MOE.Archive.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Categories.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IMapper mapper )
        {
            _categoryRepository = categoryRepository;   
            _unitOfWork = unitOfWork;
            _mapper = mapper;   
        }

        public async Task<CategoryResponseDto> CreateAsync(CreateCategoryRequestDto request, Guid? createdBy, CancellationToken cancellationToken = default)
        {

            //check if parent category exists if parentCategoryId is provided
            var parentCategoryId = request.ParentCategoryId;
            if (parentCategoryId.HasValue)
            {
                var parentCategory = await _categoryRepository.GetByIdAsync(parentCategoryId.Value, cancellationToken);
                if (parentCategory == null)
                {
                    throw new KeyNotFoundException("التصنيف الأب غير موجود.");
                }
            }
            // mapping request dto to category entity
            var categoryEntity = _mapper.Map<Domain.Entities.Category>(request);    
            categoryEntity.CreatedBy = createdBy;


            // saving category entity to database
            await _categoryRepository.AddAsync(categoryEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            //returning mapped response dto
            var categoryResponseDto = _mapper.Map<CategoryResponseDto>(categoryEntity); 
            return categoryResponseDto;
        }


        public async Task<IEnumerable<CategoryResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            //get all category entities from database
            var categoryEntities = await _categoryRepository.GetAllAsync(cancellationToken);

            //map the entities to response dtos and return them
            var categoryResponseDtos = _mapper.Map<IEnumerable<CategoryResponseDto>>(categoryEntities); 
            return categoryResponseDtos;
        }

        public async Task<CategoryResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            //get the category entity from database
            var categoryEntity = await _categoryRepository.GetByIdAsync(id, cancellationToken);
            if(categoryEntity == null)
            {
                throw new KeyNotFoundException("التصنيف غير موجود.");
            }

            //return the mapped response dto
            var categoryResponseDto = _mapper.Map<CategoryResponseDto>(categoryEntity);
            return categoryResponseDto;
        }
        public async Task<CategoryResponseDto> UpdateAsync(int id, UpdateCategoryRequestDto request, Guid? updatedBy, CancellationToken cancellationToken = default)
        {
            //check if parent category exists if parentCategoryId is provided
            var parentCategoryId = request.ParentCategoryId;
            if (parentCategoryId.HasValue)
            {
                var parentCategory = await _categoryRepository.GetByIdAsync(parentCategoryId.Value, cancellationToken);
                if (parentCategory == null)
                {
                    throw new KeyNotFoundException("التصنيف الأب غير موجود.");
                }
            }

            //get the category entity from database
            var categoryEntity = await _categoryRepository.GetByIdAsync(id, cancellationToken);
            if(categoryEntity == null)
            {
                throw new KeyNotFoundException("التصنيف غير موجود.");
            }

            //update the entity properties from request dto
            _mapper.Map(request, categoryEntity);
            categoryEntity.UpdatedBy = updatedBy;
            categoryEntity.UpdatedAt = DateTime.UtcNow;

            //save the changes to database
            await _categoryRepository.UpdateAsync(categoryEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            //return the mapped response dto
            var categoryResponseDto = _mapper.Map<CategoryResponseDto>(categoryEntity);
            return categoryResponseDto;
        }

        public async Task<CategoryResponseDto> DeleteAsync(int id, Guid? deletedBy, CancellationToken cancellationToken = default)
        {
            //get the category entity from database 
            var categoryEntity = await _categoryRepository.GetByIdAsync(id, cancellationToken);
            if (categoryEntity == null)
            {
                throw new KeyNotFoundException("التصنيف غير موجود.");
            }

            //soft delete the entity
            categoryEntity.UpdatedBy = deletedBy;
            await _categoryRepository.DeleteAsync(categoryEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            //return the mapped response dto
            var categoryResponseDto = _mapper.Map<CategoryResponseDto>(categoryEntity);
            return categoryResponseDto;

        }
    }
}
