
//UserFavoriteService is the middleman between the Page and the API.
import api from "../api/axios"; //This brings in your configured Axios helper.// Think>Bring me the phone I use to call the backend.

import type { UserFavorite } from "../types/UserFavorite"; //This brings the description of the data. Tell me what a UserFavorite looks like

class UserFavoriteService { //Create a worker whose job is handling User Favorite requests. It has 3 jobs: getMyFavorites, addFavorite, removeFavorite.

    async getMyFavorites(): Promise<UserFavorite[]> { //Create a function called getMyFavorites that will eventually return a list of UserFavorite.

        const response =
            await api.get<UserFavorite[]>("/userfavorite");//Use Axios to call the backend /userfavorite endpoint and get my favourites.

        return response.data; //Return the list of UserFavorite returned by the API to the Page that called this function.

    }

    async add(restaurantId: number): Promise<void> { //Give me a restaurant ID and I'll add that restaurant to the user's favourites.

        await api.post(  //Send a POST request to /userfavorite with the restaurant ID
            "/userfavorite",
            {
                restaurantId
            }
        );

    }

    async remove(restaurantId: number): Promise<void> { //Give me a restaurant ID and I'll remove it from favourites.

        await api.delete(
            "/userfavorite",
            {
                data: {
                    restaurantId
                }
            }
        );

    }

}

export default new UserFavoriteService(); //It creates the service once and exports it. So another file can import it and use it without creating a new instance of the service. eg  UserFavoriteService.getMyFavorites()


// ==========================================================
// EATKATH FRONTEND FLOW
// ==========================================================
//
// 1. index.html              ✅
// 2. main.tsx                ✅
// 3. App.tsx                 ✅
// 4. AppRoutes.tsx           ✅
// 5. MainLayout.tsx          ✅
// 6. Page                    ✅ MyFavoritesPage
//       ↓
// 7. Service                 ✅ UserFavoriteService
//       ↓
// 8. axios.ts                ← NEXT
//       ↓
// 9. .NET API
//
// ==========================================================