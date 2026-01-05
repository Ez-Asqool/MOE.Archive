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
        Task<DocumentResponseDto> UploadAsync(UploadDocumentRequestDto request, Guid? currentUserId, CancellationToken ct = default);
    }
}
