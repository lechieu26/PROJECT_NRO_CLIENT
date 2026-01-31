using System;
using System.Collections.Generic;
using Assets.src.e;

public class SmallImage
{
	public static int[][] smallImg;

	public static SmallImage instance;

	public static Image[] imgbig;

	public static Small[] imgNew;

	public static MyVector vKeys = new MyVector();

	public static Image imgEmpty = null;

	public static sbyte[] newSmallVersion;

	public static int smallCount;

	public static short maxSmall;

	public static Dictionary<int, Image> imageRaw = new Dictionary<int, Image>();

	public SmallImage()
	{
		readImage();
	}

	public static void loadBigRMS()
	{
		if (imgbig == null)
		{
			imgbig = new Image[5]
			{
				GameCanvas.loadImageRMS("/img/Big0.png"),
				GameCanvas.loadImageRMS("/img/Big1.png"),
				GameCanvas.loadImageRMS("/img/Big2.png"),
				GameCanvas.loadImageRMS("/img/Big3.png"),
				GameCanvas.loadImageRMS("/img/Big4.png")
			};
		}
	}

	public static void loadBigImage()
	{
		imgEmpty = Image.createRGBImage(new int[1], 1, 1, bl: true);
	}

	public static void init()
	{
		instance = null;
		instance = new SmallImage();
	}

	public void readImage()
	{
		int num = 0;
		try
		{
			DataInputStream dataInputStream = new DataInputStream(Rms.loadRMS("NR_image"));
			short num2 = dataInputStream.readShort();
			smallImg = new int[num2][];
			for (int i = 0; i < smallImg.Length; i++)
			{
				smallImg[i] = new int[5];
			}
			for (int j = 0; j < num2; j++)
			{
				num++;
				smallImg[j][0] = dataInputStream.readUnsignedByte();
				smallImg[j][1] = dataInputStream.readShort();
				smallImg[j][2] = dataInputStream.readShort();
				smallImg[j][3] = dataInputStream.readShort();
				smallImg[j][4] = dataInputStream.readShort();
			}
		}
		catch (Exception ex)
		{
			Cout.LogError3("Loi readImage: " + ex.ToString() + "i= " + num);
		}
	}

	public static void clearHastable()
	{
	}

	public static void createImage(int id)
	{
		if (mGraphics.zoomLevel == 1)
		{
			// Thử load từ file local trước
			Image image = GameCanvas.loadImage("/SmallImage/Small" + id + ".png");
			if (image != null)
			{
				imgNew[id] = new Small(image, id);
				return;
			}
			// Thử load từ RMS (cache lâu dài)
			Image cachedImg = loadIconFromRMS(id);
			if (cachedImg != null)
			{
				imgNew[id] = new Small(cachedImg, id);
				return;
			}
			// Không có trong cache, yêu cầu từ server
			imgNew[id] = new Small(imgEmpty, id);
			Service.gI().requestIcon(id);
			return;
		}
		Image image2 = GameCanvas.loadImage("/SmallImage/Small" + id + ".png");
		if (image2 != null)
		{
			imgNew[id] = new Small(image2, id);
			return;
		}
		bool flag = false;
		if (imageRaw.ContainsKey(id))
		{
			Image img = null;
			imageRaw.TryGetValue(id, out img);
			if (img != null)
			{
				imgNew[id] = new Small(img, id);
			}
			else
			{
				flag = true;
			}
		}
		else
		{
			// Thử load từ RMS (cache lâu dài)
			Image cachedImg = loadIconFromRMS(id);
			if (cachedImg != null)
			{
				imgNew[id] = new Small(cachedImg, id);
				imageRaw[id] = cachedImg;
				return;
			}
			flag = true;
		}
		if (flag)
		{
			imgNew[id] = new Small(imgEmpty, id);
			Service.gI().requestIcon(id);
		}
	}

	/// <summary>
	/// Load icon từ RMS (bộ nhớ cache lâu dài)
	/// </summary>
	public static Image loadIconFromRMS(int id)
	{
		try
		{
			string rmsKey = mGraphics.zoomLevel + "icon" + id;
			sbyte[] data = Rms.loadRMS(rmsKey);
			if (data != null && data.Length > 0)
			{
				Image img = Image.createImage(data, 0, data.Length);
				return img;
			}
		}
		catch (System.Exception)
		{
			// Không có trong RMS hoặc lỗi
		}
		return null;
	}

	/// <summary>
	/// Lưu icon vào RMS để cache lâu dài
	/// </summary>
	public static void saveIconToRMS(int id, sbyte[] data)
	{
		try
		{
			string rmsKey = mGraphics.zoomLevel + "icon" + id;
			Rms.saveRMS(rmsKey, data);
		}
		catch (System.Exception ex)
		{
			Cout.println("Loi saveIconToRMS: " + ex.Message);
		}
	}

	public static void drawSmallImage(mGraphics g, int id, int x, int y, int transform, int anchor)
	{
		if (imgbig == null)
		{
			Small small = imgNew[id];
			if (small == null)
			{
				createImage(id);
			}
			else
			{
				g.drawRegion(small, 0, 0, mGraphics.getImageWidth(small.img), mGraphics.getImageHeight(small.img), transform, x, y, anchor);
				// Cập nhật thời gian sử dụng gần nhất cho LRU
				small.lastUsedTick = GameCanvas.gameTick;
			}
		}
		else if (smallImg != null)
		{
			if (id >= smallImg.Length || smallImg[id][1] >= 256 || smallImg[id][3] >= 256 || smallImg[id][2] >= 256 || smallImg[id][4] >= 256)
			{
				Small small2 = imgNew[id];
				if (small2 == null)
				{
					createImage(id);
				}
				else
				{
					small2.paint(g, transform, x, y, anchor);
				}
			}
			else if (imgbig[smallImg[id][0]] != null)
			{
				g.drawRegion(imgbig[smallImg[id][0]], smallImg[id][1], smallImg[id][2], smallImg[id][3], smallImg[id][4], transform, x, y, anchor);
			}
		}
		else if (GameCanvas.currentScreen != GameScr.gI())
		{
			Small small3 = imgNew[id];
			if (small3 == null)
			{
				createImage(id);
			}
			else
			{
				small3.paint(g, transform, x, y, anchor);
			}
		}
	}

	public static void drawSmallImage(mGraphics g, int id, int f, int x, int y, int w, int h, int transform, int anchor)
	{
		if (imgbig == null)
		{
			Small small = imgNew[id];
			if (small == null)
			{
				createImage(id);
			}
			else
			{
				g.drawRegion(small.img, 0, f * w, w, h, transform, x, y, anchor);
			}
		}
		else if (smallImg != null)
		{
			if (id >= smallImg.Length || smallImg[id] == null || smallImg[id][1] >= 256 || smallImg[id][3] >= 256 || smallImg[id][2] >= 256 || smallImg[id][4] >= 256)
			{
				Small small2 = imgNew[id];
				if (small2 == null)
				{
					createImage(id);
				}
				else
				{
					small2.paint(g, transform, f, x, y, w, h, anchor);
				}
			}
			else if (smallImg[id][0] != 4 && imgbig[smallImg[id][0]] != null)
			{
				g.drawRegion(imgbig[smallImg[id][0]], 0, f * w, w, h, transform, x, y, anchor);
			}
			else
			{
				Small small3 = imgNew[id];
				if (small3 == null)
				{
					createImage(id);
				}
				else
				{
					small3.paint(g, transform, f, x, y, w, h, anchor);
				}
			}
		}
		else if (GameCanvas.currentScreen != GameScr.gI())
		{
			Small small4 = imgNew[id];
			if (small4 == null)
			{
				createImage(id);
			}
			else
			{
				small4.paint(g, transform, f, x, y, w, h, anchor);
			}
		}
	}

	public static void update()
	{
		int num = 0;
		if (GameCanvas.gameTick % 1000 != 0)
		{
			return;
		}
		for (int i = 0; i < imgNew.Length; i++)
		{
			if (imgNew[i] != null)
			{
				num++;
				imgNew[i].update();
				smallCount++;
			}
		}
		// KHÔNG XÓA CACHE - Dữ liệu từ server sẽ được giữ vĩnh viễn
		// Chỉ xóa khi user chủ động xóa dữ liệu hoặc server update version
		// Logic xóa cache cũ đã được loại bỏ để tránh mất icon item
	}

	/// <summary>
	/// Lưu version icon hiện tại vào RMS
	/// </summary>
	public static void saveIconVersion()
	{
		try
		{
			if (newSmallVersion != null && newSmallVersion.Length > 0)
			{
				// Tạo hash từ version array
				int versionHash = 0;
				for (int i = 0; i < newSmallVersion.Length; i++)
				{
					versionHash = versionHash * 31 + newSmallVersion[i];
				}
				string rmsKey = mGraphics.zoomLevel + "iconVersion";
				Rms.saveRMSInt(rmsKey, versionHash);
			}
		}
		catch (System.Exception ex)
		{
			Cout.println("Loi saveIconVersion: " + ex.Message);
		}
	}

	/// <summary>
	/// Kiểm tra xem version có thay đổi không (chỉ true khi ĐÃ CÓ version cũ VÀ khác version mới)
	/// </summary>
	public static bool isVersionChanged()
	{
		try
		{
			if (newSmallVersion == null || newSmallVersion.Length == 0)
			{
				return false;
			}
			
			// Load version đã lưu
			string rmsKey = mGraphics.zoomLevel + "iconVersion";
			int savedVersionHash = Rms.loadRMSInt(rmsKey, -1);
			
			// Nếu chưa có version cũ (lần đầu chạy), KHÔNG xóa cache
			if (savedVersionHash == -1)
			{
				return false;
			}
			
			// Tính hash từ version array hiện tại
			int currentVersionHash = 0;
			for (int i = 0; i < newSmallVersion.Length; i++)
			{
				currentVersionHash = currentVersionHash * 31 + newSmallVersion[i];
			}
			
			// Chỉ trả về true nếu version thực sự thay đổi
			return currentVersionHash != savedVersionHash;
		}
		catch (System.Exception)
		{
			// Nếu lỗi, KHÔNG xóa cache
			return false;
		}
	}

	/// <summary>
	/// Xóa toàn bộ cache icon (CHỈ khi user yêu cầu hoặc version thay đổi)
	/// </summary>
	public static void clearAllIconCache()
	{
		try
		{
			// Reset imgNew array
			if (imgNew != null)
			{
				for (int i = 0; i < imgNew.Length; i++)
				{
					imgNew[i] = null;
				}
			}
			// Clear imageRaw dictionary
			imageRaw.Clear();
			
			Cout.println("SmallImage: Đã xóa toàn bộ cache icon");
		}
		catch (System.Exception ex)
		{
			Cout.println("Loi clearAllIconCache: " + ex.Message);
		}
	}

	/// <summary>
	/// Kiểm tra version và xóa cache nếu version thay đổi
	/// Gọi hàm này sau khi nhận được version từ server (case -77)
	/// </summary>
	public static void checkAndClearCacheIfVersionChanged()
	{
		// Chỉ xóa cache khi version THỰC SỰ thay đổi (không xóa lần đầu)
		if (isVersionChanged())
		{
			Cout.println("SmallImage: Version thay đổi, xóa cache để tải lại icon mới");
			clearAllIconCache();
		}
		// Lưu version mới
		saveIconVersion();
	}
}
