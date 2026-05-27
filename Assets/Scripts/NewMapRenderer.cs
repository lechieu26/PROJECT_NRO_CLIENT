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

	/// <summary>
	/// Parse map config JSON tu server va load assets.
	/// </summary>
	public void loadMap7VNR(string configJson)
	{
		isLoaded = false;
		groundPoints.Clear();
		platforms.Clear();
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

			loadBackgroundImages();
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

	public void reset()
	{
		isLoaded = false;
		groundPoints.Clear();
		platforms.Clear();
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

	[Serializable]
	public class MapConfig
	{
		public int width;
		public int height;
		public string[] bgLayers;
		public Vec2Int[] groundPoints;
		public PlatformData[] platforms;
		public CameraBound cameraBounds;
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
}
