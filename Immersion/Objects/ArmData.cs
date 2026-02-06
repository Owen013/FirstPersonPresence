using Newtonsoft.Json.Linq;
using OWML.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Immersion.Objects;

public class ArmData
{
    public Vector3[] bonePositions;

    public Vector3[] boneEulers;

    public float scale;

    public string shaderName;

    private static Dictionary<string, ArmData> s_armData;

    public static void LoadArmData(string jsonPath = "")
    {
        if (s_armData == null)
            s_armData = [];

        bool isDefaultArmData;
        if (jsonPath == "")
        {
            isDefaultArmData = true;
            jsonPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "viewmodel-arm-data.json");
            ModMain.Instance.ModHelper.Console.WriteLine($"Loading default ArmData...", MessageType.Info);
        }
        else
        {
            // other mods can load custom arm data for custom items or to replace ArmData for existing tools/items
            isDefaultArmData = false;
            ModMain.Instance.ModHelper.Console.WriteLine($"Loading ArmData from \"{jsonPath}\"...", MessageType.Info);
        }

        // create ArmData from JSON
        JObject jsonData = JObject.Parse(File.ReadAllText(jsonPath));
        foreach (var (itemName, toolToken) in jsonData)
        {
            if (toolToken is not JObject toolObject) continue;

            var armData = new ArmData();

            if (toolObject["bone_positions"] is JArray posArray)
                armData.bonePositions = posArray.Select(vec => new Vector3((float)vec[0], (float)vec[1], (float)vec[2])).ToArray();

            if (toolObject["bone_eulers"] is JArray eulerArray)
                armData.boneEulers = eulerArray.Select(vec => new Vector3((float)vec[0], (float)vec[1], (float)vec[2])).ToArray();

            if (toolObject["arm_scale"] != null)
                armData.scale = (float)toolObject["arm_scale"];

            if (toolObject["arm_shader"] != null)
                armData.shaderName = (string)toolObject["arm_shader"];

            // default ArmData is only saved if there isn't already an ArmData at that key. custom ArmData replaces already existing data
            if (!s_armData.ContainsKey(itemName) || !isDefaultArmData)
                s_armData[itemName] = armData;
        }

        ModMain.Instance.ModHelper.Console.WriteLine($"ArmData loaded successfully!", MessageType.Success);
    }

    public static ArmData GetArmData(string itemName)
    {
        // if no ArmData has been loaded, or if no ArmData has been loaded for that specific item, load arm data just to make sure
        if (s_armData == null || !s_armData.ContainsKey(itemName))
        {
            LoadArmData();

            // if ArmData still isn't loaded then it either doesn't exist or is part of a custom JSON that hasn't been loaded
            if (!s_armData.ContainsKey(itemName))
            {
                LoadArmData();
                ModMain.Instance.ModHelper.Console.WriteLine($"No ArmData found for \"{itemName}\"", MessageType.Error);
                return null;
            }
        }

        return s_armData[itemName];
    }
}