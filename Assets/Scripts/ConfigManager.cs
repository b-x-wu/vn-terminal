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
    public static Dictionary<RuntimePlatform, string> RUNTIME_PLATFORM_TO_DEFAULT_SHELL_FILE_PATH = new Dictionary<RuntimePlatform, string>(){
        { RuntimePlatform.WindowsEditor, Environment.GetEnvironmentVariable("COMSPEC") ?? @"C:\Windows\System32\cmd.exe" },
        { RuntimePlatform.WindowsPlayer, Environment.GetEnvironmentVariable("COMSPEC") ?? @"C:\Windows\System32\cmd.exe" },
        { RuntimePlatform.LinuxPlayer, Environment.GetEnvironmentVariable("SHELL") ?? "/bin/bash" },
        { RuntimePlatform.OSXPlayer, Environment.GetEnvironmentVariable("SHELL") ?? "/bin/zsh" },
    };
    public static Dictionary<RuntimePlatform, string> RUNTIME_PLATFORM_TO_DEFAULT_WORKING_DIRECTORY = new Dictionary<RuntimePlatform, string>(){
        { RuntimePlatform.WindowsEditor, Environment.GetEnvironmentVariable("USERPROFILE") ?? "C:\\" },
        { RuntimePlatform.WindowsPlayer, Environment.GetEnvironmentVariable("USERPROFILE") ?? "C:\\" },
        { RuntimePlatform.LinuxPlayer, Environment.GetEnvironmentVariable("HOME") ?? "/" },
        { RuntimePlatform.OSXPlayer, Environment.GetEnvironmentVariable("HOME") ?? "/" },
    };

    [Serializable]
    public class SerializableConfigData
    {
        public float repeatRate;
        public string standardOutputColor;
        public string standardErrorColor;
        public List<string> standardOutputCharacterSprites;
        public List<string> standardErrorCharacterSprites;
        public string canvasBackgroundSprite;
        public string workingDirectory;
        public string shellFilePath;
        public string primaryColor;
        public string secondaryColor;
    }

    public class ConfigData
    {
        public float repeatRate;
        public Color standardOutputColor;
        public Color standardErrorColor;
        public List<Sprite> standardOutputCharacterSprites;
        public List<Sprite> standardErrorCharacterSprites;
        public Sprite canvasBackgroundSprite;
        public string workingDirectory;
        public string shellFilePath;
        public Color primaryColor;
        public Color secondaryColor;
    }

    public static ConfigData DEFAULT_CONFIG_DATA = new ConfigData()
    {
        repeatRate = 0.01F,
        standardOutputColor = Color.white,
        standardErrorColor = new Color(0.8392157F, 0.145098F, 0.1137255F, 1F),
        workingDirectory = RUNTIME_PLATFORM_TO_DEFAULT_WORKING_DIRECTORY.ContainsKey(Application.platform) ? RUNTIME_PLATFORM_TO_DEFAULT_WORKING_DIRECTORY[Application.platform] : null,
        shellFilePath = RUNTIME_PLATFORM_TO_DEFAULT_SHELL_FILE_PATH.ContainsKey(Application.platform) ? RUNTIME_PLATFORM_TO_DEFAULT_SHELL_FILE_PATH[Application.platform] : null,
        primaryColor = new Color(0.9607843F, 0.6039216F, 0.7215686F, 0.8078431F),
        secondaryColor = Color.white,
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
        try
        {
            if (Directory.Exists(CHARACTER_SPRITE_DIRECTORY))
            {
                Directory.Delete(CHARACTER_SPRITE_DIRECTORY, true);
            }
            if (Directory.Exists(CANVAS_BACKGROUND_SPRITE_DIRECTORY))
            {
                Directory.Delete(CANVAS_BACKGROUND_SPRITE_DIRECTORY, true);
            }
        }
        catch (IOException e)
        {
            Debug.LogError("Error clearing sprite directories.");
            Debug.LogError(e.Message);
        }
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
            }),
            workingDirectory = configData.workingDirectory,
            shellFilePath = configData.shellFilePath,
            primaryColor = "#" + ColorUtility.ToHtmlStringRGBA(configData.primaryColor),
            secondaryColor = "#" + ColorUtility.ToHtmlStringRGBA(configData.secondaryColor),
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
                repeatRate = Math.Abs(serializableConfigData.repeatRate),
                standardOutputCharacterSprites = serializableConfigData.standardOutputCharacterSprites.ConvertAll<Sprite>((string path) => {
                    return PathToSprite(path);
                }),
                standardErrorCharacterSprites = serializableConfigData.standardErrorCharacterSprites.ConvertAll<Sprite>((string path) => {
                    return PathToSprite(path);
                }),
                canvasBackgroundSprite = PathToSprite(serializableConfigData.canvasBackgroundSprite),
                workingDirectory = serializableConfigData.workingDirectory == null || serializableConfigData.workingDirectory == "" 
                    ? ConfigManager.DEFAULT_CONFIG_DATA.workingDirectory 
                    : serializableConfigData.workingDirectory,
                shellFilePath = serializableConfigData.shellFilePath == null || serializableConfigData.shellFilePath == ""
                    ? ConfigManager.DEFAULT_CONFIG_DATA.shellFilePath
                    : serializableConfigData.shellFilePath
            };
            bool standardOutputColorParsed = ColorUtility.TryParseHtmlString(serializableConfigData.standardOutputColor, out configData.standardOutputColor);
            configData.standardOutputColor = standardOutputColorParsed ? configData.standardOutputColor : ConfigManager.DEFAULT_CONFIG_DATA.standardOutputColor;
            bool standardErrorColorParsed = ColorUtility.TryParseHtmlString(serializableConfigData.standardErrorColor, out configData.standardErrorColor);
            configData.standardErrorColor = standardErrorColorParsed ? configData.standardErrorColor : ConfigManager.DEFAULT_CONFIG_DATA.standardErrorColor;
            bool primaryColorParsed = ColorUtility.TryParseHtmlString(serializableConfigData.primaryColor, out configData.primaryColor);
            configData.primaryColor = primaryColorParsed ? configData.primaryColor : ConfigManager.DEFAULT_CONFIG_DATA.primaryColor;
            bool secondaryColorParsed = ColorUtility.TryParseHtmlString(serializableConfigData.secondaryColor, out configData.secondaryColor);
            configData.secondaryColor = secondaryColorParsed ? configData.secondaryColor : ConfigManager.DEFAULT_CONFIG_DATA.secondaryColor;

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
