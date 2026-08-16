public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message)
        : base(message)
    {
    }
}




// ==========================================================
// EXCEPTIONS
// ==========================================================
//
// ⚠️ Think: "Something went wrong according to our rules."
//
// Services can THROW custom exceptions when a problem occurs.
//
// DuplicateEntityException
// → Something already exists.
//
// BusinessRuleException
// → An EatKath business rule was broken.
//
// Flow:
//
// Service
//    ↓
// throw Exception
//    ↓
// ExceptionMiddleware
//    ↓
// HTTP response
//    ↓
// React
//
// 🔑 Service = reports the problem
// ExceptionMiddleware = turns it into an HTTP response
// ==========================================================



//React
// ↓
//Controller
// ↓
//DTO
// ↓
//Validator
// ↓
//Service
// ↓
//❌ Exception  ← here when a business problem occurs
// ↓
//ExceptionMiddleware //This is already done in middleware
// ↓
//HTTP response → React