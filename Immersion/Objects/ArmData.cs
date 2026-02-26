using Newtonsoft.Json;
using OWML.Common;
using OWML.Utils;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Immersion.Objects;

public class ArmData
{
    [JsonProperty("arm_local_position")]
    public readonly Vector3 armLocalPosition;

    [JsonProperty("arm_local_euler_angles")]
    public readonly Vector3 armLocalEulerAngles;

    [JsonProperty("arm_scale")]
    public readonly float armScale;

    [JsonProperty("arm_shader")]
    public readonly string armShader;

    [JsonProperty("bones_local_euler_angles")]
    public readonly Dictionary<string, Vector3> bonesLocalEulerAngles;

    private static Dictionary<string, ArmData> s_armData;

    private static bool s_isDefaultArmDataLoaded;

    public static void LoadArmData(string jsonPath = "")
    {
        bool isLoadingDefaultArmData = string.IsNullOrEmpty(jsonPath);
        if (isLoadingDefaultArmData)
        {
            jsonPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Data/viewmodel-arm-data.json";
            ModMain.Log($"Loading default Arm Data...", MessageType.Info);
        }
        else
            // other mods can load custom arm data for custom items or to replace arm data for existing tools/items
            ModMain.Log($"Loading arm data from \"{jsonPath}\"...", MessageType.Info);

        var newArmData = JsonConvert.DeserializeObject<Dictionary<string, ArmData>>(File.ReadAllText(jsonPath));
        if (s_armData == null)
            s_armData = newArmData;
        else if (isLoadingDefaultArmData)
        {
            foreach (var data in newArmData)
            {
                // only write new arm data if there is no arm data at this key
                if (!s_armData.ContainsKey(data.Key))
                    s_armData.Add(data.Key, data.Value);
            }
        }
        else
        {
            foreach (var data in newArmData)
                // overwrite arm data if this is a custom json
                if (s_armData.ContainsKey(data.Key))
                    s_armData[data.Key] = data.Value;
                else
                    s_armData.Add(data.Key, data.Value);
        }

        if (isLoadingDefaultArmData)
            s_isDefaultArmDataLoaded = true;
        ModMain.Log($"Arm Data loaded successfully!", MessageType.Success);
    }

    public static bool Exists(string armDataID)
    {
        if (string.IsNullOrEmpty(armDataID)) return false;

        if ((s_armData == null || !s_armData.ContainsKey(armDataID)) && !s_isDefaultArmDataLoaded)
            LoadArmData();

        return s_armData.ContainsKey(armDataID);
    }

    public static ArmData Find(string armDataID)
    {
        if (Exists(armDataID))
            return s_armData[armDataID];

        ModMain.Log($"No Arm Data found for {armDataID}", MessageType.Error);
        return null;
    }

    public static string TryGetArmDataID(OWItem item)
    {
        var itemTypeName = item.GetItemType().GetName();
        return itemTypeName switch
        {
            // vanilla items
            "SharedStone" or "SlideReel" or "Lantern" or "VisionTorch" => itemTypeName,

            // vanilla items with variants
            "Scroll" => item.name switch
            {
                "Prefab_NOM_Scroll_egg" => "Scroll_Egg",
                "Prefab_NOM_Scroll_Jeff" => "Scroll_Jeff",
                _ => "Scroll"
            },
            "ConversationStone" => (item as NomaiConversationStone).GetWord() switch
            {
                NomaiWord.Identify or NomaiWord.Explain => "ConversationStone_Big",
                _ => "ConversationStone"
            },
            "WarpCore" => (item as WarpCoreItem).GetWarpCoreType() switch
            {
                WarpCoreType.Vessel or WarpCoreType.VesselBroken => "WarpCore",
                _ => "WarpCore_Simple"
            },
            "DreamLantern" => (item as DreamLanternItem).GetLanternType() switch
            {
                DreamLanternType.Nonfunctioning => "DreamLantern_Nonfunctioning",
                DreamLanternType.Malfunctioning => "DreamLantern_Malfunctioning",
                _ => "DreamLantern"
            },

            // TSTA items
            "CloakMineral" or "StrangerSeal" or "GhostbirdSkull" => $"TSTA_{itemTypeName}",

            // nothing found
            _ => null,
        };
    }
}