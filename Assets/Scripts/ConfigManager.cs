using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class ConfigManager
{
    public static string CONFIG_FILE_NAME = "config.json";

    [Serializable]
    public class SerializableConfigData
    {
        public float repeatRate;
        public string standardOutputColor;
        public string standardErrorColor;
    }

    public class ConfigData
    {
        public float repeatRate;
        public Color standardOutputColor;
        public Color standardErrorColor;
    }

    public static ConfigData DEFAULT_CONFIG_DATA = new ConfigData()
    {
        repeatRate = 0.01F,
        standardOutputColor = Color.white,
        standardErrorColor = Color.red,
    };

    public static void WriteData(ConfigData configData)
    {
        SerializableConfigData serializableConfigData = new SerializableConfigData()
        {
            repeatRate = configData.repeatRate,
            standardOutputColor = "#" + ColorUtility.ToHtmlStringRGBA(configData.standardOutputColor),
            standardErrorColor = "#" + ColorUtility.ToHtmlStringRGBA(configData.standardErrorColor),
        };
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
            };
            bool standardOutputColorParsed = ColorUtility.TryParseHtmlString(serializableConfigData.standardOutputColor, out configData.standardOutputColor);
            bool standardErrorColorParsed = ColorUtility.TryParseHtmlString(serializableConfigData.standardErrorColor, out configData.standardErrorColor);

            if (!(standardOutputColorParsed && standardErrorColorParsed))
            {
                throw new ArgumentException($"standardOutputColor: \"{serializableConfigData.standardOutputColor}\", standardErrorColor: \"{serializableConfigData.standardErrorColor}\"");
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
            Debug.LogError("Error parsing RGBA value.");
            Debug.LogError(e.Message);
            return DEFAULT_CONFIG_DATA;
        }
    }
}
