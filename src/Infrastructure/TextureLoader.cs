using UnityEngine;

namespace Cms21ImmersionPlus
{
    /// <summary>Loads PNG/JPG data into Unity sprites.</summary>
    public static class TextureLoader
    {
        public static Sprite LoadSpriteFromFile(string filePath,
            bool highQualityCompression = true, bool trimTransparentBorders = false)
        {
            if (!System.IO.File.Exists(filePath)) {
                ModLogger.Log("File not found " + filePath,
                    Types.LoggingLevels.Warning);
                return null;
            }

            return LoadSpriteFromBytes(System.IO.File.ReadAllBytes(filePath),
                System.IO.Path.GetFileNameWithoutExtension(filePath),
                highQualityCompression, trimTransparentBorders);
        }

        public static Sprite LoadSpriteFromBytes(byte[] imageData)
        {
            return LoadSpriteFromBytes(imageData, null, true, false);
        }

        private static Sprite LoadSpriteFromBytes(byte[] imageData,
            string textureName, bool highQualityCompression,
            bool trimTransparentBorders)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            Texture2D texture = new Texture2D(2, 2);
            if (!string.IsNullOrEmpty(textureName))
                texture.name = textureName;
            if (!ImageConversion.LoadImage(texture, imageData)) {
                ModLogger.Log("LoadImage error", Types.LoggingLevels.Warning);
                UnityEngine.Object.Destroy(texture);
                return null;
            }

            Rect spriteRect = trimTransparentBorders
                ? GetVisibleRect(texture)
                : new Rect(0f, 0f, texture.width, texture.height);
            texture.Compress(highQualityCompression);
            return Sprite.Create(texture, spriteRect, Vector2.zero, 100f, 0U,
                SpriteMeshType.Tight);
        }

        public static Sprite CreateTrimmedSprite(Sprite source)
        {
            if (source == null || source.texture == null)
                return null;

            return Sprite.Create(source.texture, GetVisibleRect(source.texture),
                Vector2.zero, source.pixelsPerUnit, 0U, SpriteMeshType.Tight);
        }

        public static Sprite CreateScaledTrimmedSprite(Sprite source,
            float visibleScale)
        {
            if (source == null || source.texture == null)
                return null;

            visibleScale = Mathf.Clamp(visibleScale, 0.05f, 1f);
            Texture2D sourceTexture = source.texture;
            Rect visibleRect = GetVisibleRect(sourceTexture);
            int sourceX = Mathf.RoundToInt(visibleRect.x);
            int sourceY = Mathf.RoundToInt(visibleRect.y);
            int visibleWidth = Mathf.RoundToInt(visibleRect.width);
            int visibleHeight = Mathf.RoundToInt(visibleRect.height);
            int canvasWidth = Mathf.Max(visibleWidth,
                Mathf.CeilToInt(visibleWidth / visibleScale));
            int canvasHeight = Mathf.Max(visibleHeight,
                Mathf.CeilToInt(visibleHeight / visibleScale));

            Texture2D canvas = new Texture2D(canvasWidth, canvasHeight,
                TextureFormat.RGBA32, false);
            canvas.name = sourceTexture.name;
            canvas.wrapMode = TextureWrapMode.Clamp;
            canvas.filterMode = FilterMode.Bilinear;

            Color32[] sourcePixels = sourceTexture.GetPixels32();
            Color32[] canvasPixels = new Color32[canvasWidth * canvasHeight];
            int targetX = (canvasWidth - visibleWidth) / 2;
            int targetY = (canvasHeight - visibleHeight) / 2;
            for (int y = 0; y < visibleHeight; y++) {
                int sourceRow = (sourceY + y) * sourceTexture.width + sourceX;
                int targetRow = (targetY + y) * canvasWidth + targetX;
                for (int x = 0; x < visibleWidth; x++)
                    canvasPixels[targetRow + x] = sourcePixels[sourceRow + x];
            }

            canvas.SetPixels32(canvasPixels);
            canvas.Apply(false, false);
            canvas.Compress(true);
            Sprite sprite = Sprite.Create(canvas,
                new Rect(0f, 0f, canvas.width, canvas.height), Vector2.zero,
                source.pixelsPerUnit, 0U, SpriteMeshType.Tight);
            sprite.name = source.name;
            return sprite;
        }

        private static Rect GetVisibleRect(Texture2D texture)
        {
            var pixels = texture.GetPixels32();
            int minX = texture.width;
            int minY = texture.height;
            int maxX = -1;
            int maxY = -1;

            for (int y = 0; y < texture.height; y++) {
                int row = y * texture.width;
                for (int x = 0; x < texture.width; x++) {
                    if (pixels[row + x].a == 0)
                        continue;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }

            if (maxX < minX || maxY < minY)
                return new Rect(0f, 0f, texture.width, texture.height);

            const int padding = 2;
            minX = Mathf.Max(0, minX - padding);
            minY = Mathf.Max(0, minY - padding);
            maxX = Mathf.Min(texture.width - 1, maxX + padding);
            maxY = Mathf.Min(texture.height - 1, maxY + padding);
            return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }
    }
}
