using UnityEditor;
using UnityEngine;
using System.IO;

public class ProjectFolderGenerator
{
    [MenuItem("OctOrbit/Setup/Generate Folders")]
    public static void Generate()
    {
        string root = "_Project";
        string[] folders = {
            "Animations", "Audio", "Data", "Prefabs", "Shaders",
            "Scripts/Core", "Scripts/Editor", "Scripts/Gameplay", 
            "Scripts/Models", "Scripts/Utils"
        };

        foreach (string folder in folders)
        {
            string path = Path.Combine(Application.dataPath, root, folder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"Folder Created: {path}");
            }
        }
        AssetDatabase.Refresh();
        Debug.Log("<b>OctOrbit:</b> 모든 기본 폴더 구조가 생성되었습니다.");
    }
}