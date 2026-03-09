using Newtonsoft.Json;
using OWML.Common;
using OWML.Utils;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Immersion.Scripts
{
    public class ArmData
    {
        [JsonProperty("arm_position")]
        public readonly Vector3 armPosition;

        [JsonProperty("arm_rotation")]
        public readonly Vector3 armRotation;

        [JsonProperty("arm_scale")]
        public readonly float armScale;

        [JsonProperty("arm_shader")]
        public readonly string armShader;

        [JsonProperty("bone_eulers")]
        public readonly Dictionary<string, Vector3> boneEulers;

        private static Dictionary<string, ArmData> s_armData;

        private static bool s_isDefaultArmDataLoaded;

        private static List<string> s_jsonsPathsToLoad;

        /// <summary>
        /// Returns whether or not there is a corresponding ArmData for a given ID
        /// </summary>
        /// <param name="armDataId">The ID of the desired ArmData</param>
        /// <returns>True if there is an ArmData for the given ID, false if there isn't one</returns>
        public static bool Exists(string armDataId)
        {
            return !string.IsNullOrEmpty(armDataId) && s_armData != null && s_armData.ContainsKey(armDataId);
        }

        /// <summary>
        /// Returns the ArmData linked to the given ID, if it exists
        /// </summary>
        /// <param name="armDataId">The ID of the desired ArmData</param>
        /// <returns>The corresponding ArmData, if it exists</returns>
        public static ArmData Find(string armDataId)
        {
            if (Exists(armDataId))
            {
                return s_armData[armDataId];
            }

            ModMain.Console.WriteLine($"No Arm Data found for {armDataId}", MessageType.Error);
            return null;
        }

        /// <summary>
        /// Returns the ID of the ArmData that cooresponds to the given item, if it exists
        /// </summary>
        /// <param name="item">The item to try to get the ArmData ID for</param>
        /// <returns>The ID of the corresponding ArmData if it exists, null if it doesn't</returns>
        public static string FindArmDataIdOfItem(OWItem item)
        {
            var itemTypeName = item.GetItemType().GetName();
            switch (itemTypeName)
            {
                // vanilla items
                case "SharedStone":
                case "SlideReel":
                case "Lantern":
                case "VisionTorch":
                    return itemTypeName;

                // vanilla items with variants
                case "Scroll":
                    return item.name switch
                    {
                        "Prefab_NOM_Scroll_egg" => "Scroll_Egg",
                        "Prefab_NOM_Scroll_Jeff" => "Scroll_Jeff",
                        _ => "Scroll"
                    };
                case "ConversationStone":
                    if (item is NomaiConversationStone conversationStone)
                    {
                        return conversationStone.GetWord() switch
                        {
                            NomaiWord.Identify or NomaiWord.Explain => "ConversationStone_Big",
                            _ => "ConversationStone"
                        };
                    }
                    return null;
                case "WarpCore":
                    if (item is WarpCoreItem warpCore)
                    {
                        return warpCore.GetWarpCoreType() switch
                        {
                            WarpCoreType.Vessel or WarpCoreType.VesselBroken => "WarpCore",
                            _ => "WarpCore_Simple"
                        };
                    }
                    return null;
                case "DreamLantern":
                    if (item is DreamLanternItem dreamLantern)
                    {
                        return dreamLantern.GetLanternType() switch
                        {
                            DreamLanternType.Nonfunctioning => "DreamLantern_Nonfunctioning",
                            DreamLanternType.Malfunctioning => "DreamLantern_Malfunctioning",
                            _ => "DreamLantern"
                        };
                    }
                    return null;

                // TSTA items
                case "CloakMineral":
                case "StrangerSeal":
                case "GhostbirdSkull":
                    return $"TSTA_{itemTypeName}";

                // Dreambound compass
                case "Compass":
                    return "Dreambound_Compass";
            }
            ;

            return null;
        }

        /// <summary>
        /// Loads ArmData from a custom JSON
        /// </summary>
        /// <param name="jsonPath">The path to the JSON with the custom ArmData info</param>
        public static void LoadCustomArmData(string jsonPath)
        {
            if (!s_isDefaultArmDataLoaded)
            {
                // add to list to be loaded right after default arm data
                s_jsonsPathsToLoad ??= [];
                s_jsonsPathsToLoad.Add(jsonPath);
                return;
            }

            ModMain.Console.WriteLine($"Loading Arm Data from \"{jsonPath}\"...", MessageType.Info);
            LoadArmData(jsonPath);
        }

        internal static void LoadDefaultArmData()
        {
            if (s_isDefaultArmDataLoaded)
            {
                ModMain.Console.WriteLine("Default Arm Data is already loaded.", MessageType.Error);
                return;
            }

            ModMain.Console.WriteLine($"Loading default Arm Data...", MessageType.Info);
            LoadArmData($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/viewmodel-arm-data.json");
            s_isDefaultArmDataLoaded = true;

            if (s_jsonsPathsToLoad != null)
            {
                // load any custom arm data that was waiting for default arm data to be loaded
                foreach (var jsonPath in s_jsonsPathsToLoad)
                {
                    LoadArmData(jsonPath);
                }

                s_jsonsPathsToLoad = null;
            }
        }

        private static void LoadArmData(string jsonPath)
        {
            var newArmData = JsonConvert.DeserializeObject<Dictionary<string, ArmData>>(File.ReadAllText(jsonPath));
            if (s_armData == null)
            {
                s_armData = newArmData;
            }
            else
            {
                foreach (var armData in newArmData)
                {
                    // overwrite existing arm data
                    if (s_armData.ContainsKey(armData.Key))
                    {
                        s_armData[armData.Key] = armData.Value;
                    }
                    else
                    {
                        s_armData.Add(armData.Key, armData.Value);
                    }
                }
            }

            ModMain.Console.WriteLine($"Arm Data loaded successfully!", MessageType.Success);
        }
    }
}