using FluentValidation;
using MOE.Archive.Application.UserManagement.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.UserManagement.Validators
{
    public class UpdateUserRequestDtoValidator : AbstractValidator<UpdateUserRequestDto>
    {
        public UpdateUserRequestDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("معرّف المستخدم مطلوب.");

            RuleFor(x => x.FullName)
                .MaximumLength(150).WithMessage("الاسم الكامل يجب ألا يزيد عن 150 حرفًا.")
                .When(x => x.FullName != null);

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("البريد الإلكتروني غير صحيح.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(50).WithMessage("رقم الهاتف يجب ألا يزيد عن 50 حرفًا.")
                .When(x => x.PhoneNumber != null);

            RuleFor(x => x.JobNumber)
                .MaximumLength(50).WithMessage("الرقم الوظيفي يجب ألا يزيد عن 50 حرفًا.")
                .When(x => x.JobNumber != null);

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("رقم القسم غير صحيح.")
                .When(x => x.DepartmentId.HasValue);

            RuleFor(x => x.Role)
                .Must(r => r is null || r == "Employee" || r == "DeptAdmin")
                .WithMessage("الدور غير صحيح. الأدوار المسموحة: Employee أو DeptAdmin.");
        }
    }
}
