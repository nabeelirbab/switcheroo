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
                Message = "You only have 30 right swipes/cash offers a day.\r\nYou have just used 1 and have 29 lefts. \r\nUse them wisely.";
            else if (currentSwipeNumber == 7)
                Message = "You have 23 right swipes/cash offers left.";
            else if (currentSwipeNumber > 29)
                Message = "You have used all your swipes/cash offers for today.\r\nCome back tomorrow and try again!";
        }
        public bool WasLocked { get; set; }
        public int RemainingSwipes { get; set; }
        public int CurrentSwipeNumber { get; set; }
        public string Message { get; set; }
    }
}
