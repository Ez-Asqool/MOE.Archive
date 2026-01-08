using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Domain.Enums
{
    public enum RoleType
    {
        Admin = 1,
        Employee = 2,
        DeptAdmin = 3   
    }

    public enum OcrStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3
    }

    public enum IndexStatus
    {
        Pending = 0,
        Indexing = 1,
        Indexed = 2,
        Failed = 3
    }

    public enum AuditAction
    {
        Login = 0,
        Logout = 1,
        UploadDocument = 2,
        ViewDocument = 3,
        DownloadDocument = 4,
        DeleteDocument = 5,
        UpdateDocument = 6
    }
}
