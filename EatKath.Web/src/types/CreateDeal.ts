export interface CreateDeal {

    restaurantId: number;

    title: string;

    description: string;

    discountPercentage: number;

    offerType: number;

    promoImageUrl: string;

    termsAndConditions: string;

    startDate: string;

    endDate: string;

    startTime: string;

    endTime: string;

    maximumGuests: number;

    reservationLimit: number;

    dailyRedemptionLimit: number;

    isActive: boolean;
}


// ==========================================================
// CreateDeal.ts
// ==========================================================
//
// CreateDeal = 📋 Blueprint for data used to CREATE a Deal.
//
// Think:
//
// 👤 Owner fills in Create Deal form
//          ↓
// 📝 CreateDeal object
//          ↓
// 📞 DealService
//          ↓
// axios
//          ↓
// .NET API
//
// ----------------------------------------------------------
//
// IMPORTANT:
//
// This type does NOT send data to the API.
//
// It only describes what data should be included
// when creating a new Deal.
//
// ----------------------------------------------------------
//
// EXAMPLES:
//
// restaurantId
// → Which restaurant owns the deal.
//
// title
// → Deal title.
//
// discountPercentage
// → Discount amount.
//
// startDate / endDate
// → When the deal starts and ends.
//
// startTime / endTime
// → What time the deal is available.
//
// maximumGuests
// → Maximum number of guests.
//
// reservationLimit
// → How many reservations are allowed.
//
// dailyRedemptionLimit
// → Maximum redemptions allowed per day.
//
// isActive
// → Whether the deal is currently active.
//
// ----------------------------------------------------------
//
// IMPORTANT DIFFERENCE:
//
// CreateDeal
// → Data needed to CREATE a new deal.
//
// Deal
// → Data representing an EXISTING deal.
//
// ----------------------------------------------------------
//
// 🔑 Remember:
//
// Type = "What does this data look like?"
//
// CreateDeal = "What information do I need to send
//               when creating a Deal?"
//
// ==========================================================
//The next useful folder is utils/.

// ==========================================================
// EATKATH FRONTEND — CURRENT FLOW
//
// 1. index.html              ✅
// 2. main.tsx                ✅
// 3. App.tsx                 ✅
// 4. AppRoutes.tsx           ✅
// 5. MainLayout.tsx          ✅
// 6. Page                    ✅
// 7. Service                 ✅
// 8. axios.ts                ✅
// 9. .NET API                ⏭️ SKIP
// 10. Response               ✅
// 11. State update           ✅
// 12. React re-render        ✅
// 13. UI update              ✅
// 14. Components             ✅
// 15. AuthContext.tsx        ✅
// 16. AuthProvider.tsx       ✅
// 17. ProtectedRoute.tsx     ✅
// 18. LoginPage.tsx          ✅
// 19. AuthService.ts         ✅
// 20. Types                  ⏭️ SKIP remaining files
//       ↓
// 21. Utils                  ← NEXT
//
// ==========================================================