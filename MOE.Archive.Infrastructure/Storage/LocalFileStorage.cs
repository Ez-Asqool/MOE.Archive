using Microsoft.AspNetCore.Hosting;
using MOE.Archive.Application.Common.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Infrastructure.Storage
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public LocalFileStorage(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<(string relativePath, string savedFileName)> SaveAsync(
            Stream fileStream,
            string originalFileName,
            int departmentId,
            int categoryId,
            Guid documentId,
            CancellationToken ct = default)
        {
            // Ensure wwwroot exists
            var webRoot = _hostingEnvironment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
            {
                webRoot = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot");
                Directory.CreateDirectory(webRoot);
            }

            var now = DateTime.UtcNow;

            // wwwroot/MoEArchiveStorage/departments/{dept}/categories/{cat}/{yyyy}/{MM}/
            var relativeFolder = Path.Combine(
                "MoEArchiveStorage",
                "departments", departmentId.ToString(),
                "categories", categoryId.ToString(),
                now.Year.ToString(),
                now.Month.ToString("D2")
            );

            var absoluteFolder = Path.Combine(webRoot, relativeFolder);
            Directory.CreateDirectory(absoluteFolder);

            // GUID filename + extension
            //var ext = Path.GetExtension(originalFileName);
            //var safeExt = string.IsNullOrWhiteSpace(ext) ? "" : ext;
            var savedFileName = $"{documentId}{originalFileName}";

            var absolutePath = Path.Combine(absoluteFolder, savedFileName);

            // Use Create (safe with GUID names) + async IO
            await using var outStream = new FileStream(
                absolutePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            await fileStream.CopyToAsync(outStream, ct);

            // Store relative path in DB (use forward slashes)
            var relativePath = Path.Combine(relativeFolder, savedFileName).Replace('\\', '/');
            return (relativePath, savedFileName);
        }
    }
}
