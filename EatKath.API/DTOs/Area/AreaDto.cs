namespace EatKath.API.DTOs.Area;

// ==========================================================
// DTOs (Data Transfer Objects)
// ==========================================================
//
// 📦 Think: DTO = a box that carries data.
//
// When React wants to CREATE an Area:
//
// React
//   ↓
// "I want to create an Area"
//   ↓
// CreateAreaDto 📦
//   ↓
// AreasController
//   ↓
// AreaService
//   ↓
// Database
//
// The CreateAreaDto is simply the box containing the data
// that React sends to the Controller.
//
// Example:
//
// React sends:
// {
//     "name": "Thamel"
// }
//
// ASP.NET puts that data into:
// CreateAreaDto 📦
//
// Then the Controller passes that box to the Service:
//
// _areaService.CreateAsync(dto);
//
// So:
//
// CreateAreaDto = "Here is the data needed to create an Area."
//
//
// ----------------------------------------------------------
// When React wants to UPDATE an Area:
//
// React
//   ↓
// "I want to update Area 5"
//   ↓
// UpdateAreaDto 📦
//   ↓
// AreasController
//   ↓
// AreaService
//   ↓
// Database
//
// UpdateAreaDto = "Here is the new data for the Area."
//
//
// ----------------------------------------------------------
// When sending Area data BACK to React:
//
// Database
//   ↓
// AreaService
//   ↓
// AreaDto 📦
//   ↓
// AreasController
//   ↓
// React
//
// AreaDto = "Here is the Area data to send back."
//
// 🔑 Remember:
// DTO = a box that carries data.
// ==========================================================
public class AreaDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}