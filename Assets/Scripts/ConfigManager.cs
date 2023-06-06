using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class ConfigManager
{
    public static string CONFIG_FILE_NAME = "config.json";
    public static string CHARACTER_SPRITE_DIRECTORY = Path.Join(Application.dataPath, "character_sprites");
    public static string CANVAS_BACKGROUND_SPRITE_DIRECTORY = Path.Join(Application.dataPath, "background_sprites");
    

    [Serializable]
    public class SerializableConfigData
    {
        public float repeatRate;
        public string standardOutputColor;
        public string standardErrorColor;
        public List<string> standardOutputCharacterSprites;
        public List<string> standardErrorCharacterSprites;
        public string canvasBackgroundSprite;
    }

    public class ConfigData
    {
        public float repeatRate;
        public Color standardOutputColor;
        public Color standardErrorColor;
        public List<Sprite> standardOutputCharacterSprites;
        public List<Sprite> standardErrorCharacterSprites;
        public Sprite canvasBackgroundSprite;
    }

    public static ConfigData DEFAULT_CONFIG_DATA = new ConfigData()
    {
        repeatRate = 0.01F,
        standardOutputColor = Color.white,
        standardErrorColor = Color.red,
    };

    private static string WriteSprite(Sprite sprite, string directory)
    {
        // writes the sprite as a png and returns the path to the png
        // returns null if unsucessful
        string spritePath = Path.Join(directory, sprite.GetInstanceID() + ".png");
        try
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllBytes(spritePath, sprite.texture.EncodeToPNG());
            return spritePath;
        }
        catch (IOException e)
        {
            Debug.LogError($"IO Error while writing out texture. path: [{spritePath}]");
            Debug.LogError(e.Message);
            return null;
        }
    }

    public static void WriteData(ConfigData configData)
    {
        SerializableConfigData serializableConfigData = new SerializableConfigData()
        {
            repeatRate = configData.repeatRate,
            standardOutputColor = "#" + ColorUtility.ToHtmlStringRGBA(configData.standardOutputColor),
            standardErrorColor = "#" + ColorUtility.ToHtmlStringRGBA(configData.standardErrorColor),
            standardOutputCharacterSprites = configData.standardOutputCharacterSprites.ConvertAll<string>((Sprite sprite) => {
                return WriteSprite(sprite, CHARACTER_SPRITE_DIRECTORY);
            }),
            standardErrorCharacterSprites = configData.standardErrorCharacterSprites.ConvertAll<string>((Sprite sprite) => {
                return WriteSprite(sprite, CHARACTER_SPRITE_DIRECTORY);
            })
        };

        serializableConfigData.standardOutputCharacterSprites.RemoveAll(path => path == null);
        serializableConfigData.standardErrorCharacterSprites.RemoveAll(path => path == null);

        string pathToCanvasBackgroundSprite = WriteSprite(configData.canvasBackgroundSprite, CANVAS_BACKGROUND_SPRITE_DIRECTORY);
        if (pathToCanvasBackgroundSprite != null)
        {
            serializableConfigData.canvasBackgroundSprite = pathToCanvasBackgroundSprite;
        }

        string json = JsonUtility.ToJson(serializableConfigData);
        string path = Path.Join(Application.dataPath, CONFIG_FILE_NAME);
        try
        {
            File.WriteAllText(path, json);
            Debug.Log($"Config written to {path}.");
        }
        catch (IOException e)
        {
            Debug.LogError("Unable to write config.");
            Debug.LogError(e);
        }
    }

    private static Sprite PathToSprite(string path)
    {
        try
        {
            Texture2D texture = new Texture2D(2, 2);
            if (!texture.LoadImage(File.ReadAllBytes(path)))
            {
                throw new ArgumentException();
            }
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5F, 0.0F));
        }
        catch (IOException e)
        {
            Debug.LogError($"IOException loading texture from path: [{path}]");
            Debug.LogError(e.Message);
            return null;
        }
        catch (ArgumentException)
        {
            Debug.LogError($"Error while loading texture");
            return null;
        }
    }

    public static ConfigData ReadData()
    {
        try
        {
            string path = Path.Join(Application.dataPath, CONFIG_FILE_NAME);
            string json = File.ReadAllText(path);
            SerializableConfigData serializableConfigData = JsonUtility.FromJson<SerializableConfigData>(json);
            ConfigData configData = new ConfigData()
            {
                repeatRate = serializableConfigData.repeatRate,
                standardOutputCharacterSprites = serializableConfigData.standardOutputCharacterSprites.ConvertAll<Sprite>((string path) => {
                    return PathToSprite(path);
                }),
                standardErrorCharacterSprites = serializableConfigData.standardErrorCharacterSprites.ConvertAll<Sprite>((string path) => {
                    return PathToSprite(path);
                }),
                canvasBackgroundSprite = PathToSprite(serializableConfigData.canvasBackgroundSprite)
            };
            bool standardOutputColorParsed = ColorUtility.TryParseHtmlString(serializableConfigData.standardOutputColor, out configData.standardOutputColor);
            bool standardErrorColorParsed = ColorUtility.TryParseHtmlString(serializableConfigData.standardErrorColor, out configData.standardErrorColor);

            if (!(standardOutputColorParsed && standardErrorColorParsed))
            {
                throw new ArgumentException(
                    $"Error parsing RGBA value.\nstandardOutputColor: \"{serializableConfigData.standardOutputColor}\", standardErrorColor: \"{serializableConfigData.standardErrorColor}\""
                );
            }

            configData.standardOutputCharacterSprites.RemoveAll(sprite => sprite == null);
            configData.standardErrorCharacterSprites.RemoveAll(sprite => sprite == null);

            if (configData.standardOutputCharacterSprites.Count == 0 || configData.standardErrorCharacterSprites.Count == 0)
            {
                throw new ArgumentException($"No character sprites loaded.");
            }

            Debug.Log($"Config read from {path}.");
            return configData;
        }
        catch (IOException e)
        {
            Debug.LogError("Unable to read config.");
            Debug.LogError(e.Message);
            return DEFAULT_CONFIG_DATA;
        }
        catch (ArgumentException e)
        {
            Debug.LogError(e.Message);
            return DEFAULT_CONFIG_DATA;
        }
    }
}
