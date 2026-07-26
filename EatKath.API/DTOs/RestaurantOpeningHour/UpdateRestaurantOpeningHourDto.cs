namespace EatKath.API.DTOs.RestaurantOpeningHour
{
    public class UpdateRestaurantOpeningHourDto
    {
        public DayOfWeek DayOfWeek { get; set; }

        public TimeOnly OpenTime { get; set; }

        public TimeOnly CloseTime { get; set; }

        public bool IsClosed { get; set; }
    }
}