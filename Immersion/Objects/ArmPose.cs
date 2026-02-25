using Newtonsoft.Json;
using OWML.Common;
using OWML.Utils;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Immersion.Objects;

public class ArmPose
{
    [JsonProperty("arm_local_position")]
    public Vector3 armLocalPosition;

    [JsonProperty("arm_local_euler_angles")]
    public Vector3 armLocalEulerAngles;

    [JsonProperty("arm_scale")]
    public float armScale;

    [JsonProperty("arm_shader")]
    public string armShader;

    [JsonProperty("bones_local_euler_angles")]
    public Dictionary<string, Vector3> bonesLocalEulerAngles;

    private static Dictionary<string, ArmPose> s_armPoses;

    private static bool s_areDefaultArmPosesLoaded;

    public static void LoadArmPoses(string jsonPath = "")
    {
        bool isLoadingDefaultArmPoses = string.IsNullOrEmpty(jsonPath);
        if (isLoadingDefaultArmPoses)
        {
            jsonPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Data/viewmodel-arm-poses.json";
            ModMain.Log($"Loading default arm poses...", MessageType.Info);
        }
        else
            // other mods can load custom arm data for custom items or to replace ArmData for existing tools/items
            ModMain.Log($"Loading arm poses from \"{jsonPath}\"...", MessageType.Info);

        var newArmData = JsonConvert.DeserializeObject<Dictionary<string, ArmPose>>(File.ReadAllText(jsonPath));
        if (s_armPoses == null)
            s_armPoses = newArmData;
        else if (isLoadingDefaultArmPoses)
        {
            foreach (var data in newArmData)
            {
                // only write new arm data if there is no arm data at this key
                if (!s_armPoses.ContainsKey(data.Key))
                    s_armPoses.Add(data.Key, data.Value);
            }
        }
        else
        {
            foreach (var data in newArmData)
                // overwrite arm data if this is a custom json
                if (s_armPoses.ContainsKey(data.Key))
                    s_armPoses[data.Key] = data.Value;
                else
                    s_armPoses.Add(data.Key, data.Value);
        }

        if (isLoadingDefaultArmPoses)
            s_areDefaultArmPosesLoaded = true;
        ModMain.Log($"Arm poses loaded successfully!", MessageType.Success);
    }

    public static bool ArmPoseExists(string armDataID)
    {
        if (string.IsNullOrEmpty(armDataID)) return false;

        if ((s_armPoses == null || !s_armPoses.ContainsKey(armDataID)) && !s_areDefaultArmPosesLoaded)
            LoadArmPoses();

        return s_armPoses.ContainsKey(armDataID);
    }

    public static ArmPose GetArmPose(string armDataID)
    {
        if (ArmPoseExists(armDataID))
            return s_armPoses[armDataID];

        ModMain.Log($"No ArmData found for {armDataID}", MessageType.Error);
        return null;
    }

    internal static string TryGetArmPoseID(OWItem item)
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