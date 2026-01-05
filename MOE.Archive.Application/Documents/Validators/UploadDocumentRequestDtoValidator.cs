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
            RuleFor(x => x.File)
                .NotNull().WithMessage("الملف مطلوب.")
                .Must(file => file != null && file.Length > 0).WithMessage("الملف لا يمكن أن يكون فارغاً."); 
        }
    }
}
