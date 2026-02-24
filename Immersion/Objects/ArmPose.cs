using Newtonsoft.Json;
using OWML.Common;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Immersion.Objects;

public class ArmPose
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

    private static Dictionary<string, ArmPose> s_armPoses;

    private static bool s_isDefaultArmDataLoaded;

    public static void LoadArmData(string jsonPath = "")
    {
        bool isDefaultArmPoses;
        if (jsonPath == "")
        {
            isDefaultArmPoses = true;
            jsonPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Data/viewmodel-arm-poses.json";
            ModMain.Log($"Loading default arm poses...", MessageType.Info);
        }
        else
        {
            // other mods can load custom arm poses for custom items or to replace arm poses for existing tools/items
            isDefaultArmPoses = false;
            ModMain.Log($"Loading arm poses from \"{jsonPath}\"...", MessageType.Info);
        }

        var newArmPoses = JsonConvert.DeserializeObject<Dictionary<string, ArmPose>>(File.ReadAllText(jsonPath));
        if (s_armPoses == null)
            s_armPoses = newArmPoses;
        else if (isDefaultArmPoses)
        {
            foreach (var pose in newArmPoses)
            {
                // only write new arm data if there is no arm data at this key
                if (!s_armPoses.ContainsKey(pose.Key))
                    s_armPoses.Add(pose.Key, pose.Value);
            }
        }
        else
        {
            foreach (var pose in newArmPoses)
                // overwrite arm data if this is a custom json
                if (s_armPoses.ContainsKey(pose.Key))
                    s_armPoses[pose.Key] = pose.Value;
                else
                    s_armPoses.Add(pose.Key, pose.Value);
        }

        if (isDefaultArmPoses)
            s_isDefaultArmDataLoaded = true;
        ModMain.Log($"Arm poses loaded successfully!", MessageType.Success);
    }

    public static bool ArmPoseExists(string armDataID)
    {
        if ((s_armPoses == null || !s_armPoses.ContainsKey(armDataID)) && !s_isDefaultArmDataLoaded)
            LoadArmData();

        return s_armPoses.ContainsKey(armDataID);
    }

    public static ArmPose GetArmPose(string armPoseID)
    {
        if (ArmPoseExists(armPoseID))
            return s_armPoses[armPoseID];

        ModMain.Log($"No ArmData found for {armPoseID}", MessageType.Error);
        return null;
    }

    public static string GetArmPoseID(OWItem item)
    {
        switch (item.GetItemType())
        {
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

            default:
                return item.GetDisplayName() switch
                {
                    // base game
                    "<color=orange>Ash Twin</color> Projection Stone" => "SharedStone",
                    "<color=orange>Ember Twin</color> Projection Stone" => "SharedStone",
                    "<color=orange>Timber Hearth</color> Projection Stone" => "SharedStone",
                    "<color=orange>Brittle Hollow</color> Projection Stone" => "SharedStone",
                    "<color=orange>Giant's Deep</color> Projection Stone" => "SharedStone",
                    "<color=orange>Launch Module</color> Projection Stone" => "SharedStone",
                    "<color=orange>Control Module</color> Projection Stone" => "SharedStone",
                    "<color=orange>Probe Tracking Module</color> Projection Stone" => "SharedStone",
                    "<color=orange>Scroll</color> Projection Stone" => "Scroll",

                    // TSTA
                    "Mineral" => "TSTA_Mineral",
                    "Sizzling Sands Seal" => "TSTA_Seal",
                    "Ringed Giant Seal" => "TSTA_Seal",
                    "Velvex Vortex Seal" => "TSTA_Seal",
                    "Burning Bombardier Seal" => "TSTA_Seal",
                    "Distant Enigma Seal" => "TSTA_Seal",
                    "Skull" => "TSTA_Skull",

                    // no arm pose ID
                    _ => null
                };
        }
    }
}