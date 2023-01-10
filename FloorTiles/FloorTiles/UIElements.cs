using System.IO;
using UnityEngine;
using UnityEngine.UI;

public static class UIElements
{
    public static Texture2D LoadImage(string filename)
    {
        try
        {
            byte[] bytes = File.ReadAllBytes(filename);

            Texture2D texture = new Texture2D(4, 4);
            ImageConversion.LoadImage(texture, bytes);

            return texture;
        } catch
        {
            Texture2D texture = new Texture2D(4, 4);
            Color pink = new Color(1, 0.5f, 0.5f);
            Color black = Color.black;
            Color[] colors = new Color[16] { pink, black, pink, black, pink, black, pink, black, pink, black, pink, black, pink, black, pink, black };
            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }
    }

    public static Sprite ToSprite(Texture2D texture, int pixelsPerUnit = 100)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    public static void SliceTextureIntoObject(ref Image image, Texture2D texture, Vector4 border, Color color, int pixelsPerUnit = 100)
    {
        image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.Tight, border);
        image.type = Image.Type.Sliced;
        image.color = color;
    }

    public static Image SliceTextureIntoObject(Texture2D texture, Vector4 border, Color color, int pixelsPerUnit = 100)
    {
        Image image = new GameObject().AddComponent<Image>();
        image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.Tight, border);
        image.type = Image.Type.Sliced;
        image.color = color;
        return image;
    }
}