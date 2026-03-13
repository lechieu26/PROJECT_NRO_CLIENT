using System;

public class CookingSlot
{
    public ItemRecipe recipe;
    public long finishTime;
    public bool isLocked;
    
    public bool isCooking()
    {
        return recipe != null;
    }
}
