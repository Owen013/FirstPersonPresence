using Newtonsoft.Json;
using OWML.Common;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Immersion.Objects;

public class ArmData
{
    public Vector3 arm_offset_pos;

    public Vector3 arm_offset_rot;

    public float arm_scale;

    public string arm_shader;

    public Dictionary<string, Vector3> bone_eulers;

    private static Dictionary<string, ArmData> s_armData;

    public static void LoadArmData(string jsonPath = "")
    {
        bool isDefaultArmData;
        if (jsonPath == "")
        {
            isDefaultArmData = true;
            jsonPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "viewmodel-arm-data.json");
            ModMain.Log($"Loading default ArmData...", MessageType.Info);
        }
        else
        {
            // other mods can load custom arm data for custom items or to replace ArmData for existing tools/items
            isDefaultArmData = false;
            ModMain.Log($"Loading ArmData from \"{jsonPath}\"...", MessageType.Info);
        }

        var newArmData = JsonConvert.DeserializeObject<Dictionary<string, ArmData>>(File.ReadAllText(jsonPath));
        if (s_armData == null)
            s_armData = newArmData;
        else if (isDefaultArmData)
        {
            foreach (var data in newArmData)
            {
                // only write new arm data if there is no arm data at this key
                if (s_armData[data.Key] == null)
                    s_armData[data.Key] = data.Value;
            }
        }
        else
        {
            foreach (var data in newArmData)
                // overwrite arm data if this is a custom json
                s_armData[data.Key] = data.Value;
        }

        ModMain.Log($"ArmData loaded successfully!", MessageType.Success);
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
                ModMain.Log($"No ArmData found for \"{itemName}\"", MessageType.Error);
                return null;
            }
        }

        return s_armData[itemName];
    }
}