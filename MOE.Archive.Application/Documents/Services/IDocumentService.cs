using Microsoft.AspNetCore.Http;
using MOE.Archive.Application.Documents.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Documents.Services
{
    public interface IDocumentService
    {
        //Task<DocumentResponseDto> UploadAsync(UploadDocumentRequestDto request, Guid? currentUserId, CancellationToken ct = default);

        Task<List<DocumentResponseDto>> UploadAsync(
            UploadDocumentRequestDto request,
            Guid? currentUserId,
            bool isAdmin,
            int? callerDepartmentId,
            CancellationToken ct = default);

        Task<DocumentResponseDto> UpdateAsync(
        Guid documentId,
        UpdateDocumentRequestDto request,
        Guid? currentUserId,
        string currentRole,
        int? currentDepartmentId,
        CancellationToken ct = default);



        Task<DocumentResponseDto> DeleteAsync(
            Guid documentId,
            Guid? currentUserId,
            bool isAdmin,
            int? callerDepartmentId,
            CancellationToken ct = default);
    }
}
