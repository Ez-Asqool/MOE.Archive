using FluentValidation;
using MOE.Archive.Application.UserManagement.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.UserManagement.Validators
{
    public class AdminCreateUserRequestDtoValidator : AbstractValidator<AdminCreateUserRequestDto>
    {
        public AdminCreateUserRequestDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("الاسم الكامل مطلوب.")
                .MaximumLength(150).WithMessage("الاسم الكامل يجب ألا يتجاوز 150 حرفاً.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
                .EmailAddress().WithMessage("البريد الإلكتروني غير صحيح.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("كلمة المرور مطلوبة.")
                .MinimumLength(6).WithMessage("كلمة المرور يجب ألا تقل عن 6 أحرف.");

            RuleFor(x => x.JobNumber)
                .NotEmpty().WithMessage("الرقم الوظيفي مطلوب.")
                .MaximumLength(50).WithMessage("الرقم الوظيفي يجب ألا يتجاوز 50 حرفاً.");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("رقم القسم غير صحيح.");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("الدور مطلوب.")
                .Must(r => r == "Employee" || r == "DeptAdmin")
                .WithMessage("الدور يجب أن يكون Employee أو DeptAdmin.");
        }
    }
}
