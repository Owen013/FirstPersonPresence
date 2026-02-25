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
    [JsonProperty("arm_offset_pos")]
    public readonly Vector3 armOffsetPos;

    [JsonProperty("arm_offset_rot")]
    public readonly Vector3 armOffsetRot;

    [JsonProperty("arm_scale")]
    public readonly float armScale;

    [JsonProperty("arm_shader")]
    public readonly string armShader;

    [JsonProperty("bone_eulers")]
    public readonly Dictionary<string, Vector3> boneEulers;

    private static Dictionary<string, ArmPose> s_armPoses;

    private static bool s_isDefaultArmPosesLoaded;

    public static void LoadArmPoses(string jsonPath = "")
    {
        bool isDefaultArmPoses;
        if (jsonPath == "")
        {
            isDefaultArmPoses = true;
            jsonPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Data/viewmodel-arm-poses.json";
            ModMain.Log($"Loading default Arm Poses...", MessageType.Info);
        }
        else
        {
            // other mods can load custom arm data for custom items or to replace arm poses for existing tools/items
            isDefaultArmPoses = false;
            ModMain.Log($"Loading Arm Poses from \"{jsonPath}\"...", MessageType.Info);
        }

        var newArmPoses = JsonConvert.DeserializeObject<Dictionary<string, ArmPose>>(File.ReadAllText(jsonPath));
        if (s_armPoses == null)
            s_armPoses = newArmPoses;
        else if (isDefaultArmPoses)
        {
            foreach (var data in newArmPoses)
            {
                // only write new arm data if there is no arm data at this key
                if (!s_armPoses.ContainsKey(data.Key))
                    s_armPoses.Add(data.Key, data.Value);
            }
        }
        else
        {
            foreach (var data in newArmPoses)
                // overwrite arm data if this is a custom json
                if (s_armPoses.ContainsKey(data.Key))
                    s_armPoses[data.Key] = data.Value;
                else
                    s_armPoses.Add(data.Key, data.Value);
        }

        if (isDefaultArmPoses)
            s_isDefaultArmPosesLoaded = true;
        ModMain.Log($"Arm Poses loaded successfully!", MessageType.Success);
    }

    public static bool ArmPoseExists(string armPoseID)
    {
        if (string.IsNullOrEmpty(armPoseID)) return false;

        if ((s_armPoses == null || !s_armPoses.ContainsKey(armPoseID)) && !s_isDefaultArmPosesLoaded)
            LoadArmPoses();

        return s_armPoses.ContainsKey(armPoseID);
    }

    public static ArmPose GetArmPose(string armPoseID)
    {
        if (ArmPoseExists(armPoseID))
            return s_armPoses[armPoseID];

        ModMain.Log($"No Arm Pose found for {armPoseID}", MessageType.Error);
        return null;
    }

    public static string TryGetArmPoseID(OWItem item)
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