using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Renderer cho map kieu 7VNR.
/// Quan ly background layers, ground collision (polyline), camera bounds.
/// Thay the tile-based rendering khi TileMap.isMap7VNR == true.
/// </summary>
public class NewMapRenderer
{
	private static NewMapRenderer instance;

	public static NewMapRenderer gI()
	{
		if (instance == null)
			instance = new NewMapRenderer();
		return instance;
	}

	public int mapWidth;
	public int mapHeight;
	public string[] bgLayers;
	public List<GroundPoint> groundPoints = new List<GroundPoint>();
	public List<PlatformSegment> platforms = new List<PlatformSegment>();
	public int camTop, camBottom, camLeft, camRight;

	public bool isLoaded = false;
	private Image[] bgLayerImages;

	private List<DecorationInstance> decorations = new List<DecorationInstance>();
	private Dictionary<string, Image> decorationImageCache = new Dictionary<string, Image>();

	/// <summary>
	/// Parse map config JSON tu server va load assets.
	/// </summary>
	public void loadMap7VNR(string configJson)
	{
		isLoaded = false;
		groundPoints.Clear();
		platforms.Clear();
		decorations.Clear();
		decorationImageCache.Clear();
		bgLayerImages = null;

		try
		{
			MapConfig config = JsonUtility.FromJson<MapConfig>(configJson);
			if (config == null)
				return;

			mapWidth = config.width;
			mapHeight = config.height;
			bgLayers = config.bgLayers;

			if (config.groundPoints != null)
			{
				for (int i = 0; i < config.groundPoints.Length; i++)
				{
					groundPoints.Add(new GroundPoint
					{
						x = config.groundPoints[i].x,
						y = config.groundPoints[i].y
					});
				}
			}

			if (config.platforms != null)
			{
				for (int i = 0; i < config.platforms.Length; i++)
				{
					platforms.Add(new PlatformSegment
					{
						x1 = config.platforms[i].x1,
						y1 = config.platforms[i].y1,
						x2 = config.platforms[i].x2,
						y2 = config.platforms[i].y2
					});
				}
			}

			if (config.cameraBounds != null)
			{
				camTop = config.cameraBounds.top;
				camBottom = config.cameraBounds.bottom;
				camLeft = config.cameraBounds.left;
				camRight = config.cameraBounds.right;
			}

			TileMap.pxw = mapWidth;
			TileMap.pxh = mapHeight;
			TileMap.tmw = mapWidth / TileMap.size + 1;
			TileMap.tmh = mapHeight / TileMap.size + 1;
			TileMap.maps = new int[TileMap.tmw * TileMap.tmh];
			TileMap.types = new int[TileMap.tmw * TileMap.tmh];

			if (config.decorations != null)
			{
				for (int i = 0; i < config.decorations.Length; i++)
				{
					DecorationConfig dc = config.decorations[i];
					decorations.Add(new DecorationInstance
					{
						imageName = dc.image,
						x = dc.x,
						y = dc.y,
						width = dc.width,
						height = dc.height,
						order = dc.order,
						flipX = dc.flipX,
						flipY = dc.flipY,
						layer = dc.layer
					});
				}
				decorations.Sort((a, b) => a.order.CompareTo(b.order));
			}

			loadBackgroundImages();
			loadDecorationImages();
			isLoaded = true;
		}
		catch (Exception)
		{
		}
	}

	private void loadBackgroundImages()
	{
		if (bgLayers == null || bgLayers.Length == 0)
		{
			bgLayerImages = new Image[0];
			return;
		}
		bgLayerImages = new Image[bgLayers.Length];
		for (int i = 0; i < bgLayers.Length; i++)
		{
			try
			{
				bgLayerImages[i] = GameCanvas.loadImage("/newmap/" + bgLayers[i]);
			}
			catch (Exception)
			{
				bgLayerImages[i] = null;
			}
		}
	}

	/// <summary>
	/// Ve map 7VNR (background layers voi parallax).
	/// Goi thay cho GameCanvas.paintBGGameScr + TileMap.paintTilemap.
	/// </summary>
	public void paintMap(mGraphics g)
	{
		if (!isLoaded)
			return;

		paintBackgroundLayers(g);
		paintDecorations(g);
	}

	private void paintBackgroundLayers(mGraphics g)
	{
		if (bgLayerImages == null)
			return;
		for (int i = 0; i < bgLayerImages.Length; i++)
		{
			if (bgLayerImages[i] != null)
			{
				int parallaxFactor = bgLayerImages.Length - i;
				if (parallaxFactor < 1) parallaxFactor = 1;
				int offsetX = -(GameScr.cmx / parallaxFactor);
				int offsetY = -(GameScr.cmy / parallaxFactor);
				g.drawImage(bgLayerImages[i], offsetX, offsetY, 0);
			}
		}
	}

	/// <summary>
	/// Tim Y ground tai vi tri px (pixel X).
	/// Dung linear interpolation tren polyline.
	/// </summary>
	public int getGroundY(int px)
	{
		if (groundPoints.Count < 2)
			return mapHeight;

		for (int i = 0; i < groundPoints.Count - 1; i++)
		{
			if (px >= groundPoints[i].x && px <= groundPoints[i + 1].x)
			{
				int dx = groundPoints[i + 1].x - groundPoints[i].x;
				if (dx == 0)
					return groundPoints[i].y;
				float t = (float)(px - groundPoints[i].x) / dx;
				return (int)(groundPoints[i].y + t * (groundPoints[i + 1].y - groundPoints[i].y));
			}
		}

		if (px < groundPoints[0].x)
			return groundPoints[0].y;
		return groundPoints[groundPoints.Count - 1].y;
	}

	/// <summary>
	/// Tim Y ground gan nhat phia duoi tu vi tri (px, startPy).
	/// Tuong duong yPhysicInTop tren server.
	/// </summary>
	public int findGroundBelow(int px, int startPy)
	{
		int groundY = getGroundY(px);
		if (startPy <= groundY)
			return groundY;
		return startPy;
	}

	private void loadDecorationImages()
	{
		foreach (DecorationInstance deco in decorations)
		{
			if (string.IsNullOrEmpty(deco.imageName) || decorationImageCache.ContainsKey(deco.imageName))
				continue;
			try
			{
				Image img = GameCanvas.loadImage("/newmap/" + deco.imageName);
				decorationImageCache[deco.imageName] = img;
			}
			catch (Exception)
			{
				decorationImageCache[deco.imageName] = null;
			}
		}
	}

	private void paintDecorations(mGraphics g)
	{
		int screenW = GameCanvas.w;
		int screenH = GameCanvas.h;

		foreach (DecorationInstance deco in decorations)
		{
			Image img;
			if (!decorationImageCache.TryGetValue(deco.imageName, out img) || img == null)
				continue;

			int drawX = deco.x - GameScr.cmx;
			int drawY = deco.y - GameScr.cmy;

			if (drawX + deco.width < 0 || drawX > screenW ||
				drawY + deco.height < 0 || drawY > screenH)
				continue;

			if (deco.flipX || deco.flipY)
			{
				int trans = 0;
				if (deco.flipX) trans = mGraphics.TRANS_MIRROR;
				g.drawRegion(img, 0, 0, deco.width, deco.height,
					trans, drawX, drawY, 0);
			}
			else
			{
				g.drawImage(img, drawX, drawY, 0);
			}
		}
	}

	public void reset()
	{
		isLoaded = false;
		groundPoints.Clear();
		platforms.Clear();
		decorations.Clear();
		decorationImageCache.Clear();
		bgLayerImages = null;
		mapWidth = 0;
		mapHeight = 0;
	}

	public struct GroundPoint
	{
		public int x, y;
	}

	public struct PlatformSegment
	{
		public int x1, y1, x2, y2;
	}

	public class DecorationInstance
	{
		public string imageName;
		public int x, y;
		public int width, height;
		public int order;
		public bool flipX, flipY;
		public string layer;
	}

	[Serializable]
	public class MapConfig
	{
		public int width;
		public int height;
		public string[] bgLayers;
		public Vec2Int[] groundPoints;
		public PlatformData[] platforms;
		public CameraBound cameraBounds;
		public DecorationConfig[] decorations;
	}

	[Serializable]
	public class Vec2Int
	{
		public int x, y;
	}

	[Serializable]
	public class PlatformData
	{
		public int x1, y1, x2, y2;
	}

	[Serializable]
	public class CameraBound
	{
		public int top, bottom, left, right;
	}

	[Serializable]
	public class DecorationConfig
	{
		public string image;
		public int x, y;
		public int width, height;
		public int order;
		public bool flipX, flipY;
		public string layer;
	}
}
