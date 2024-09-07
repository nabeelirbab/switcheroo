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
                Message = "You only have 10 right swipes/cash offers a day.\r\nYou have just used 1 and have 9 lefts. \r\nUse them wisely.";
            else if (currentSwipeNumber == 7)
                Message = "You have 3 right swipes/cash offers left.";
            else if (currentSwipeNumber > 9)
                Message = "You have used all your swipes/cash offers for today.\r\nCome back tomorrow and try again!";
        }
        public bool WasLocked { get; set; }
        public int RemainingSwipes { get; set; }
        public int CurrentSwipeNumber { get; set; }
        public string Message { get; set; }
    }
}
