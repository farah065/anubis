using UnityEngine;
using UnityEditor;
using System.IO;

namespace GEM
{
    public class PowerupGenerator : EditorWindow
    {
        private string folderPath = "Assets/Resources/Powerups";

        [MenuItem("Tools/Generate Powerup Permutations")]
        public static void ShowWindow()
        {
            GetWindow<PowerupGenerator>("Powerup Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Powerup Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("Target Folder Path:", EditorStyles.label);
            folderPath = EditorGUILayout.TextField(folderPath);

            GUILayout.Space(10);

            if (GUILayout.Button("Generate All Powerup Permutations", GUILayout.Height(30)))
            {
                GeneratePowerups();
            }

            GUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "This will create a PowerupData scriptable object for each combination of " +
                "PowerupRarity and PlayerProperty. Existing assets will not be overwritten.",
                MessageType.Info
            );
        }

        private void GeneratePowerups()
        {
            // Ensure the folder exists
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                // Create the folder structure if it doesn't exist
                string[] folders = folderPath.Split('/');
                string currentPath = folders[0];

                for (int i = 1; i < folders.Length; i++)
                {
                    string newPath = currentPath + "/" + folders[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = newPath;
                }
            }

            int createdCount = 0;
            int skippedCount = 0;

            // Get all enum values
            PowerupRarity[] rarities = (PowerupRarity[])System.Enum.GetValues(typeof(PowerupRarity));
            PlayerProperty[] properties = (PlayerProperty[])System.Enum.GetValues(typeof(PlayerProperty));

            // Generate all permutations
            foreach (PowerupRarity rarity in rarities)
            {
                foreach (PlayerProperty property in properties)
                {
                    string assetName = $"{rarity}{property}";
                    string assetPath = $"{folderPath}/{assetName}.asset";

                    // Check if asset already exists
                    if (File.Exists(assetPath))
                    {
                        skippedCount++;
                        continue;
                    }

                    // Create new PowerupData
                    PowerupData powerup = ScriptableObject.CreateInstance<PowerupData>();
                    powerup.rarity = rarity;
                    powerup.property = property;
                    powerup.value = 0f; // Default value

                    // Save the asset
                    AssetDatabase.CreateAsset(powerup, assetPath);
                    createdCount++;
                }
            }

            // Save and refresh
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Show results
            EditorUtility.DisplayDialog(
                "Generation Complete",
                $"Created {createdCount} new powerup(s).\nSkipped {skippedCount} existing powerup(s).",
                "OK"
            );

            Debug.Log($"Powerup generation complete: {createdCount} created, {skippedCount} skipped.");
        }
    }
}