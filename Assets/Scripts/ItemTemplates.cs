public class ItemTemplates
{
	public static MyHashTable itemTemplates = new MyHashTable();
	
	// Item placeholder khi không tìm thấy item trong cache
	private static ItemTemplate missingItem = null;

	public static void add(ItemTemplate it)
	{
		if (itemTemplates == null)
		{
			itemTemplates = new MyHashTable();
		}
		itemTemplates.put(it.id, it);
	}

	public static ItemTemplate get(short id)
	{
		// Safety check for null hashtable
		if (itemTemplates == null)
		{
			UnityEngine.Debug.LogError("ItemTemplates.itemTemplates is NULL! Creating new instance.");
			itemTemplates = new MyHashTable();
		}
		
		ItemTemplate item = (ItemTemplate)itemTemplates.get(id);
		if (item == null)
		{
			Res.outz("WARNING: ItemTemplate not found for id=" + id + ". Total items: " + itemTemplates.size());
			// Trả về item placeholder nếu không tìm thấy
			if (missingItem == null)
			{
				// Constructor: (short templateID, sbyte type, sbyte gender, string name, string description, sbyte level, int strRequire, short iconID, short part, bool isUpToUp)
				missingItem = new ItemTemplate(-1, 99, 3, "???", "Item không tồn tại", 0, 0, 0, 0, false);
			}
			return missingItem;
		}
		return item;
	}
	
	public static bool exists(short id)
	{
		if (itemTemplates == null) return false;
		return itemTemplates.get(id) != null;
	}
	
	public static int count()
	{
		if (itemTemplates == null) return 0;
		return itemTemplates.size();
	}
}
