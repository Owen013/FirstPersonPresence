using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Immersion.Objects;

public class ArmData
{
    public Vector3[] boneEulers;

    public Vector3 localPosition;

    public Vector3 localRotation;

    public float scale;

    public string shaderName;

    private static Dictionary<string, ArmData> s_armData;

    public static ArmData GetArmData(string itemName)
    {
        if (s_armData == null)
        {
            LoadArmData();
        }

        return s_armData[itemName];
    }

    private static void LoadArmData()
    {
        if (s_armData == null)
        {
            s_armData = [];

            string jsonPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "viewmodel-arm-data.json");
            JObject jsonData = JObject.Parse(File.ReadAllText(jsonPath));

            foreach (var (toolName, toolToken) in jsonData)
            {
                if (toolToken is not JObject toolObject) continue;

                var armData = new ArmData();

                if (toolObject["bone_eulers"] is JArray eulerArray)
                {
                    armData.boneEulers = eulerArray.Select(vec => new Vector3((float)vec[0], (float)vec[1], (float)vec[2])).ToArray();
                }

                if (toolObject["arm_local_position"] != null)
                {
                    armData.localPosition = new Vector3((float)toolObject["arm_local_position"][0], (float)toolObject["arm_local_position"][1], (float)toolObject["arm_local_position"][2]);
                }

                if (toolObject["arm_local_rotation"] != null)
                {
                    armData.localRotation = new Vector3((float)toolObject["arm_local_rotation"][0], (float)toolObject["arm_local_rotation"][1], (float)toolObject["arm_local_rotation"][2]);
                }

                if (toolObject["arm_scale"] != null)
                {
                    armData.scale = (float)toolObject["arm_scale"];
                }

                if (toolObject["arm_shader"] != null)
                {
                    armData.shaderName = (string)toolObject["arm_shader"];
                }

                s_armData[toolName] = armData;
            }
        }
    }
}