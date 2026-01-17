using FluentValidation;
using MOE.Archive.Application.Documents.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Documents.Validators
{
    public class UpdateDocumentRequestDtoValidator : AbstractValidator<UpdateDocumentRequestDto>
    {
        public UpdateDocumentRequestDtoValidator()
        {
            RuleFor(x => x.OriginalName)
                .MaximumLength(260).WithMessage("اسم الملف الأصلي يجب ألا يتجاوز 260 حرفاً.")
                .When(x => x.OriginalName != null);

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("رقم التصنيف غير صحيح.")
                .When(x => x.CategoryId.HasValue);

            RuleFor(x => x)
                .Must(x => x.OriginalName != null || x.CategoryId.HasValue)
                .WithMessage("يجب إرسال اسم الملف أو رقم التصنيف للتحديث.");
        }
    }
}
