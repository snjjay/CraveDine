using EatKath.API.DTOs.Area;
using FluentValidation;

namespace EatKath.API.Validators.Area;

// ==========================================================
// VALIDATOR
// ==========================================================
//
// 🛂 Think: "The DTO is a box of data.
//            The Validator checks whether the data is okay."
//
// Flow:
//
// React
//   ↓
// CreateAreaDto 📦
//   ↓
// CreateAreaValidator 🛂
//   ↓
// "Is the data valid?"
//   ↓
// YES → Controller → Service → Database
// NO  → 400 Bad Request
//
// DTO and Validator have different jobs:
//
// CreateAreaDto       → Carries the data
// CreateAreaValidator → Checks the data
//
// ==========================================================

public class CreateAreaValidator : AbstractValidator<CreateAreaDto>
{
    public CreateAreaValidator()
    {
        RuleFor(x => x.Name) //I am going to check the Name inside the DTO
            .NotEmpty().WithMessage("Area name is required.") //Name cannot be empty
            .MaximumLength(100).WithMessage("Area name cannot exceed 100 characters.");//Name cannot be longer than 100 characters
    }
}


// ==========================================================
// EATKATH REQUEST FLOW
// ==========================================================
//
// 📱 React
//    ↓
// 📦 DTO
//    "Here is my data"
//    ↓
// 🛂 Validator
//    "Is this data okay?"
//    ↓
// 🎯 Controller
//    "I'll send it to the right place"
//    ↓
// ⚙️ Service
//    "I'll do the actual work"
//    ↓
// 🗄️ Database
//    "I'll store/retrieve the data"
//
// ==========================================================