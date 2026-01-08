using FluentValidation;
using MOE.Archive.Application.Categories.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Categories.Validators
{
    public class CreateCategoryRequestDtoValidator : AbstractValidator<CreateCategoryRequestDto>
    {
        public CreateCategoryRequestDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("اسم التصنيف مطلوب.")
                .MaximumLength(150).WithMessage("اسم التصنيف يجب ألا يتجاوز 150 حرفاً.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("وصف التصنيف يجب ألا يتجاوز 500 حرفاً.");

            RuleFor(x => x.ParentCategoryId)
                .GreaterThan(0).WithMessage("رقم التصنيف الأب غير صحيح.")
                .When(x => x.ParentCategoryId.HasValue);

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("رقم القسم غير صحيح.")
                .When(x => x.DepartmentId.HasValue);
        }
    }
}
