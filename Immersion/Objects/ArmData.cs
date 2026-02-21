using Immersion.Utils;
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
    [JsonProperty("arm_offset_pos")]
    public Vector3 armOffsetPos;

    [JsonProperty("arm_offset_rot")]
    public Vector3 armOffsetRot;

    [JsonProperty("arm_scale")]
    public float armScale;

    [JsonProperty("arm_shader")]
    public string armShader;

    [JsonProperty("bone_eulers")]
    public Dictionary<string, Vector3> boneEulers;

    private static Dictionary<string, ArmData> s_armData;

    private static bool s_isDefaultArmDataLoaded;

    public static void LoadArmData(string jsonPath = "")
    {
        bool isDefaultArmData;
        if (jsonPath == "")
        {
            isDefaultArmData = true;
            jsonPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Data/viewmodel-arm-data.json";
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

        if (isDefaultArmData)
            s_isDefaultArmDataLoaded = true;
        ModMain.Log($"ArmData loaded successfully!", MessageType.Success);
    }

    public static bool ArmDataExists(string armDataID)
    {
        if ((s_armData == null || !s_armData.ContainsKey(armDataID)) && !s_isDefaultArmDataLoaded)
            LoadArmData();

        return s_armData.ContainsKey(armDataID);
    }

    public static ArmData GetArmData(string armDataID)
    {
        if (ArmDataExists(armDataID))
            return s_armData[armDataID];

        ModMain.Log($"No ArmData found for {armDataID}", MessageType.Error);
        return null;
    }

    public static string TryGetArmDataID(OWItem item)
    {
        if (ItemUtils.IsBaseGameItem(item))
        {
            var itemType = item.GetItemType();
            switch (itemType)
            {
                // some items have variants
                case ItemType.Scroll:
                    return item.name switch
                    {
                        "Prefab_NOM_Scroll_egg" => "Scroll_Egg",
                        "Prefab_NOM_Scroll_Jeff" => "Scroll_Jeff",
                        _ => "Scroll"
                    };
                case ItemType.ConversationStone:
                    var word = (item as NomaiConversationStone).GetWord();
                    if (word == NomaiWord.Identify || word == NomaiWord.Explain)
                        return "ConversationStone_Big";
                    else
                        return "ConversationStone";

                case ItemType.WarpCore:
                    var warpCoreType = (item as WarpCoreItem).GetWarpCoreType();
                    if (warpCoreType == WarpCoreType.Vessel || warpCoreType == WarpCoreType.VesselBroken)
                        return "WarpCore";
                    else
                        return "WarpCore_Simple";

                case ItemType.DreamLantern:
                    return (item as DreamLanternItem).GetLanternType() switch
                    {
                        DreamLanternType.Nonfunctioning => "DreamLantern_Nonfunctioning",
                        DreamLanternType.Malfunctioning => "DreamLantern_Malfunctioning",
                        _ => "DreamLantern"
                    };

                // for the rest, their arm data identifier is simply their item type
                default:
                    return itemType.GetName();
            }
        }
        else if (ItemUtils.IsTSTAItem(item))
            return $"TSTA_{item.GetDisplayName().Trim()}";

        return null;
    }
}