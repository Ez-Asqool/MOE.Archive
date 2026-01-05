using FluentValidation;
using MOE.Archive.Application.Auth.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOE.Archive.Application.Auth.Validators
{
    public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestDtoValidator()
        {
            RuleFor(x => x.UserNameOrEmail)
                .NotEmpty().WithMessage("البريد الإلكتروني أو اسم المستخدم مطلوب")
                .MaximumLength(256).WithMessage("لا يجب أن يتجاوز 256 حرفاً");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("كلمة المرور مطلوبة")
                .MinimumLength(6).WithMessage("كلمة المرور يجب أن تكون على الأقل 6 أحرف")
                .MaximumLength(100).WithMessage("كلمة المرور يجب أن تكون أقل من 100 حرف");
        }
    }
}
