
public class PawnInputAnalizer : PawnMovementController
{
    public virtual void OnClick()
    {
        print("On Click Pawn Called");
        CheckHome();
        if (!isLeftTheHouse)
            return;
        if (richedTheDestination)
            return;

        if (!EnoughSpotsLeft(DiceController.instance.currentDiceValue))
            return;
        //this will make the player move only one unit when dice value is 6 while exiting the house
        int steps = stepOutFromHome == 1 ? stepOutFromHome : DiceController.instance.currentDiceValue;

        if (stepOutFromHome == 1) stepOutFromHome = 0;

        if (DiceController.instance.playerCanMove)
        {
            StartCoroutine(MoveTo(steps));
            DiceController.instance.playerCanMove = false;
        }
    }
}
