using FluentValidation;
using DevResourceAPI.DTOs; 

namespace DevResourceAPI.Validators;
// DOĞRU: Girdi (Create) DTO'sunu kontrol edecek
public class ResourceValidator : AbstractValidator<CreateResourceDto> 
{
    public ResourceValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık alanı boş bırakılamaz.")
            .MinimumLength(3).WithMessage("Başlık en az 3 karakter olmalıdır.");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("URL alanı boş bırakılamaz.")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Geçerli bir URL giriniz (örn: https://google.com).");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Lütfen bir kategori seçiniz.");
    }
}