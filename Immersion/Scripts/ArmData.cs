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
        /// Returns whether or not there is a corresponding ArmData for a given PlayerTool
        /// </summary>
        /// <param name="playerTool">The PlayerTool of the desired ArmData</param>
        /// <returns>True if there is an ArmData for the given PlayerTool, false if there isn't one</returns>
        public static bool Exists(PlayerTool playerTool)
        {
            string armDataId = FindId(playerTool);
            return ArmData.Exists(armDataId);
        }

        /// <summary>
        /// Returns whether or not there is a corresponding ArmData for a given OWItem
        /// </summary>
        /// <param name="owItem">The OWItem of the desired ArmData</param>
        /// <returns>True if there is an ArmData for the given OWItem, false if there isn't one</returns>
        public static bool Exists(OWItem owItem)
        {
            string armDataId = FindId(owItem);
            return ArmData.Exists(armDataId);
        }

        /// <summary>
        /// Returns the ArmData linked to the given ID, if it exists
        /// </summary>
        /// <param name="armDataId">The ID of the desired ArmData</param>
        /// <returns>The corresponding ArmData, if it exists</returns>
        public static ArmData Find(string armDataId)
        {
            if (ArmData.Exists(armDataId))
            {
                return s_armData[armDataId];
            }

            ModMain.Console.WriteLine($"No Arm Data found for {armDataId}", MessageType.Error);
            return null;
        }

        public static ArmData Find(PlayerTool playerTool)
        {
            if (!ArmData.Exists(playerTool))
            {
                ModMain.Console.WriteLine($"No ArmData exists for PlayerTool \"{playerTool.name}\"");
                return null;
            }

            string armDataId = ArmData.FindId(playerTool);
            return ArmData.Find(armDataId);
        }

        public static ArmData Find(OWItem owItem)
        {
            if (!ArmData.Exists(owItem))
            {
                ModMain.Console.WriteLine($"No ArmData exists for OWItem \"{owItem.name}\"");
                return null;
            }

            string armDataId = ArmData.FindId(owItem);
            return ArmData.Find(armDataId);
        }

        /// <summary>
        /// Returns the ID of the ArmData that cooresponds to the given PlayerTool, if it exists
        /// </summary>
        /// <param name="playerTool">The PlayerTool to try to get the ArmData ID for</param>
        /// <returns>The ID of the corresponding ArmData if it exists, null if it doesn't</returns>
        public static string FindId(PlayerTool playerTool)
        {
            return playerTool.name;
        }

        /// <summary>
        /// Returns the ID of the ArmData that cooresponds to the given OWItem, if it exists
        /// </summary>
        /// <param name="owItem">The OWItem to try to get the ArmData ID for</param>
        /// <returns>The ID of the corresponding ArmData if it exists, null if it doesn't</returns>
        public static string FindId(OWItem owItem)
        {
            var itemTypeName = owItem.GetItemType().GetName();
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
                    return owItem.name switch
                    {
                        "Prefab_NOM_Scroll_egg" => "Scroll_Egg",
                        "Prefab_NOM_Scroll_Jeff" => "Scroll_Jeff",
                        _ => "Scroll"
                    };
                case "ConversationStone":
                    if (owItem is NomaiConversationStone conversationStone)
                    {
                        return conversationStone.GetWord() switch
                        {
                            NomaiWord.Identify or NomaiWord.Explain => "ConversationStone_Big",
                            _ => "ConversationStone"
                        };
                    }
                    return null;
                case "WarpCore":
                    if (owItem is WarpCoreItem warpCore)
                    {
                        return warpCore.GetWarpCoreType() switch
                        {
                            WarpCoreType.Vessel or WarpCoreType.VesselBroken => "WarpCore",
                            _ => "WarpCore_Simple"
                        };
                    }
                    return null;
                case "DreamLantern":
                    if (owItem is DreamLanternItem dreamLantern)
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