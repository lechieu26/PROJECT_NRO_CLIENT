using System;

public class EnemyPopup : ListPopup
{
    private static EnemyPopup instance;

    public static EnemyPopup gI()
    {
        if (instance == null) instance = new EnemyPopup();
        return instance;
    }

    public EnemyPopup() : base("Danh sách Kẻ thù", 1)
    {
    }
}
