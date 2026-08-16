using Microsoft.AspNetCore.Http;

namespace EatKath.API.Enums
{
    public enum OfferType //Enum = a list of allowed choices.
    {
        DineIn = 1,
        Takeaway = 2,
        Delivery = 3
    }
}


// ==========================================================
// ENUMS
// ==========================================================
//
// 🔢 Think: "Enum = a fixed list of allowed choices."
//
// Instead of using random text, we define the choices
// that are allowed.
//
// Example:
//
// OfferType
//   → Percentage
//   → FixedAmount
//
// RedemptionStatus
//   → Pending
//   → Redeemed
//   → Cancelled
//
// Enums can be used by:
// DTOs
// Entities
// Services
//
// 🔑 Enum = fixed set of choices.
// ==========================================================



//Program.cs
// ↓
//Middleware
// ↓
//Controller
// ↓
//DTO
// ↓
//Validator
// ↓
//Service
// ↓
//Entity
// ↓
//DbContext
// ↓
//Database

//Enums can be used by DTOs, Entities and Services
//to represent a fixed set of choices.