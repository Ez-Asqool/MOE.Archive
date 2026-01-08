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
        }

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
    }
}