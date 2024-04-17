using UnityEditor;
using UnityEngine;
using System.IO;

namespace AhabTools
{
    public class RenamerWindow : EditorWindow
    {
        private enum IterationType
        {
            SELECTED_OBJECTS,
            SELECTED_FOLDER
        }
        private IterationType iterationType;
        private string originalNamePart = "";
        private string newNamePart = "";
        private string prefix = "";
        private string suffix = "";
        public bool renamerFoldout;
        public bool apendixFoldout;
        private GUIStyle foldoutHeaderStyle;
        private GUIStyle foldoutContentStyle;

        [MenuItem("Tools/Ahab Tools/A Simple Renamer")]
        public static void ShowWindow()
        {
            GetWindow<RenamerWindow>("A Simple Renamer v1.0.0");
        }

        void OnGUI()
        {
            #region How to use infobox
            EditorGUILayout.HelpBox("Choose a method depending on if you want to rename only the current " +
                "selected objects in the Project Tab or the current selected Folder also in the Project Tab.", MessageType.Info);
            #endregion
            #region Operation Type
            // Toggle buttons for iteration type
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(iterationType == IterationType.SELECTED_OBJECTS, "Selected Objects", "Button"))
            {
                iterationType = IterationType.SELECTED_OBJECTS;
            }
            if (GUILayout.Toggle(iterationType == IterationType.SELECTED_FOLDER, "Selected Folder", "Button"))
            {
                iterationType = IterationType.SELECTED_FOLDER;
            }
            GUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(10);
            #region Foldout Styles
            if (foldoutHeaderStyle == null)
            {
                foldoutHeaderStyle = new GUIStyle("ShurikenModuleTitle")
                {
                    border = new RectOffset(2, 2, 2, 2),
                    fixedHeight = 22,
                    contentOffset = new Vector2(20f, -2f)
                };
                foldoutHeaderStyle.contentOffset = new Vector2(20f, -2f);
                foldoutHeaderStyle.fixedWidth = 0;
            }

            if (foldoutContentStyle == null)
            {
                foldoutContentStyle = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(1, 1, 1, 1),
                    margin = new RectOffset(1, 1, 1, 1)
                };
            }
            #endregion
            #region Foldout drawing 1
            Rect backgroundRect9 = GUILayoutUtility.GetRect(1f, 22f, GUILayout.ExpandWidth(true));
            GUI.Box(backgroundRect9, GUIContent.none, foldoutHeaderStyle);
            renamerFoldout = EditorGUI.Foldout(backgroundRect9, renamerFoldout, "Renaming Options", true, foldoutHeaderStyle);
            #endregion
            if (renamerFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUI.DrawRect(EditorGUILayout.BeginVertical(EditorStyles.helpBox), new Color(0.18f, 0.18f, 0.18f, 1f));
                GUILayout.BeginHorizontal();
                GUILayout.Label("Original Name Part:", GUILayout.Width(150));
                originalNamePart = GUILayout.TextField(originalNamePart);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("New Name Part:", GUILayout.Width(150));
                newNamePart = GUILayout.TextField(newNamePart);
                GUILayout.EndHorizontal();
                GUIContent buttonContent1 = new GUIContent("Rename Files", "Changes cannot be undone with Control+Z. Careful.");
                if (GUILayout.Button(buttonContent1, GUILayout.Height(40)))
                {
                    switch (iterationType)
                    {
                        case IterationType.SELECTED_OBJECTS:
                            RenameSelectedObjects();
                            break;
                        case IterationType.SELECTED_FOLDER:
                            RenameFilesInSelectedFolder();
                            break;
                        default:
                            break;
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }

            #region Foldout drawing 2
            Rect backgroundRect1 = GUILayoutUtility.GetRect(1f, 22f, GUILayout.ExpandWidth(true));
            GUI.Box(backgroundRect1, GUIContent.none, foldoutHeaderStyle);
            apendixFoldout = EditorGUI.Foldout(backgroundRect1, apendixFoldout, "Suffix/Prefix Options", true, foldoutHeaderStyle);
            #endregion
            if (apendixFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUI.DrawRect(EditorGUILayout.BeginVertical(EditorStyles.helpBox), new Color(0.18f, 0.18f, 0.18f, 1f));

                GUILayout.BeginHorizontal();
                GUILayout.Label("Prefix:", GUILayout.Width(150));
                prefix = GUILayout.TextField(prefix);
                GUILayout.EndHorizontal();

                GUIContent buttonContent2 = new GUIContent("Add Prefix", "Changes cannot be undone with Control+Z. Careful.");
                if (GUILayout.Button(buttonContent2, GUILayout.Height(40)))
                {
                    switch (iterationType)
                    {
                        case IterationType.SELECTED_OBJECTS:
                            AddPrefixToSelectedObjects();
                            break;
                        case IterationType.SELECTED_FOLDER:
                            AddPrefixToSelectedFolder();
                            break;
                        default:
                            break;
                    }
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Suffix:", GUILayout.Width(150));
                suffix = GUILayout.TextField(suffix);
                GUILayout.EndHorizontal();

                GUIContent buttonContent3 = new GUIContent("Add Sufix", "Changes cannot be undone with Control+Z. Careful.");
                if (GUILayout.Button(buttonContent3, GUILayout.Height(40)))
                {
                    switch (iterationType)
                    {
                        case IterationType.SELECTED_OBJECTS:
                            AddSuffixToSelectedObjects();
                            break;
                        case IterationType.SELECTED_FOLDER:
                            AddSuffixToSelectedFolder();
                            break;
                        default:
                            break;
                    }
                }

                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;

            }
        }

        #region Auxiliar methods
        private void AddSuffixToSelectedObjects()
        {
            if (Selection.objects == null || Selection.objects.Length == 0)
            {
                Debug.LogWarning("No objects are currently selected.");
                return;
            }

            AssetDatabase.StartAssetEditing();
            foreach (var selectedObject in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(selectedObject);
                if (!string.IsNullOrEmpty(path) && !path.EndsWith(".meta"))
                {
                    string extension = Path.GetExtension(path);
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
                    string newName = fileNameWithoutExtension + suffix + extension;
                    string newPath = Path.Combine(Path.GetDirectoryName(path), newName);
                    AssetDatabase.RenameAsset(path, newName);
                    Debug.Log($"Added suffix to {fileNameWithoutExtension}, new name {newName}");
                }
            }
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        private void AddSuffixToSelectedFolder()
        {
            string path = GetSelectedFolderPath();
            if (path != null && !string.IsNullOrEmpty(suffix))
            {
                string[] fileEntries = Directory.GetFiles(path);
                AssetDatabase.StartAssetEditing();
                foreach (string filePath in fileEntries)
                {
                    if (!filePath.EndsWith(".meta"))
                    {
                        string extension = Path.GetExtension(filePath);
                        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                        string newName = fileNameWithoutExtension + suffix + extension;
                        string newPath = Path.Combine(Path.GetDirectoryName(filePath), newName);
                        AssetDatabase.RenameAsset(filePath, newName);
                        Debug.Log($"Added suffix to {fileNameWithoutExtension}, new name {newName}");
                    }
                }
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
            else if (string.IsNullOrEmpty(suffix))
            {
                Debug.LogWarning("Suffix is empty. Please enter a suffix to add.");
            }
        }
        private void AddPrefixToSelectedObjects()
        {
            if (Selection.objects == null || Selection.objects.Length == 0)
            {
                Debug.LogWarning("No objects are currently selected.");
                return;
            }

            AssetDatabase.StartAssetEditing();
            foreach (var selectedObject in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(selectedObject);
                if (!string.IsNullOrEmpty(path) && !path.EndsWith(".meta"))
                {
                    string fileName = Path.GetFileName(path);
                    string newName = prefix + fileName;
                    string newPath = Path.Combine(Path.GetDirectoryName(path), newName);
                    AssetDatabase.RenameAsset(path, newName);
                    Debug.Log($"Added prefix to {fileName}, new name {newName}");
                }
            }
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        private void AddPrefixToSelectedFolder()
        {
            string path = GetSelectedFolderPath();
            if (path != null && !string.IsNullOrEmpty(prefix))
            {
                string[] fileEntries = Directory.GetFiles(path);
                AssetDatabase.StartAssetEditing();
                foreach (string filePath in fileEntries)
                {
                    if (!filePath.EndsWith(".meta"))
                    {
                        string fileName = Path.GetFileName(filePath);
                        string newName = prefix + fileName;
                        string newPath = Path.Combine(Path.GetDirectoryName(filePath), newName);
                        AssetDatabase.RenameAsset(filePath, newName);
                        Debug.Log($"Added prefix to {fileName}, new name {newName}");
                    }
                }
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
            else if (string.IsNullOrEmpty(prefix))
            {
                Debug.LogWarning("Prefix is empty. Please enter a prefix to add.");
            }
        }

        private void RenameSelectedObjects()
        {
            if (Selection.objects == null || Selection.objects.Length == 0)
            {
                Debug.LogWarning("No objects are currently selected.");
                return;
            }

            AssetDatabase.StartAssetEditing();
            foreach (var selectedObject in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(selectedObject);
                if (!string.IsNullOrEmpty(path) && !path.EndsWith(".meta"))
                {
                    string fileName = Path.GetFileName(path);
                    if (fileName.Contains(originalNamePart))
                    {
                        string newName = fileName.Replace(originalNamePart, newNamePart);
                        string newPath = Path.Combine(Path.GetDirectoryName(path), newName);
                        AssetDatabase.RenameAsset(path, newName);
                        Debug.Log($"Renamed {fileName} to {newName}");
                    }
                }
            }
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }
        private void RenameFilesInSelectedFolder()
        {
            string path = GetSelectedFolderPath();
            if (path != null && !string.IsNullOrEmpty(originalNamePart))
            {
                string[] fileEntries = Directory.GetFiles(path);
                AssetDatabase.StartAssetEditing(); // Begin grouping asset database changes
                foreach (string filePath in fileEntries)
                {
                    if (!filePath.EndsWith(".meta"))
                    {
                        string fileName = Path.GetFileName(filePath);
                        if (fileName.Contains(originalNamePart))
                        {
                            string newName = fileName.Replace(originalNamePart, newNamePart);
                            string newPath = Path.Combine(Path.GetDirectoryName(filePath), newName);
                            AssetDatabase.RenameAsset(filePath, newName);
                            Debug.Log($"Renamed {fileName} to {newName}");
                        }
                    }
                }
                AssetDatabase.StopAssetEditing(); // End grouping asset database changes
                AssetDatabase.Refresh(); // Refresh the Asset Database to show the new file names
            }
            else if (string.IsNullOrEmpty(originalNamePart))
            {
                Debug.LogWarning("Original name part is empty. Please enter the part of the file names you want to replace.");
            }
        }

        private string GetSelectedFolderPath()
        {
            if (Selection.activeObject == null)
            {
                Debug.LogWarning("No folder or object is currently selected.");
                return null;
            }

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (AssetDatabase.IsValidFolder(path))
            {
                return path;
            }
            else
            {
                Debug.LogWarning("The selected item is not a folder.");
                return null;
            }
        }
        #endregion
    }
}
