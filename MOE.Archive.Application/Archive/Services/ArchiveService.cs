using AutoMapper;
using MOE.Archive.Application.Archive.DTOs;
using MOE.Archive.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Archive.Services
{
    public class ArchiveService : IArchiveService
    {
        private readonly ICategoryRepository _categoryRepository;   
        private readonly IDocumentRepository _documentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ArchiveService(IUnitOfWork unitOfWork, IMapper mapper, ICategoryRepository categoryRepository, IDocumentRepository documentRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _categoryRepository = categoryRepository;
            _documentRepository = documentRepository;
        }

        public async Task<List<ArchiveTreeDto>> GetArchiveAsync(
            bool isAdmin,
            int? callerDepartmentId,
            int? requestedDepartmentId,
            CancellationToken ct = default)
        {
            // 1) Determine target department
            int? targetDeptId;

            if (isAdmin)
            {
                // Admin can request a department archive explicitly
                if (!requestedDepartmentId.HasValue)
                    throw new InvalidOperationException("Admin must provide departmentId to view a department archive.");

                targetDeptId = requestedDepartmentId.Value;
            }
            else
            {
                // Employee/DeptAdmin must use own department
                if (!callerDepartmentId.HasValue)
                    throw new UnauthorizedAccessException("لا يمكن تحديد قسم المستخدم.");

                targetDeptId = callerDepartmentId.Value;
            }

            // 2) Load allowed categories (global + target department)
            // global: DepartmentId == null
            // dept: DepartmentId == targetDeptId
            var categories = (await _categoryRepository.FindAsync(
                c => c.DepartmentId == null || c.DepartmentId == targetDeptId,
                ct)).ToList();

            var categoryIds = categories.Select(c => c.Id).ToList();

            // 3) Load documents for those categories ONLY for target department
            // (documents are department-scoped)
            var documents = (await _documentRepository.FindAsync(
                d => categoryIds.Contains(d.CategoryId)
                     && d.DepartmentId == targetDeptId
                     && !d.IsDeleted,
                ct)).ToList();

            // 4) Map categories to DTO dictionary
            var dtoById = categories.ToDictionary(
                c => c.Id,
                c => _mapper.Map<ArchiveTreeDto>(c));

            // 5) Attach documents to category DTOs
            foreach (var doc in documents)
            {
                if (dtoById.TryGetValue(doc.CategoryId, out var catDto))
                {
                    catDto.Documents.Add(_mapper.Map<DocumentListItemDto>(doc));
                }
            }

            // 6) Build tree (children)
            var roots = new List<ArchiveTreeDto>();

            foreach (var cat in categories)
            {
                var dto = dtoById[cat.Id];

                if (cat.ParentCategoryId.HasValue && dtoById.TryGetValue(cat.ParentCategoryId.Value, out var parentDto))
                {
                    parentDto.Children.Add(dto);
                }
                else
                {
                    roots.Add(dto);
                }
            }

            // 7) Root filter:
            // return only main categories: ParentCategoryId == null
            roots = roots.Where(r => r.ParentCategoryId == null).ToList();

            return roots;
        }
    }
}
