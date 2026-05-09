using System;

public class FriendPopup : ListPopup
{
    private static FriendPopup instance;

    public static FriendPopup gI()
    {
        if (instance == null) instance = new FriendPopup();
        return instance;
    }

    public FriendPopup() : base("Danh sách Bạn bè", 0)
    {
    }
}
