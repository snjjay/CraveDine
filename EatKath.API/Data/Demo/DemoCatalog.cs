namespace EatKath.API.Data.Demo
{
    // ==========================================================
    // Development/demo dataset definitions (see DemoDataSeeder).
    //
    // All restaurants here are SYNTHETIC demo listings for the Kathmandu
    // Valley: invented names, neighbourhood-level addresses, no phone,
    // email, website, ratings or reviews. Photos come from the local demo
    // image library (uploads/demo) and are illustrative, not photographs
    // of these restaurants. Prices are in NPR.
    // ==========================================================

    public sealed record DemoMenuItem(string Name, string Description, decimal Price);

    public sealed record DemoMenuCategory(string Name, DemoMenuItem[] Items);

    // Hours: daily open/close; Fri/Sat may close later.
    public sealed record DemoKitchen(
        string Key,
        DemoMenuCategory[] Menu,
        TimeOnly Opens,
        TimeOnly Closes,
        TimeOnly WeekendCloses,
        bool CafeStyle);

    public sealed record DemoRestaurant(
        string Name,
        string Area,
        string Address,
        string Description,
        string[] Cuisines,
        string Kitchen,
        string CoverCategory,
        string[] GalleryCategories);

    public static class DemoCatalog
    {
        public const string OwnerEmailPattern = "cdowner{0}@cd.com";

        private static DemoMenuItem I(string name, string description, decimal price) => new(name, description, price);

        private static DemoMenuCategory C(string name, params DemoMenuItem[] items) => new(name, items);

        private static TimeOnly T(int hour, int minute = 0) => new(hour, minute);

        private static readonly DemoMenuCategory Drinks = C("Drinks",
            I("Masala Chiya", "Spiced milk tea", 70),
            I("Sweet Lassi", "Chilled yoghurt drink", 160),
            I("Fresh Lime Soda", "Sweet or salted", 150));

        public static readonly Dictionary<string, DemoKitchen> Kitchens = new()
        {
            ["momo"] = new("momo", new[]
            {
                C("Momo",
                    I("Chicken Steamed Momo", "Ten pieces with tomato achar", 220),
                    I("Buff Steamed Momo", "Ten pieces with sesame achar", 200),
                    I("Veg Steamed Momo", "Cabbage, paneer and spring onion", 180),
                    I("Jhol Momo", "Momo in a warm spiced soup", 260)),
                C("Specials",
                    I("Kothey Momo", "Pan-fried on one side", 250),
                    I("C-Momo", "Tossed in a spicy chilli sauce", 280),
                    I("Fried Momo", "Crisp golden momo", 240)),
                C("Sides",
                    I("Aloo Sadeko", "Spiced potato salad", 180),
                    I("Chicken Sekuwa", "Charcoal-grilled skewers", 380)),
                Drinks
            }, T(10), T(21, 30), T(22), false),

            ["nepali"] = new("nepali", new[]
            {
                C("Dal Bhat Sets",
                    I("Veg Dal Bhat Set", "Rice, lentils, seasonal tarkari, saag and achar", 380),
                    I("Chicken Dal Bhat Set", "With home-style chicken curry", 520),
                    I("Mutton Dal Bhat Set", "With slow-cooked mutton curry", 680)),
                C("Snacks",
                    I("Sel Roti with Aloo Tarkari", "Rice-flour rings with potato curry", 160),
                    I("Chatpate", "Puffed rice with spices and lemon", 120),
                    I("Sukuti Sadeko", "Spiced dried meat salad", 360)),
                C("Curries",
                    I("Chicken Curry", "Nepali-style curry, served with rice", 450),
                    I("Aloo Tama Bodi", "Bamboo shoot, potato and bean curry", 290)),
                Drinks
            }, T(10), T(21, 30), T(22), false),

            ["thakali"] = new("thakali", new[]
            {
                C("Thakali Khana Sets",
                    I("Veg Thakali Set", "Dal, rice, gundruk, tarkari and achar", 450),
                    I("Chicken Thakali Set", "With local chicken curry", 620),
                    I("Mutton Thakali Set", "With mutton curry", 780)),
                C("Starters",
                    I("Gundruk Soup", "Fermented leafy-green soup", 150),
                    I("Kanchemba", "Crispy buckwheat fritters", 180),
                    I("Fried Fish", "Spiced river fish", 450)),
                C("Desserts",
                    I("Kheer", "Rice pudding with cardamom", 160)),
                Drinks
            }, T(10, 30), T(21, 30), T(22), false),

            ["newari"] = new("newari", new[]
            {
                C("Samay Baji",
                    I("Samay Baji Set", "Beaten rice with choila, bara, egg and achar", 450),
                    I("Veg Samay Baji", "With bara, aloo and achar", 380)),
                C("Bara & Chatamari",
                    I("Plain Bara", "Lentil pancake", 150),
                    I("Egg Bara", "Lentil pancake with egg", 200),
                    I("Chicken Chatamari", "Rice-flour crepe with minced chicken", 280)),
                C("Choila & Specials",
                    I("Buff Choila", "Grilled, spiced buff", 320),
                    I("Chicken Choila", "Smoky spiced chicken", 340),
                    I("Aalu Tama", "Potato and bamboo shoot curry", 260)),
                C("Sweets",
                    I("Yomari", "Steamed rice dumpling with chaku", 120),
                    I("Juju Dhau", "Creamy king curd", 140))
            }, T(10), T(21), T(21, 30), false),

            ["tibetan"] = new("tibetan", new[]
            {
                C("Noodle Soups",
                    I("Chicken Thukpa", "Hand-pulled noodles in broth", 280),
                    I("Veg Thukpa", "Seasonal vegetables and noodles", 240),
                    I("Thenthuk", "Hand-torn noodle soup", 280)),
                C("Breads & Pies",
                    I("Buff Shapale", "Fried pie with spiced filling", 250),
                    I("Tingmo with Curry", "Steamed bread with vegetable curry", 300)),
                C("Momo",
                    I("Steamed Momo", "Ten pieces with chilli sauce", 220),
                    I("Kothey Momo", "Pan-fried momo", 250)),
                C("Drinks",
                    I("Butter Tea", "Traditional salted tea", 120),
                    I("Masala Chiya", "Spiced milk tea", 70))
            }, T(10), T(21), T(21, 30), false),

            ["northindian"] = new("northindian", new[]
            {
                C("Starters",
                    I("Paneer Tikka", "Tandoor-roasted paneer", 480),
                    I("Chicken Tikka", "Tandoor-roasted chicken", 520)),
                C("Mains",
                    I("Butter Chicken", "Creamy tomato gravy", 620),
                    I("Dal Makhani", "Slow-cooked black lentils", 420),
                    I("Palak Paneer", "Spinach and cottage cheese", 460)),
                C("Breads & Rice",
                    I("Butter Naan", "Tandoor bread", 120),
                    I("Garlic Naan", "With garlic and coriander", 140),
                    I("Jeera Rice", "Cumin-tempered basmati", 220)),
                C("Desserts",
                    I("Gulab Jamun", "Two pieces in syrup", 160))
            }, T(11), T(22), T(22, 30), false),

            ["southindian"] = new("southindian", new[]
            {
                C("Dosa",
                    I("Masala Dosa", "With potato filling, sambar and chutney", 280),
                    I("Mysore Masala Dosa", "With red chutney", 320),
                    I("Paper Dosa", "Extra-thin and crisp", 300)),
                C("Idli & Vada",
                    I("Idli Sambar", "Three steamed rice cakes", 220),
                    I("Medu Vada", "Two lentil doughnuts", 220)),
                C("Uttapam & Thali",
                    I("Onion Uttapam", "Thick rice pancake", 300),
                    I("South Indian Thali", "Rice, sambar, rasam and sides", 520)),
                C("Drinks",
                    I("Filter Coffee", "South Indian style", 150),
                    I("Sweet Lassi", "Chilled yoghurt drink", 160))
            }, T(8), T(21), T(21, 30), false),

            ["mughlai"] = new("mughlai", new[]
            {
                C("Biryani",
                    I("Chicken Biryani", "Dum-cooked basmati with chicken", 550),
                    I("Mutton Biryani", "Dum-cooked basmati with mutton", 720),
                    I("Veg Biryani", "Vegetables and saffron rice", 450)),
                C("Kebabs",
                    I("Seekh Kebab", "Minced meat skewers", 520),
                    I("Chicken Malai Tikka", "Creamy marinated chicken", 560)),
                C("Curries",
                    I("Mutton Korma", "Rich nut-based gravy", 760),
                    I("Chicken Nihari", "Slow-cooked spiced stew", 690),
                    I("Rogan Josh", "Kashmiri-style mutton curry", 780)),
                C("Breads & Desserts",
                    I("Sheermal", "Saffron flatbread", 150),
                    I("Firni", "Ground-rice pudding", 180))
            }, T(11), T(22), T(22, 30), false),

            ["chinese"] = new("chinese", new[]
            {
                C("Starters",
                    I("Chilli Chicken", "Indo-Chinese style, dry or gravy", 450),
                    I("Crispy Chilli Potato", "Tossed in chilli-honey sauce", 320),
                    I("Steamed Dim Sum", "Six pieces, chicken or veg", 380)),
                C("Noodles & Rice",
                    I("Chicken Chowmein", "Wok-tossed noodles", 280),
                    I("Veg Hakka Noodles", "With seasonal vegetables", 260),
                    I("Chicken Fried Rice", "Egg-fried rice with chicken", 320)),
                C("Mains",
                    I("Veg Manchurian", "Vegetable balls in gravy", 380),
                    I("Kung Pao Chicken", "With peanuts and chillies", 520),
                    I("Sweet and Sour Fish", "Crisp fish, tangy sauce", 620)),
                C("Soups",
                    I("Hot and Sour Soup", "Chicken or veg", 220),
                    I("Sweet Corn Soup", "Chicken or veg", 200))
            }, T(11), T(22), T(22), false),

            ["japanese"] = new("japanese", new[]
            {
                C("Sushi",
                    I("Salmon Nigiri", "Four pieces", 680),
                    I("California Roll", "Eight pieces", 750),
                    I("Veg Maki", "Cucumber and avocado, eight pieces", 480)),
                C("Ramen",
                    I("Shoyu Ramen", "Soy broth, chicken chashu, egg", 720),
                    I("Miso Ramen", "Miso broth, vegetables, egg", 750)),
                C("Mains",
                    I("Chicken Katsu Curry", "Crumbed chicken, Japanese curry, rice", 780),
                    I("Teriyaki Chicken Bento", "With rice, salad and pickles", 850)),
                C("Sides",
                    I("Gyoza", "Six pan-fried dumplings", 450),
                    I("Edamame", "Lightly salted", 350))
            }, T(11, 30), T(21, 30), T(22), false),

            ["korean"] = new("korean", new[]
            {
                C("Rice & Stews",
                    I("Bibimbap", "Rice bowl with vegetables, egg and gochujang", 680),
                    I("Kimchi Jjigae", "Kimchi and tofu stew with rice", 650),
                    I("Bulgogi Set", "Marinated grilled chicken with rice", 850)),
                C("Street Food",
                    I("Tteokbokki", "Spicy rice cakes", 450),
                    I("Korean Fried Chicken", "Sweet-spicy glazed", 750),
                    I("Kimbap", "Seaweed rice rolls", 420)),
                C("Drinks",
                    I("Iced Barley Tea", "Roasted barley tea", 150),
                    I("Fresh Lime Soda", "Sweet or salted", 150))
            }, T(11, 30), T(21, 30), T(22), false),

            ["thai"] = new("thai", new[]
            {
                C("Curries",
                    I("Green Curry", "Chicken or veg, with jasmine rice", 620),
                    I("Red Curry", "Chicken or veg, with jasmine rice", 620),
                    I("Massaman Curry", "With potato and peanuts", 680)),
                C("Noodles & Rice",
                    I("Pad Thai", "Rice noodles, tamarind, peanuts", 580),
                    I("Pineapple Fried Rice", "With cashew nuts", 560)),
                C("Soups & Salads",
                    I("Tom Yum Soup", "Hot and sour lemongrass soup", 450),
                    I("Som Tam", "Green papaya salad", 380)),
                C("Desserts",
                    I("Mango Sticky Rice", "Seasonal", 380))
            }, T(11, 30), T(22), T(22), false),

            ["vietnamese"] = new("vietnamese", new[]
            {
                C("Pho",
                    I("Chicken Pho", "Rice noodles in aromatic broth", 580),
                    I("Veg Pho", "With tofu and herbs", 520)),
                C("Banh Mi & Rolls",
                    I("Chicken Banh Mi", "Baguette with pickles and herbs", 450),
                    I("Fresh Spring Rolls", "Rice-paper rolls, peanut sauce", 380),
                    I("Crispy Spring Rolls", "Four pieces", 360)),
                C("Rice Bowls",
                    I("Lemongrass Chicken Bowl", "With rice and salad", 560)),
                C("Drinks",
                    I("Vietnamese Iced Coffee", "With condensed milk", 220),
                    I("Fresh Lime Soda", "Sweet or salted", 150))
            }, T(11), T(21, 30), T(21, 30), false),

            ["italian"] = new("italian", new[]
            {
                C("Pizza",
                    I("Margherita", "Tomato, mozzarella, basil", 750),
                    I("Pepperoni", "Chicken pepperoni and mozzarella", 950),
                    I("Farmhouse Veg", "Peppers, mushroom, olives, onion", 850),
                    I("Chicken Tikka Pizza", "Tandoori chicken and onion", 980)),
                C("Pasta",
                    I("Spaghetti Aglio e Olio", "Garlic, chilli and olive oil", 650),
                    I("Penne Arrabbiata", "Spicy tomato sauce", 680),
                    I("Chicken Alfredo", "Creamy parmesan sauce", 820),
                    I("Lasagna", "Baked layers with ragu", 880)),
                C("Starters",
                    I("Garlic Bread", "With cheese", 320),
                    I("Bruschetta", "Tomato and basil", 380)),
                C("Desserts",
                    I("Tiramisu", "Coffee and mascarpone", 450))
            }, T(11), T(22), T(22, 30), false),

            ["burgers"] = new("burgers", new[]
            {
                C("Burgers",
                    I("Classic Chicken Burger", "Crispy chicken, lettuce, mayo", 450),
                    I("Buff Burger", "Grilled buff patty, cheese", 420),
                    I("Veg Crunch Burger", "Spiced veg patty", 380),
                    I("Double Cheese Burger", "Two patties, double cheese", 620)),
                C("Sides",
                    I("French Fries", "Salted", 200),
                    I("Peri Peri Fries", "With peri peri seasoning", 250),
                    I("Chicken Wings", "Six pieces, hot or BBQ", 520)),
                C("Diner Favourites",
                    I("Pancake Stack", "With honey and butter", 420),
                    I("Fried Chicken Basket", "Four pieces with fries", 780),
                    I("Hot Dog", "With mustard and onions", 350)),
                C("Shakes",
                    I("Chocolate Shake", "Thick and creamy", 320),
                    I("Oreo Shake", "Cookies and cream", 340))
            }, T(11), T(22), T(22, 30), false),

            ["cafe"] = new("cafe", new[]
            {
                C("Coffee",
                    I("Espresso", "Single shot", 150),
                    I("Cappuccino", "With steamed milk", 220),
                    I("Cafe Latte", "Hot or iced", 240),
                    I("Iced Americano", "Over ice", 220)),
                C("Tea",
                    I("Masala Chiya", "Spiced milk tea", 80),
                    I("Himalayan Green Tea", "Pot for one", 150)),
                C("Breakfast",
                    I("Avocado Toast", "Sourdough, avocado, egg", 520),
                    I("English Breakfast", "Eggs, sausage, beans, toast", 650),
                    I("Pancakes", "With honey or chocolate", 420)),
                C("Sandwiches",
                    I("Club Sandwich", "Chicken, egg, lettuce, tomato", 480),
                    I("Chicken Panini", "Grilled with cheese", 520))
            }, T(7), T(20), T(20, 30), true),

            ["chiya"] = new("chiya", new[]
            {
                C("Chiya & Coffee",
                    I("Masala Chiya", "Spiced milk tea", 60),
                    I("Black Chiya", "With lemon and ginger", 50),
                    I("Milk Coffee", "Hot", 120)),
                C("Nepali Snacks",
                    I("Sel Roti", "Two rice-flour rings", 100),
                    I("Aloo Chop", "Spiced potato fritters", 120),
                    I("Samosa", "Two pieces with chutney", 100),
                    I("Chatpate", "Puffed rice with spices", 120)),
                C("Light Meals",
                    I("Veg Momo", "Ten pieces", 170),
                    I("Chicken Chowmein", "Wok-tossed noodles", 220))
            }, T(7), T(20), T(20), true),

            ["bakery"] = new("bakery", new[]
            {
                C("Breads & Pastries",
                    I("Butter Croissant", "Freshly baked", 180),
                    I("Pain au Chocolat", "Chocolate-filled pastry", 220),
                    I("Cinnamon Roll", "With sugar glaze", 200),
                    I("Banana Bread", "Thick slice", 180)),
                C("Cakes",
                    I("Black Forest Slice", "Chocolate, cream and cherries", 250),
                    I("Baked Cheesecake", "Classic", 380),
                    I("Chocolate Truffle Cake", "Rich slice", 320),
                    I("Red Velvet Slice", "With cream cheese frosting", 300)),
                C("Desserts",
                    I("Brownie with Ice Cream", "Warm brownie, vanilla scoop", 350),
                    I("Rasbari", "Two pieces", 120)),
                C("Drinks",
                    I("Hot Chocolate", "With cream", 260),
                    I("Cappuccino", "With steamed milk", 220))
            }, T(7), T(21), T(21), true),

            ["middleeast"] = new("middleeast", new[]
            {
                C("Mezze",
                    I("Hummus with Pita", "Chickpea dip, olive oil", 420),
                    I("Falafel Plate", "With tahini and salad", 480),
                    I("Baba Ganoush", "Smoky aubergine dip", 420)),
                C("Grills",
                    I("Chicken Shawarma Plate", "With rice, salad and garlic sauce", 620),
                    I("Adana Kebab", "Spiced minced-meat kebab", 750),
                    I("Chicken Shish Tawook", "Marinated chicken skewers", 680)),
                C("Wraps",
                    I("Chicken Shawarma Wrap", "With pickles and garlic sauce", 380),
                    I("Falafel Wrap", "With tahini", 340)),
                C("Desserts & Tea",
                    I("Baklava", "Three pieces", 320),
                    I("Turkish Tea", "Black tea", 120))
            }, T(11), T(22), T(22, 30), false),

            ["french"] = new("french", new[]
            {
                C("Starters",
                    I("French Onion Soup", "With cheese crouton", 420),
                    I("Quiche Lorraine", "With green salad", 520)),
                C("Mains",
                    I("Steak Frites", "Grilled steak, fries, pepper sauce", 1250),
                    I("Chicken Chasseur", "Mushroom and tomato sauce", 880),
                    I("Ratatouille", "Provençal vegetable stew", 650)),
                C("Desserts",
                    I("Crème Brûlée", "Vanilla custard", 420),
                    I("Crêpe Suzette", "Orange butter sauce", 450)),
                C("Bakery",
                    I("Butter Croissant", "Freshly baked", 180),
                    I("Cafe au Lait", "Coffee with hot milk", 220))
            }, T(8), T(22), T(22), false),

            ["continental"] = new("continental", new[]
            {
                C("Starters",
                    I("Cream of Mushroom Soup", "With garlic bread", 280),
                    I("Caesar Salad", "With grilled chicken", 480)),
                C("Mains",
                    I("Grilled Chicken Steak", "Mashed potato, vegetables", 850),
                    I("Fish Fillet Lemon Butter", "Herbed rice", 950),
                    I("Pasta Primavera", "Seasonal vegetables", 650)),
                C("Desserts",
                    I("Apple Pie", "With vanilla ice cream", 350),
                    I("Chocolate Mousse", "Rich and airy", 320))
            }, T(11), T(22), T(22), false),

            ["mexican"] = new("mexican", new[]
            {
                C("Tacos",
                    I("Chicken Tacos", "Three soft tacos, salsa, lime", 520),
                    I("Veg Tacos", "Beans, peppers, salsa", 450)),
                C("Burritos & Quesadillas",
                    I("Chicken Burrito", "Rice, beans, cheese, salsa", 580),
                    I("Bean Burrito", "Rice, beans, cheese", 480),
                    I("Cheese Quesadilla", "With sour cream", 520)),
                C("Sharing",
                    I("Loaded Nachos", "Cheese, beans, jalapeños, salsa", 520),
                    I("Guacamole and Chips", "Fresh avocado dip", 450)),
                C("Desserts",
                    I("Churros", "With chocolate sauce", 320))
            }, T(11, 30), T(22), T(22, 30), false),

            ["seafood"] = new("seafood", new[]
            {
                C("Grills",
                    I("Grilled Trout", "Whole trout with herbs", 950),
                    I("Chicken Sekuwa Platter", "With beaten rice and achar", 650),
                    I("BBQ Pork Ribs", "Smoky glaze", 1100)),
                C("Seafood",
                    I("Butter Garlic Prawns", "With garlic rice", 1200),
                    I("Fish and Chips", "Beer-battered fish", 850),
                    I("Nepali Fish Curry", "With rice", 750)),
                C("Sides",
                    I("Grilled Vegetables", "Seasonal", 380),
                    I("Garlic Rice", "Fried rice with garlic", 250))
            }, T(12), T(22, 30), T(23), false),

            ["grill"] = new("grill", new[]
            {
                C("Sekuwa & Grills",
                    I("Chicken Sekuwa", "Charcoal-grilled with spices", 380),
                    I("Buff Sekuwa", "Charcoal-grilled with timur", 360),
                    I("Mutton Sekuwa", "Charcoal-grilled with spices", 520),
                    I("Grilled Fish", "Local fish, lemon and chilli", 550)),
                C("Sides",
                    I("Beaten Rice (Chiura)", "With achar", 100),
                    I("Aloo Sadeko", "Spiced potato salad", 180),
                    I("Bhatmas Sadeko", "Roasted soybean salad", 160)),
                C("Sets",
                    I("Sekuwa Set", "Sekuwa, chiura, achar and salad", 550),
                    I("Veg Dal Bhat Set", "Rice, lentils, tarkari, achar", 380)),
                Drinks
            }, T(12), T(22, 30), T(23), false),

            ["vegan"] = new("vegan", new[]
            {
                C("Bowls & Salads",
                    I("Buddha Bowl", "Grains, roasted vegetables, tahini", 550),
                    I("Quinoa Salad", "Herbs, cucumber, lemon", 520),
                    I("Tofu Stir Fry", "With brown rice", 480)),
                C("Mains",
                    I("Vegan Dal Bhat Set", "Rice, lentils, tarkari, saag", 420),
                    I("Jackfruit Curry", "With rice", 520)),
                C("Drinks",
                    I("Cold-Pressed Juice", "Seasonal fruits and greens", 280),
                    I("Fresh Lime Soda", "Sweet or salted", 150)),
                C("Desserts",
                    I("Vegan Chocolate Cake", "Dairy-free slice", 300))
            }, T(8), T(21), T(21), false)
        };

        // 48 demo restaurants: at least 2 per Area, every catalogue cuisine
        // used at least once.
        public static readonly DemoRestaurant[] Restaurants =
        {
            new("Himal Momo Kitchen", "Thamel", "Chaksibari Marg, Thamel, Kathmandu",
                "Steamed, jhol and kothey momo made to order, with house tomato and sesame achar.",
                new[] { "Momo & Dumplings", "Nepali", "Tibetan" }, "momo", "momo", new[] { "momo", "tibetan", "interior" }),
            new("Muktinath Thakali Bhansa", "Thamel", "J.P. Marg, Thamel, Kathmandu",
                "Thakali khana sets with buckwheat, gundruk and slow-cooked curries in a relaxed dining room.",
                new[] { "Thakali", "Nepali" }, "thakali", "dalbhat", new[] { "dalbhat", "nepali", "interior" }),
            new("Thamel Wood-Fire Pizzeria", "Thamel", "Saatghumti Marg, Thamel, Kathmandu",
                "Thin-crust pizzas from a wood-fired oven, plus fresh pasta and simple Italian desserts.",
                new[] { "Pizza", "Italian" }, "italian", "pizza", new[] { "pizza", "italian", "interior" }),

            new("Kantipath Dosa Corner", "Kathmandu", "Kantipath, Kathmandu",
                "Crisp dosas, idli and uttapam with sambar and fresh chutneys, served from breakfast.",
                new[] { "South Indian", "Indian" }, "southindian", "southindian", new[] { "southindian", "southindian", "indian" }),
            new("Durbar Marg Tandoor House", "Kathmandu", "Durbar Marg, Kathmandu",
                "North Indian curries, kebabs and breads from the tandoor.",
                new[] { "North Indian", "Mughlai" }, "northindian", "indian", new[] { "indian", "mughlai", "interior" }),
            new("Basantapur Newa Bhoye", "Kathmandu", "Basantapur, Kathmandu",
                "Samay baji, bara, chatamari and choila in the style of a Newari feast.",
                new[] { "Newari", "Nepali" }, "newari", "newari", new[] { "newari", "newari", "nepali" }),
            new("Asan Bakery & Cafe", "Kathmandu", "Asan Tole, Kathmandu",
                "Morning croissants, cakes and coffee in the heart of the old bazaar.",
                new[] { "Bakery", "Cafe" }, "bakery", "bakery", new[] { "bakery", "cafe", "desserts" }),

            new("Patan Courtyard Cafe", "Lalitpur", "Mangal Bazaar, Patan, Lalitpur",
                "All-day breakfasts, sandwiches and espresso around a quiet courtyard.",
                new[] { "Cafe", "Continental" }, "cafe", "cafe", new[] { "cafe", "bakery", "interior" }),
            new("Pulchowk Sushi & Ramen", "Lalitpur", "Pulchowk Road, Lalitpur",
                "Sushi rolls, nigiri and bowls of ramen with Japanese-style sides.",
                new[] { "Japanese" }, "japanese", "japanese", new[] { "japanese", "japanese", "interior" }),
            new("Kumaripati Seoul Kitchen", "Lalitpur", "Kumaripati, Lalitpur",
                "Bibimbap, kimchi stew and Korean fried chicken.",
                new[] { "Korean" }, "korean", "korean", new[] { "korean", "korean", "bbq" }),
            new("Mangal Bazaar Newa Kitchen", "Lalitpur", "Near Mangal Bazaar, Patan, Lalitpur",
                "Newari snacks and sets alongside steamed momo.",
                new[] { "Newari", "Momo & Dumplings" }, "newari", "newari", new[] { "newari", "momo", "nepali" }),

            new("Taumadhi Dhau & Bara Corner", "Bhaktapur", "Near Taumadhi Square, Bhaktapur",
                "Bhaktapur-style juju dhau, yomari and freshly fried bara.",
                new[] { "Newari", "Desserts" }, "newari", "newari", new[] { "newari", "desserts", "nepali" }),
            new("Durbar Square Bhojanalaya", "Bhaktapur", "Near Bhaktapur Durbar Square, Bhaktapur",
                "Home-style dal bhat and Thakali sets for hungry sightseers.",
                new[] { "Nepali", "Thakali" }, "nepali", "dalbhat", new[] { "dalbhat", "nepali", "newari" }),
            new("Suryabinayak Burger Shack", "Bhaktapur", "Suryabinayak, Bhaktapur",
                "Burgers, fries and shakes made to order.",
                new[] { "Burgers", "American", "Fast Food" }, "burgers", "burgers", new[] { "burgers", "american", "interior" }),
            new("Thimi Chiya Pasal", "Bhaktapur", "Madhyapur Thimi, Bhaktapur",
                "Masala chiya, sel roti and Nepali snacks from early morning.",
                new[] { "Nepali", "Cafe" }, "chiya", "nepali", new[] { "nepali", "cafe", "momo" }),

            new("Shankhamul Biryani House", "New Baneshwor", "Shankhamul Road, New Baneshwor, Kathmandu",
                "Dum biryani, kebabs and rich curries in the Mughlai tradition.",
                new[] { "Mughlai", "Pakistani" }, "mughlai", "mughlai", new[] { "pakistani", "mughlai", "indian" }),
            new("Bijulibazar Wok", "New Baneshwor", "Bijulibazar, New Baneshwor, Kathmandu",
                "Indo-Chinese favourites: chilli chicken, chowmein and fried rice.",
                new[] { "Indo-Chinese", "Chinese" }, "chinese", "indochinese", new[] { "chinese", "indochinese", "interior" }),

            new("Old Baneshwor Thai Orchid", "Baneshwor", "Old Baneshwor, Kathmandu",
                "Thai curries, pad thai and tom yum.",
                new[] { "Thai" }, "thai", "thai", new[] { "thai", "thai", "interior" }),
            new("Baneshwor Momo Adda", "Baneshwor", "Baneshwor Chowk, Kathmandu",
                "Quick momo, chowmein and snacks.",
                new[] { "Momo & Dumplings", "Fast Food" }, "momo", "momo", new[] { "momo", "momo", "indochinese" }),

            new("Kalanki Sekuwa Ghar", "Kalanki", "Kalanki Chowk, Kathmandu",
                "Charcoal-grilled sekuwa with chiura, achar and sadeko.",
                new[] { "BBQ", "Nepali" }, "grill", "bbq", new[] { "bbq", "nepali", "dalbhat" }),
            new("Kalanki Family Bhojan", "Kalanki", "Ring Road, Kalanki, Kathmandu",
                "Generous dal bhat sets and Nepali curries.",
                new[] { "Nepali" }, "nepali", "dalbhat", new[] { "dalbhat", "nepali", "momo" }),

            new("Koteshwor Pho House", "Koteshwor", "Koteshwor, Kathmandu",
                "Bowls of pho, banh mi and fresh spring rolls.",
                new[] { "Vietnamese" }, "vietnamese", "vietnamese", new[] { "vietnamese", "vietnamese", "interior" }),
            new("Koteshwor Kebab & Grill", "Koteshwor", "Near Koteshwor Chowk, Kathmandu",
                "Shawarma, kebabs and mezze with Turkish tea.",
                new[] { "Middle Eastern", "Turkish" }, "middleeast", "middleeast", new[] { "middleeast", "bbq", "middleeast" }),

            new("Stupa View Rooftop Cafe", "Boudha", "Boudha Stupa area, Kathmandu",
                "Coffee, breakfasts and light meals with a relaxed rooftop feel.",
                new[] { "Cafe", "Tibetan" }, "cafe", "cafe", new[] { "cafe", "tibetan", "bakery" }),
            new("Phulbari Himalayan Kitchen", "Boudha", "Phulbari, Boudha, Kathmandu",
                "Himalayan and Tibetan dishes: thukpa, shapale and tingmo.",
                new[] { "Himalayan", "Tibetan" }, "tibetan", "tibetan", new[] { "tibetan", "momo", "interior" }),
            new("Boudha Thukpa Ghar", "Boudha", "Boudha, Kathmandu",
                "Steaming bowls of thukpa and plates of momo.",
                new[] { "Tibetan", "Momo & Dumplings" }, "tibetan", "tibetan", new[] { "tibetan", "momo", "tibetan" }),

            new("Lazimpat Petit Bistro", "Lazimpat", "Lazimpat Road, Kathmandu",
                "French bistro dishes, pastries and coffee.",
                new[] { "French", "Continental" }, "french", "french", new[] { "french", "bakery", "interior" }),
            new("Lazimpat Mezze Garden", "Lazimpat", "Off Lazimpat Road, Kathmandu",
                "Mediterranean and Middle Eastern mezze, grills and salads.",
                new[] { "Mediterranean", "Middle Eastern" }, "middleeast", "middleeast", new[] { "middleeast", "vegan", "desserts" }),

            new("Chabahil Fish & Grill", "Chabahil", "Chabahil Chowk, Kathmandu",
                "Grilled trout, prawns and smoky grills.",
                new[] { "Seafood", "BBQ" }, "seafood", "seafood", new[] { "seafood", "bbq", "interior" }),
            new("Chabahil Chowmein Corner", "Chabahil", "Chabahil, Kathmandu",
                "Fast Indo-Chinese plates: chowmein, chilli chicken and fried rice.",
                new[] { "Indo-Chinese", "Fast Food" }, "chinese", "indochinese", new[] { "indochinese", "chinese", "momo" }),

            new("Maharajgunj Garden Kitchen", "Maharajgunj", "Maharajgunj Road, Kathmandu",
                "Continental and Italian plates in a garden setting.",
                new[] { "Continental", "Italian" }, "continental", "italian", new[] { "italian", "french", "interior" }),
            new("Maharajgunj Taco Cantina", "Maharajgunj", "Maharajgunj, Kathmandu",
                "Tacos, burritos and nachos with fresh salsa.",
                new[] { "Mexican" }, "mexican", "mexican", new[] { "mexican", "mexican", "interior" }),

            new("Balaju Green Bowl", "Balaju", "Balaju, Kathmandu",
                "Plant-based bowls, salads and a vegan dal bhat set.",
                new[] { "Vegan", "Mediterranean" }, "vegan", "vegan", new[] { "vegan", "middleeast", "vegan" }),
            new("Balaju Tandoori Nights", "Balaju", "Balaju Chowk, Kathmandu",
                "Tandoori grills, curries and naan.",
                new[] { "North Indian", "Indian" }, "northindian", "indian", new[] { "indian", "mughlai", "desserts" }),

            new("Kirtipur Hilltop Bhojan", "Kirtipur", "Naya Bazaar, Kirtipur",
                "Newari and Nepali dishes in the old hill town.",
                new[] { "Newari", "Nepali" }, "newari", "newari", new[] { "newari", "dalbhat", "nepali" }),
            new("Kirtipur Student Cafe", "Kirtipur", "Near the university gate, Kirtipur",
                "Coffee, burgers and snacks at student-friendly prices.",
                new[] { "Cafe", "Fast Food" }, "cafe", "cafe", new[] { "cafe", "burgers", "bakery" }),

            new("Jawalakhel Pasta House", "Jawalakhel", "Jawalakhel Chowk, Lalitpur",
                "Fresh pasta, pizzas and tiramisu.",
                new[] { "Italian", "Pizza" }, "italian", "italian", new[] { "italian", "pizza", "desserts" }),
            new("Jawalakhel Seoul Street", "Jawalakhel", "Jawalakhel, Lalitpur",
                "Korean street food with a few Japanese favourites.",
                new[] { "Korean", "Japanese" }, "korean", "korean", new[] { "korean", "japanese", "korean" }),

            new("Putalisadak Dosa Express", "Putalisadak", "Putalisadak, Kathmandu",
                "Quick South Indian breakfasts and thalis.",
                new[] { "South Indian", "Indian" }, "southindian", "southindian", new[] { "southindian", "southindian", "indian" }),
            new("Putalisadak Burger Lab", "Putalisadak", "Off Putalisadak Road, Kathmandu",
                "Stacked burgers, loaded fries and thick shakes.",
                new[] { "Burgers", "American" }, "burgers", "burgers", new[] { "burgers", "burgers", "american" }),

            new("Teku Karachi Grill", "Teku", "Teku Road, Kathmandu",
                "Pakistani-style karahi, nihari and kebabs.",
                new[] { "Pakistani", "Mughlai", "BBQ" }, "mughlai", "pakistani", new[] { "pakistani", "bbq", "mughlai" }),
            new("Teku Dragon Garden", "Teku", "Teku, Kathmandu",
                "Dim sum, noodles and classic Chinese mains.",
                new[] { "Chinese" }, "chinese", "chinese", new[] { "chinese", "chinese", "indochinese" }),

            new("Tripureshwor Thakali Kitchen", "Tripureshwor", "Tripureshwor, Kathmandu",
                "Thakali sets with dal, gundruk and local curries.",
                new[] { "Thakali", "Nepali" }, "thakali", "dalbhat", new[] { "dalbhat", "nepali", "interior" }),
            new("Tripureshwor Mithai & Cake House", "Tripureshwor", "Near Tripureshwor Chowk, Kathmandu",
                "Cakes, pastries and traditional sweets.",
                new[] { "Desserts", "Bakery" }, "bakery", "desserts", new[] { "desserts", "bakery", "cafe" }),

            new("Gongabu Bus Park Momo", "Gongabu", "New Bus Park area, Gongabu, Kathmandu",
                "Fast, hot momo for travellers and locals.",
                new[] { "Momo & Dumplings", "Nepali" }, "momo", "momo", new[] { "momo", "momo", "nepali" }),
            new("Gongabu Biryani & Kebab", "Gongabu", "Gongabu, Kathmandu",
                "Biryani, kebabs and North Indian curries.",
                new[] { "Mughlai", "North Indian" }, "mughlai", "mughlai", new[] { "mughlai", "pakistani", "indian" }),

            new("Sinamangal Runway Diner", "Sinamangal", "Sinamangal, near Ring Road, Kathmandu",
                "All-day diner breakfasts, burgers and fried chicken.",
                new[] { "American", "Continental", "Burgers" }, "burgers", "american", new[] { "american", "burgers", "desserts" }),
            new("Sinamangal Thai & Sushi", "Sinamangal", "Sinamangal, Kathmandu",
                "Thai curries and noodles with sushi rolls.",
                new[] { "Thai", "Japanese" }, "thai", "thai", new[] { "thai", "japanese", "interior" })
        };
    }
}
