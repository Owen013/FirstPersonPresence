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
    public readonly Vector3 armLocalPosition;

    [JsonProperty("arm_local_euler_angles")]
    public readonly Vector3 armLocalEulerAngles;

    [JsonProperty("arm_scale")]
    public readonly float armScale;

    [JsonProperty("arm_shader")]
    public readonly string armShader;

    [JsonProperty("bones_local_euler_angles")]
    public readonly Dictionary<string, Vector3> bonesLocalEulerAngles;

    private static Dictionary<string, ArmPose> s_armPoses;

    private static bool s_areDefaultArmPosesLoaded;

    public static void LoadArmPoses(string jsonPath = "")
    {
        bool isLoadingDefaultArmPoses = string.IsNullOrEmpty(jsonPath);
        if (isLoadingDefaultArmPoses)
        {
            jsonPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Data/viewmodel-arm-poses.json";
            ModMain.Log($"Loading default Arm Poses...", MessageType.Info);
        }
        else
            // other mods can load custom arm poses for custom items or to replace arm poses for existing tools/items
            ModMain.Log($"Loading arm poses from \"{jsonPath}\"...", MessageType.Info);

        var newArmPoses = JsonConvert.DeserializeObject<Dictionary<string, ArmPose>>(File.ReadAllText(jsonPath));
        if (s_armPoses == null)
            s_armPoses = newArmPoses;
        else if (isLoadingDefaultArmPoses)
        {
            foreach (var pose in newArmPoses)
            {
                // only write new arm pose if there is no arm pose at this key
                if (!s_armPoses.ContainsKey(pose.Key))
                    s_armPoses.Add(pose.Key, pose.Value);
            }
        }
        else
        {
            foreach (var pose in newArmPoses)
                // overwrite arm poses if this is a custom json
                if (s_armPoses.ContainsKey(pose.Key))
                    s_armPoses[pose.Key] = pose.Value;
                else
                    s_armPoses.Add(pose.Key, pose.Value);
        }

        if (isLoadingDefaultArmPoses)
            s_areDefaultArmPosesLoaded = true;
        ModMain.Log($"Arm poses loaded successfully!", MessageType.Success);
    }

    public static bool Exists(string armPoseID)
    {
        if (string.IsNullOrEmpty(armPoseID)) return false;

        if ((s_armPoses == null || !s_armPoses.ContainsKey(armPoseID)) && !s_areDefaultArmPosesLoaded)
            LoadArmPoses();

        return s_armPoses.ContainsKey(armPoseID);
    }

    public static ArmPose Find(string armPoseID)
    {
        if (Exists(armPoseID))
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