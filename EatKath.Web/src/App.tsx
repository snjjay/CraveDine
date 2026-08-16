import AppRoutes from "./routes/AppRoutes";

function App() {
    return <AppRoutes />;
}

export default App;

// ==========================================================
// FRONTEND FLOW — STEP 3
// ==========================================================
//
// App.tsx = MAIN APPLICATION COMPONENT.
//
// Its job is very simple:
// → Hand control to AppRoutes.
//
// return <AppRoutes />
// → "App, don't decide which page to show.
//    Let AppRoutes decide."
//
// FLOW:
//
// 1. index.html
//       ↓
// 2. main.tsx
//       ↓
// 3. App.tsx
//       ↓
// 4. AppRoutes.tsx
//
// 🔑 Remember:
//
// index.html    = Entry door
// main.tsx      = Starts React
// App.tsx       = Hands control to routing
// AppRoutes.tsx = Decides which page to show
//
// ==========================================================