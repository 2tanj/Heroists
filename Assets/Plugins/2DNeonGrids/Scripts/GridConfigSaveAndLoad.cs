using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class GridConfigSaveAndLoad : MonoBehaviour
{
    private List<GridConfig> gridConfigs = new List<GridConfig>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SaveGridConfig(GridConfig config)
    {
        var gridConfigJson = JsonUtility.ToJson(config);
        var savePath = Path.Combine(Application.dataPath, Guid.NewGuid().ToString() + ".json");
        File.WriteAllText(savePath, gridConfigJson);
    }

    public List<GridConfig> LoadGridConfigsFromPath(string path)
    {
        if (Directory.Exists(path))
        {
            DirectoryInfo di = new DirectoryInfo(path);
            var filesInfo = di.GetFiles("*.json", SearchOption.TopDirectoryOnly);
            var loadedNum = 0;
            foreach (FileInfo d in filesInfo)
            {
                var jsonFile = d.FullName;

                //var jsonFile = Path.Combine(d.DirectoryName, d.FullName
                var content = File.ReadAllText(jsonFile);
                var data = JsonUtility.FromJson<GridConfig>(content);
                gridConfigs.Add(data);
                loadedNum++;
            }

            Debug.Log("Loaded " + loadedNum + " grid configs");
            return gridConfigs;
        }
        else
        {
            Debug.LogError($"Path {path} does not exist to load grid configs from.");
            return new List<GridConfig>();
        }
    }
}
