using FluentValidation;
using MOE.Archive.Application.Categories.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Categories.Validators
{
    public class UpdateCategoryRequestDtoValidator : AbstractValidator<UpdateCategoryRequestDto>
    {
        public UpdateCategoryRequestDtoValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("اسم التصنيف لا يمكن أن يكون فارغاً.")
                .When(x => x.Name is not null)
                .MaximumLength(150).WithMessage("اسم التصنيف يجب ألا يزيد عن 150 حرفاً.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("الوصف يجب ألا يزيد عن 500 حرف.");

            RuleFor(x => x.ParentCategoryId)
                .GreaterThan(0).WithMessage("رقم التصنيف الأب غير صحيح.")
                .When(x => x.ParentCategoryId.HasValue);

        }
    }
}
