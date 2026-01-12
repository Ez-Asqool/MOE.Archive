using AutoMapper;
using MOE.Archive.Application.Categories.DTOs;
using MOE.Archive.Domain.Entities;
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
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IMapper mapper, IDepartmentRepository departmentRepository, IDocumentRepository documentRepository   )
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _departmentRepository = departmentRepository;
            _documentRepository = documentRepository;
        }

        public async Task<CategoryResponseDto> CreateAsync(
                        CreateCategoryRequestDto request,
                        Guid? createdBy,
                        bool isAdmin,
                        bool isDeptAdmin,
                        int? callerDepartmentId,
                        CancellationToken cancellationToken = default)
        {
            if (createdBy == null)
                throw new UnauthorizedAccessException("يجب تسجيل الدخول لتنفيذ هذه العملية.");

            if (!isAdmin && !isDeptAdmin)
                throw new UnauthorizedAccessException("ليس لديك صلاحية لإضافة تصنيف.");

            Category? parent = null;

            // 1) Load parent if provided
            if (request.ParentCategoryId.HasValue)
            {
                parent = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);
                if (parent == null)
                    throw new KeyNotFoundException("التصنيف الأب غير موجود.");
            }

            // 2) Apply rules
            int? finalDepartmentId;

            if (isAdmin)
            {
                // Admin can create global main OR department main
                // If creating subcategory:
                // - if parent is department-specific -> child must be in same department
                // - if parent is global -> child can be global or department (Admin decides)
                finalDepartmentId = request.DepartmentId;

                if (parent != null && parent.DepartmentId.HasValue)
                {
                    // parent is department category -> force same department
                    if (!finalDepartmentId.HasValue)
                        finalDepartmentId = parent.DepartmentId.Value;

                    // also prevent creating a "global" child under department parent
                    if (request.DepartmentId.HasValue && request.DepartmentId.Value != parent.DepartmentId.Value)
                        throw new InvalidOperationException("لا يمكن إنشاء تصنيف تحت قسم مختلف عن قسم التصنيف الأب.");
                }
            }
            else // DeptAdmin
            {
                // DeptAdmin must have a department
                if (callerDepartmentId == null)
                    throw new InvalidOperationException("لا يمكن تنفيذ العملية: لم يتم تحديد القسم لمسؤول القسم.");

                // DeptAdmin can ONLY create categories for his department
                finalDepartmentId = callerDepartmentId.Value;

                // DeptAdmin cannot create global category
                // (enforced by always forcing DepartmentId = callerDepartmentId)

                if (parent != null)
                {
                    // DeptAdmin can add subcategories only under MAIN categories
                    //if (parent.ParentCategoryId.HasValue)
                    //    throw new UnauthorizedAccessException("يمكنك الإضافة فقط تحت التصنيفات الرئيسية.");

                    // Parent allowed if:
                    // - parent is global main (DepartmentId == null)   created by Admin usually
                    // - or parent is main for same department
                    if (parent.DepartmentId.HasValue && parent.DepartmentId.Value != callerDepartmentId.Value)
                        throw new UnauthorizedAccessException("لا يمكنك الإضافة تحت تصنيف رئيسي تابع لقسم آخر.");
                }

                // If parent is null => creating MAIN category for his department (allowed)
            }

            // 3) (Optional but recommended) ensure Department exists when departmentId is not null
            if (finalDepartmentId.HasValue) 
            {
                var dept = await _departmentRepository.GetByIdAsync(finalDepartmentId.Value, cancellationToken);
                if (dept == null)
                    throw new KeyNotFoundException("القسم غير موجود.");
            }

            // 4) Map + enforce final DepartmentId + audit
            var entity = _mapper.Map<Category>(request);
            entity.DepartmentId = finalDepartmentId;
            entity.CreatedBy = createdBy;

            // 5) Save
            await _categoryRepository.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CategoryResponseDto>(entity);
        }


        public async Task<List<CategoryResponseDto>> GetAllAsync(
                    bool isAdmin,
                    int? callerDepartmentId,
                    CancellationToken ct = default)
        {
            // Admin: return everything as tree
            if (isAdmin)
            {
                var all = await _categoryRepository.GetAllWithChildrenAsync(ct); // include Children
                var tree = BuildTree(all);
                return _mapper.Map<List<CategoryResponseDto>>(tree);
            }

            // Employee/DeptAdmin must have department
            if (!callerDepartmentId.HasValue)
                throw new UnauthorizedAccessException("لا يمكن تحديد قسم المستخدم.");

            var deptId = callerDepartmentId.Value;

            // Get allowed categories only (global + user dept)
            var allowed = await _categoryRepository.GetAllowedForDepartmentAsync(deptId, ct);

            // Build tree (only from allowed set)
            var treeAllowed = BuildTree(allowed);

            // Filter roots to: global main + dept main
            var roots = treeAllowed
                .Where(c => c.ParentCategoryId == null &&
                            (c.DepartmentId == null || c.DepartmentId == deptId))
                .ToList();

            return _mapper.Map<List<CategoryResponseDto>>(roots);
        }

        private static List<Category> BuildTree(List<Category> categories)
        {
            var dict = categories.ToDictionary(c => c.Id);

            // clear children to rebuild from allowed set only
            foreach (var c in categories)
                c.Children = new List<Category>();

            var roots = new List<Category>();

            foreach (var c in categories)
            {
                if (c.ParentCategoryId.HasValue && dict.TryGetValue(c.ParentCategoryId.Value, out var parent))
                {
                    parent.Children.Add(c);
                }
                else
                {
                    roots.Add(c);
                }
            }

            return roots;
        }


        public async Task<CategoryResponseDto> GetByIdAsync(
                    int id,
                    bool isAdmin,
                    int? callerDepartmentId,
                    CancellationToken cancellationToken = default)
        {
            // 1) get category from DB
            var categoryEntity = await _categoryRepository.GetByIdAsync(id, cancellationToken);
            if (categoryEntity == null)
                throw new KeyNotFoundException("التصنيف غير موجود.");

            // 2) Admin can access anything
            if (isAdmin)
                return _mapper.Map<CategoryResponseDto>(categoryEntity);

            // 3) Employee/DeptAdmin must have department id
            if (!callerDepartmentId.HasValue)
                throw new UnauthorizedAccessException("لا يمكن تحديد قسم المستخدم.");

            var deptId = callerDepartmentId.Value;

            // 4) Access rule: only global OR same department
            var allowed =
                categoryEntity.DepartmentId == null ||
                categoryEntity.DepartmentId == deptId;

            if (!allowed)
                //throw new UnauthorizedAccessException("ليس لديك صلاحية للوصول إلى هذا التصنيف.");
                //for security, do not reveal if the category exists or not
                throw new KeyNotFoundException("التصنيف غير موجود.");

            // 5) return dto
            return _mapper.Map<CategoryResponseDto>(categoryEntity);
        }


        public async Task<CategoryResponseDto> UpdateAsync(
            int id,
            UpdateCategoryRequestDto request,
            Guid? updatedBy,
            bool isAdmin,
            bool isDeptAdmin,
            int? callerDepartmentId,
            CancellationToken cancellationToken = default)
        {
            if (updatedBy == null)
                throw new UnauthorizedAccessException("يجب تسجيل الدخول لتنفيذ هذه العملية.");

            // Only Admin / DeptAdmin can update
            if (!isAdmin && !isDeptAdmin)
                throw new UnauthorizedAccessException("ليس لديك صلاحية لتعديل التصنيفات.");

            // 1) Load category
            var categoryEntity = await _categoryRepository.GetByIdAsync(id, cancellationToken);
            if (categoryEntity == null)
                throw new KeyNotFoundException("التصنيف غير موجود.");

            // 2) Authorization rule
            if (isDeptAdmin)
            {
                if (!callerDepartmentId.HasValue)
                    throw new UnauthorizedAccessException("لا يمكن تحديد قسم المستخدم.");

                // DeptAdmin can update only his department categories (NOT global)
                if (!categoryEntity.DepartmentId.HasValue || categoryEntity.DepartmentId.Value != callerDepartmentId.Value)
                    throw new UnauthorizedAccessException("ليس لديك صلاحية لتعديل هذا التصنيف.");
            }

            // 3) Validate parent if provided
            Domain.Entities.Category? parentCategory = null;

            if (request.ParentCategoryId.HasValue)
            {
                // prevent setting itself as parent
                if (request.ParentCategoryId.Value == id)
                    throw new InvalidOperationException("لا يمكن أن يكون التصنيف الأب هو نفس التصنيف.");

                parentCategory = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);
                if (parentCategory == null)
                    throw new KeyNotFoundException("التصنيف الأب غير موجود.");

                // Department rules when changing parent:
                // - If category is department category => parent must be global OR same department
                // - If category is global => parent must be global (DeptAdmin never reaches here)
                if (categoryEntity.DepartmentId.HasValue)
                {
                    var deptId = categoryEntity.DepartmentId.Value;

                    // parent can be global (null) or same department
                    if (parentCategory.DepartmentId.HasValue && parentCategory.DepartmentId.Value != deptId)
                        throw new InvalidOperationException("لا يمكن نقل التصنيف تحت تصنيف أب تابع لقسم مختلف.");
                }
                else
                {
                    // global category: parent must also be global
                    if (parentCategory.DepartmentId != null)
                        throw new InvalidOperationException("لا يمكن نقل تصنيف عام تحت تصنيف تابع لقسم.");
                }
            }

            // 4) Apply changes
            _mapper.Map(request, categoryEntity);
            categoryEntity.UpdatedBy = updatedBy;
            categoryEntity.UpdatedAt = DateTime.UtcNow;

            // 5) Save
            await _categoryRepository.UpdateAsync(categoryEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CategoryResponseDto>(categoryEntity);
        }


        public async Task<CategoryResponseDto> DeleteAsync(
            int id,
            Guid? deletedBy,
            bool isAdmin,
            bool isDeptAdmin,
            int? callerDepartmentId,
            CancellationToken cancellationToken = default)
        {
            if (deletedBy == null)
                throw new UnauthorizedAccessException("يجب تسجيل الدخول لتنفيذ هذه العملية.");

            if (!isAdmin && !isDeptAdmin)
                throw new UnauthorizedAccessException("ليس لديك صلاحية لحذف التصنيفات.");

            // 1) Load category
            var categoryEntity = await _categoryRepository.GetByIdAsync(id, cancellationToken);
            if (categoryEntity == null)
                throw new KeyNotFoundException("التصنيف غير موجود.");

            // 2) Authorization: DeptAdmin can delete only his department categories (not global)
            if (isDeptAdmin)
            {
                if (!callerDepartmentId.HasValue)
                    throw new UnauthorizedAccessException("لا يمكن تحديد قسم المستخدم.");

                if (!categoryEntity.DepartmentId.HasValue || categoryEntity.DepartmentId.Value != callerDepartmentId.Value)
                    throw new UnauthorizedAccessException("ليس لديك صلاحية لحذف هذا التصنيف.");
            }

            // 3) Check children categories
            var hasChildren = await _categoryRepository.ExistsAsync(
                c => c.ParentCategoryId == id && !c.IsDeleted,
                cancellationToken);

            if (hasChildren)
                throw new InvalidOperationException("لا يمكن حذف التصنيف لأنه يحتوي على تصنيفات فرعية.");

            // 4) Check documents in category
            var hasDocuments = await _documentRepository.ExistsAsync(
                d => d.CategoryId == id && !d.IsDeleted,
                cancellationToken);

            if (hasDocuments)
                throw new InvalidOperationException("لا يمكن حذف التصنيف لأنه يحتوي على مستندات.");

            // 5) Soft delete
            categoryEntity.UpdatedBy = deletedBy;
            categoryEntity.UpdatedAt = DateTime.UtcNow;

            await _categoryRepository.DeleteAsync(categoryEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CategoryResponseDto>(categoryEntity);
        }


        public async Task<List<CategoryResponseDto>> GetMainCategoriesAsync(bool isAdmin, CancellationToken ct = default)
        {
            if (!isAdmin)
                throw new UnauthorizedAccessException("ليس لديك صلاحية للوصول إلى التصنيفات الرئيسية.");

            var mainCategories = await _categoryRepository.FindAsync(c => c.ParentCategoryId == null && c.DepartmentId == null);
            var categoryResponseDtos = _mapper.Map<IEnumerable<CategoryResponseDto>>(mainCategories);

            return categoryResponseDtos.ToList();   
        }
    }
}
