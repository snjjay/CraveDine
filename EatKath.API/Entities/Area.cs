namespace EatKath.API.Entities
{
    public class Area //I am defining what an Area looks like in the database
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // Navigation Property
        public ICollection<Restaurant> Restaurants { get; set; } = new List<Restaurant>(); //An Area can have many Restaurants
          //      Thamel
          //│
          //├── Restaurant A
          //├── Restaurant B
          //└── Restaurant C
    }
}


// ==========================================================
// AREA ENTITY
// ==========================================================
//
// 🗄️ Think: "Entity = represents a database record."
//
// Area Entity represents an Area in the database.
//
// Example database record:
//
// Id     Name
// ----------------
// 1      Thamel
// 2      Patan
//
// ----------------------------------------------------------
//
// Properties:
//
// Id
// → Unique ID / Primary Key
//
// Name
// → Name of the Area
//
// Restaurants
// → Navigation Property
// → One Area can have many Restaurants
//
// Example:
//
// Thamel
//   ├── Restaurant A
//   ├── Restaurant B
//   └── Restaurant C
//
// ----------------------------------------------------------
//
// Entity vs DTO:
//
// Entity → represents database data 🗄️
// DTO    → carries API data 📦
//
// ----------------------------------------------------------
//
// CREATE:
//
// React
//   ↓
// CreateAreaDto 📦
//   ↓
// Controller
//   ↓
// Service
//   ↓
// AutoMapper
//   ↓
// Area Entity 🗄️
//   ↓
// DbContext
//   ↓
// Database
//
// READ:
//
// Database
//   ↓
// DbContext
//   ↓
// Area Entity 🗄️
//   ↓
// AutoMapper
//   ↓
// AreaDto 📦
//   ↓
// Controller
//   ↓
// React
//
// 🔑 Remember:
//
// Entity = database representation
// DTO    = API data box
// ==========================================================