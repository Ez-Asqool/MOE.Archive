using FluentValidation;
using MOE.Archive.Application.Departments.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Departments.Validators
{
    public class CreateDepartmentRequestDtoValidator : AbstractValidator<CreateDepartmentRequestDto>
    {
        public CreateDepartmentRequestDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("اسم القسم لا يجب ان يكون فارغا")
                .When(x => x.Name is not null)
                .MaximumLength(150).WithMessage("اسم القسم يجب ألا يزيد عن 150 حرفاً."); 
        }
    }
}
