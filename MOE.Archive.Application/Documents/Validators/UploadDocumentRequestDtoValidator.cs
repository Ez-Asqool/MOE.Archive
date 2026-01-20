using FluentValidation;
using MOE.Archive.Application.Documents.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Documents.Validators
{
    public class UploadDocumentRequestDtoValidator : AbstractValidator<UploadDocumentRequestDto>    
    {
        public UploadDocumentRequestDtoValidator() 
        { 
            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("رقم التصنيف غير صحيح.");
            RuleFor(x => x.DepartmentId)
                .NotEmpty().WithMessage("رقم القسم غير صحيح.");
            RuleFor(x => x.Files)
                .NotNull().WithMessage("الملفات مطلوبة.")
                .Must(f => f.Count > 0).WithMessage("الملفات مطلوبة.")
                .Must(f => f.Count <= 5).WithMessage("يمكن رفع 10 ملفات كحد أقصى.");
        }
    }
}
