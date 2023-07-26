using FluentValidation;
using Files.Models;

public class GuidValidator : AbstractValidator<string>{

    public GuidValidator(){
        RuleFor(x => x).Must(BeValidGuid).WithMessage("Param must be a valid UUID");
    }

    private bool BeValidGuid(string guid)
    {
        try{
            bool isValid = Guid.TryParse(guid, out _);
            return isValid;
        }
        catch{
            return false;
        }
        
    }

}


public class UploadChunksRequestDtoValidor : AbstractValidator<UploadChunkRequestDto>
{
    public UploadChunksRequestDtoValidor ()
    {
        RuleFor(x => x.FileId)
            .NotEmpty().WithMessage("File Id must not been empty")
            .Must(BeValidGuid).WithMessage("File id must have valid UUID pattern");
        RuleFor(x => x.Number)
            .NotNull().WithMessage("Number must not be null")
            .Must(x => x>-1);
        RuleFor(x => x.Data)
            .NotNull().WithMessage("Data must not be null")
            .NotEmpty().WithMessage("Data must no be empty");
        RuleFor(x => x.Size)
            .NotNull().WithMessage("Size must no be null")
            .Must(x => x>-1);
        RuleFor(x => x.FileSize)
            .NotNull().WithMessage("FileSize must no be null")
            .Must(x => x>-1).WithMessage("FileSize can not be negative");
        RuleFor(x => x.Type)
            .NotNull().WithMessage("Type must not be null")
            .NotEmpty().WithMessage("Type must no be empty")
            .MaximumLength(50);
        RuleFor(x => x.FileName)
            .NotNull().WithMessage("FileName must not be null")
            .NotEmpty().WithMessage("FileName must no be empty")
            .MaximumLength(200);

        
    }
    private bool BeValidGuid(Guid guid)
    {
        return !Guid.Empty.Equals(guid);
    }
}
