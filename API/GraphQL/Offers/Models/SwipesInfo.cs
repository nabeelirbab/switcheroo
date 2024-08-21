namespace API.GraphQL.Offers.Models
{
    public class SwipesInfo
    {
        public SwipesInfo(int remainingSwipes = 9, int currentSwipeNumber = 1)
        {
            RemainingSwipes = remainingSwipes;
            CurrentSwipeNumber = currentSwipeNumber;
            Message = string.Empty;
            if (currentSwipeNumber == 1)
                Message = "You only have 10 right swipes a day. You have just used one and have 9 lefts. Use them wisely.";
            else if (currentSwipeNumber == 7)
                Message = "You have 3 right swipes left";
            else if (currentSwipeNumber > 9)
                Message = "You have used all your swipes for today. Come back tomorrow and try again.";
        }
        public int RemainingSwipes { get; set; }
        public int CurrentSwipeNumber { get; set; }
        public string Message { get; set; }
    }
}
