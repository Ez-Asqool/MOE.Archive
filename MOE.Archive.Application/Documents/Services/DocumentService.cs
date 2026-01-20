using AutoMapper;
using Microsoft.AspNetCore.Http;
using MOE.Archive.Application.Common.Storage;
using MOE.Archive.Application.Documents.DTOs;
using MOE.Archive.Domain.Entities;
using MOE.Archive.Domain.Enums;
using MOE.Archive.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Documents.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDocumentRepository _documentRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFileStorage _fileStorage;
        private readonly IMapper _mapper;

        public DocumentService(IUnitOfWork unitOfWork, IFileStorage fileStorage, IMapper mapper, IDocumentRepository documentRepository, IDepartmentRepository departmentRepository, ICategoryRepository categoryRepository)
        {
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
            _mapper = mapper;
            _documentRepository = documentRepository;
            _departmentRepository = departmentRepository;
            _categoryRepository = categoryRepository;
        }


        public async Task<List<DocumentResponseDto>> UploadAsync(
            UploadDocumentRequestDto request,
            Guid? currentUserId,
            bool isAdmin,
            int? callerDepartmentId,
            CancellationToken ct = default)
        {
            // 0) Auth
            if (currentUserId is null)
                throw new UnauthorizedAccessException("المستخدم غير مصرح له رفع الملفات.");

            // 1) Files validation
            if (request.Files == null || request.Files.Count == 0)
                throw new InvalidOperationException("الملفات مطلوبة.");

            if (request.Files.Count > 10)
                throw new InvalidOperationException("يمكن رفع 10 ملفات كحد أقصى في الطلب الواحد.");

            // 2) Non-admin must upload only to his department
            if (!isAdmin)
            {
                if (!callerDepartmentId.HasValue)
                    throw new UnauthorizedAccessException("لا يمكن تحديد قسم المستخدم.");

                if (request.DepartmentId != callerDepartmentId.Value)
                    throw new UnauthorizedAccessException("لا يمكنك رفع ملف لقسم آخر.");
            }

            // 3) Validate Department exists
            var dept = await _departmentRepository.GetByIdAsync(request.DepartmentId, ct);
            if (dept == null)
                throw new KeyNotFoundException("القسم غير موجود.");

            // 4) Validate Category exists
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, ct);
            if (category == null)
                throw new KeyNotFoundException("الرجاء إدخال تصنيف موجود.");

            // 5) Category rules
            // Admin:
            //  - allowed global category
            //  - allowed dept category only if it matches requested dept
            // Non-admin:
            //  - allowed global category
            //  - allowed dept category only if it matches user dept
            if (isAdmin)
            {
                if (category.DepartmentId.HasValue && category.DepartmentId.Value != request.DepartmentId)
                    throw new UnauthorizedAccessException("التصنيف لا يتبع نفس القسم المحدد.");
            }
            else
            {
                var userDeptId = callerDepartmentId!.Value;

                if (category.DepartmentId.HasValue && category.DepartmentId.Value != userDeptId)
                    throw new UnauthorizedAccessException("لا يمكنك رفع الملفات تحت تصنيفات قسم آخر.");
            }

            // 6) Upload each file
            var results = new List<DocumentResponseDto>();

            foreach (var file in request.Files)
            {
                if (file == null || file.Length == 0)
                {
                    var fileName = file?.FileName ?? "غير معروف";
                    throw new InvalidOperationException($"الملف '{fileName}' فارغ أو غير صالح.");
                }

                var originalName = file.FileName;
                var mimeType = string.IsNullOrWhiteSpace(file.ContentType)
                    ? "application/octet-stream"
                    : file.ContentType;

                var fileSize = file.Length;

                var doc = new Document
                {
                    Id = Guid.NewGuid(),
                    OriginalName = originalName,
                    MimeType = mimeType,
                    FileSize = fileSize,
                    DepartmentId = request.DepartmentId,
                    CategoryId = request.CategoryId,
                    OcrStatus = OcrStatus.Pending,
                    IndexStatus = IndexStatus.Pending,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId.Value
                };

                // Save file to storage
                await using var stream = file.OpenReadStream();
                var (relativePath, savedFileName) = await _fileStorage.SaveAsync(
                    stream,
                    originalName,
                    request.DepartmentId,
                    request.CategoryId,
                    doc.Id,
                    ct);

                doc.FileName = savedFileName;
                doc.FilePath = relativePath;

                await _documentRepository.AddAsync(doc, ct);

                results.Add(_mapper.Map<DocumentResponseDto>(doc));
            }

            // 7) Save DB once (faster)
            await _unitOfWork.SaveChangesAsync(ct);

            return results;
        }

        /*
        public async Task<DocumentResponseDto> UploadAsync(
                UploadDocumentRequestDto request,
                Guid? currentUserId,
                bool isAdmin,
                int? callerDepartmentId,
                CancellationToken ct = default)
        {
            // 0) Auth
            if (currentUserId is null)
                throw new UnauthorizedAccessException("المستخدم غير مصرح له رفع الملفات.");

            // 1) File validation
            if (request.File == null || request.File.Length == 0)
                throw new InvalidOperationException("الملف مطلوب.");

            // 2) Non-admin must upload only to his department
            if (!isAdmin)
            {
                if (!callerDepartmentId.HasValue)
                    throw new UnauthorizedAccessException("لا يمكن تحديد قسم المستخدم.");

                if (request.DepartmentId != callerDepartmentId.Value)
                    throw new UnauthorizedAccessException("لا يمكنك رفع ملف لقسم آخر.");
            }

            // 3) Validate Department exists
            var dept = await _departmentRepository.GetByIdAsync(request.DepartmentId, ct);
            if (dept == null)
                throw new KeyNotFoundException("القسم غير موجود.");

            // 4) Validate Category exists
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, ct);
            if (category == null)
                throw new KeyNotFoundException("الرجاء إدخال تصنيف موجود.");

            // 5) Category rules:
            // Admin:
            //  - allowed global category
            //  - allowed dept category only if it matches requested dept
            // Non-admin:
            //  - allowed global category
            //  - allowed dept category only if it matches user dept
            if (isAdmin)
            {
                if (category.DepartmentId.HasValue && category.DepartmentId.Value != request.DepartmentId)
                    throw new UnauthorizedAccessException("التصنيف لا يتبع نفس القسم المحدد.");
            }
            else
            {
                var userDeptId = callerDepartmentId!.Value;

                if (category.DepartmentId.HasValue && category.DepartmentId.Value != userDeptId)
                    throw new UnauthorizedAccessException("لا يمكنك رفع الملفات تحت تصنيفات قسم آخر.");
            }

            // 6) Extract metadata
            var originalName = request.File.FileName;
            var mimeType = string.IsNullOrWhiteSpace(request.File.ContentType)
                ? "application/octet-stream"
                : request.File.ContentType;
            var fileSize = request.File.Length;

            // 7) Create Document entity (CreatedBy REQUIRED)
            var doc = new Document
            {
                Id = Guid.NewGuid(),
                OriginalName = originalName,
                MimeType = mimeType,
                FileSize = fileSize,
                DepartmentId = request.DepartmentId,
                CategoryId = request.CategoryId,
                OcrStatus = OcrStatus.Pending,
                IndexStatus = IndexStatus.Pending,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId.Value
            };

            // 8) Save file to storage
            await using var stream = request.File.OpenReadStream();
            var (relativePath, savedFileName) = await _fileStorage.SaveAsync(
                stream,
                originalName,
                request.DepartmentId,
                request.CategoryId,
                doc.Id,
                ct);

            doc.FileName = savedFileName;
            doc.FilePath = relativePath;

            // 9) Save DB
            await _documentRepository.AddAsync(doc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // 10) Return response
            return _mapper.Map<DocumentResponseDto>(doc);
        }*/

        //    public async Task<DocumentResponseDto> UploadAsync(
        //        UploadDocumentRequestDto request,
        //        Guid? currentUserId,
        //        CancellationToken ct = default)
        //    {
        //        // check if user id is null
        //        if (currentUserId is null)
        //            throw new UnauthorizedAccessException("المستخدم غير مصرح له رفع الملفات");

        //        if (request.File == null || request.File.Length == 0)
        //            throw new InvalidOperationException("الملف مطلوب");

        //        // Validate Category exists (global category)
        //        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, ct);
        //        if (category == null)
        //            throw new KeyNotFoundException("الرجاء ادخال تصنيف موجود");

        //        // Validate Department exists
        //        var dept = await _departmentRepository.GetByIdAsync(request.DepartmentId, ct);
        //        if (dept == null)
        //            throw new KeyNotFoundException("القسم غير موجود");

        //        // Extract metadata from file
        //        var originalName = request.File.FileName;
        //        var mimeType = string.IsNullOrWhiteSpace(request.File.ContentType)
        //            ? "application/octet-stream"
        //            : request.File.ContentType;

        //        var fileSize = request.File.Length;

        //        // Create Document entity 
        //        var doc = new Document
        //        {
        //            Id = Guid.NewGuid(),
        //            OriginalName = originalName,
        //            MimeType = mimeType,
        //            FileSize = fileSize,
        //            DepartmentId = request.DepartmentId,
        //            CategoryId = request.CategoryId,
        //            OcrStatus = OcrStatus.Pending,
        //            IndexStatus = IndexStatus.Pending,
        //            IsDeleted = false,
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = currentUserId.Value
        //        };

        //        // Save file
        //        await using var stream = request.File.OpenReadStream();
        //        var (relativePath, savedFileName) = await _fileStorage.SaveAsync(
        //            stream,
        //            originalName,
        //            request.DepartmentId,
        //            request.CategoryId,
        //            doc.Id,
        //            ct);

        //        doc.FileName = savedFileName;
        //        doc.FilePath = relativePath;

        //        // Save to DB 
        //        await _documentRepository.AddAsync(doc, ct);
        //        await _unitOfWork.SaveChangesAsync(ct);

        //        // Map to response
        //        var response = _mapper.Map<DocumentResponseDto>(doc);
        //        response.CreatedBy = currentUserId.Value;
        //        return response;
        //    }
        //}

        public async Task<DocumentResponseDto> UpdateAsync(
        Guid documentId,
        UpdateDocumentRequestDto request,
        Guid? currentUserId,
        string currentRole,
        int? currentDepartmentId,
        CancellationToken ct = default)
        {
            if (currentUserId is null)
                throw new UnauthorizedAccessException("غير مصرح.");

            // 1) Load document
            var doc = await _documentRepository.GetByIdAsync(documentId, ct);
            if (doc == null)
                throw new KeyNotFoundException("الملف غير موجود.");

            // 2) Permission: non-admin must be same department
            var isAdmin = currentRole == "Admin";
            if (!isAdmin)
            {
                if (!currentDepartmentId.HasValue)
                    throw new UnauthorizedAccessException("غير مصرح.");

                if (doc.DepartmentId != currentDepartmentId.Value)
                    throw new UnauthorizedAccessException("غير مصرح لك بتعديل ملفات قسم آخر.");
            }

            // 3) Update OriginalName (if provided)
            if (!string.IsNullOrWhiteSpace(request.OriginalName))
            {
                doc.OriginalName = request.OriginalName.Trim();
            }

            // 4) Update CategoryId (if provided)
            if (request.CategoryId.HasValue)
            {
                var newCategory = await _categoryRepository.GetByIdAsync(request.CategoryId.Value, ct);
                if (newCategory == null)
                    throw new KeyNotFoundException("التصنيف غير موجود.");

                // Non-admin: ensure category is allowed for their department
                //if (!isAdmin)
                //{
                    // Allowed: global main category (DepartmentId null) OR dept category matching user's dept
                    if (newCategory.DepartmentId != null && newCategory.DepartmentId != currentDepartmentId)
                        throw new UnauthorizedAccessException("لا يمكنك اختيار تصنيف تابع لقسم آخر.");
                //}

                doc.CategoryId = request.CategoryId.Value;
            }

            // 5) Audit fields
            doc.UpdatedAt = DateTime.UtcNow;
            doc.UpdatedBy = currentUserId;

            // 6) Save
            await _documentRepository.UpdateAsync(doc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return _mapper.Map<DocumentResponseDto>(doc);
        }


        public async Task<DocumentResponseDto> DeleteAsync(
            Guid documentId,
            Guid? currentUserId,
            bool isAdmin,
            int? callerDepartmentId,
            CancellationToken ct = default)
        {
            if (currentUserId is null)
                throw new UnauthorizedAccessException("غير مصرح.");

            // Load document
            var doc = await _documentRepository.GetByIdAsync(documentId, ct);
            if (doc == null)
                throw new KeyNotFoundException("الملف غير موجود.");

            // Permission: non-admin must be same department
            if (!isAdmin)
            {
                if (!callerDepartmentId.HasValue)
                    throw new UnauthorizedAccessException("لا يمكن تحديد قسم المستخدم.");

                if (doc.DepartmentId != callerDepartmentId.Value)
                    throw new UnauthorizedAccessException("لا يمكنك حذف ملف تابع لقسم آخر.");
            }

            // Soft delete
            doc.UpdatedAt = DateTime.UtcNow;
            doc.UpdatedBy = currentUserId;

            await _documentRepository.DeleteAsync(doc, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return _mapper.Map<DocumentResponseDto>(doc);
        }
    }
}