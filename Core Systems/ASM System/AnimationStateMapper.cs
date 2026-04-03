using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AnimationStateMapper : EditorWindow
{
    private AnimationStateMapperSO selectedMapperSO;
    private List<AnimationStateMapperSO> allMappers = new();

    private Vector2 leftScrollPos;
    private Vector2 rightScrollPos;

    // Splitter variables
    private float splitterPos = 220f;
    private bool draggingSplitter = false;
    private readonly float splitterWidth = 6f;

    [MenuItem("Tools/Animation State Mapper")]
    public static void ShowWindow()
    {
        GetWindow<AnimationStateMapper>("Animation State Mapper");
    }

    private void OnEnable()
    {
        RefreshMapperList();
    }

    private void RefreshMapperList()
    {
        string[] guids = AssetDatabase.FindAssets("t:AnimationStateMapperSO");
        allMappers = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<AnimationStateMapperSO>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(so => so != null)
            .ToList();
    }

    private void OnGUI()
    {
        GUILayout.Label("Animation State Mapper Tool", EditorStyles.boldLabel);

        Rect windowRect = position;
        Rect leftRect = new Rect(0, 22, splitterPos, windowRect.height - 22);
        Rect splitterRect = new Rect(splitterPos, 22, splitterWidth, windowRect.height - 22);
        Rect rightRect = new Rect(splitterPos + splitterWidth, 22, windowRect.width - splitterPos - splitterWidth, windowRect.height - 22);

        // --- LEFT PANEL ---
        GUILayout.BeginArea(leftRect);
        leftScrollPos = EditorGUILayout.BeginScrollView(leftScrollPos);

        if (GUILayout.Button("Create New Mapper", GUILayout.Height(30)))
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Animation State Mapper", "NewASM", "asset", "Choose location");
            if (!string.IsNullOrEmpty(path))
            {
                var newSO = CreateInstance<AnimationStateMapperSO>();
                AssetDatabase.CreateAsset(newSO, path);
                AssetDatabase.SaveAssets();
                RefreshMapperList();
                selectedMapperSO = newSO;
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("Existing Mappers:", EditorStyles.boldLabel);

        foreach (var mapper in allMappers)
        {
            Rect btnRect = GUILayoutUtility.GetRect(new GUIContent(mapper.name), EditorStyles.label, GUILayout.Height(22));
            GUIStyle style = (mapper == selectedMapperSO) ? EditorStyles.toolbarButton : EditorStyles.label;
            if (GUI.Button(btnRect, mapper.name, style))
            {
                selectedMapperSO = mapper;
            }
        }
        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();

        // --- SPLITTER ---
        EditorGUI.DrawRect(splitterRect, Color.gray);
        EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);

        // Handle splitter drag
        if (Event.current.type == EventType.MouseDown && splitterRect.Contains(Event.current.mousePosition))
        {
            draggingSplitter = true;
        }
        if (draggingSplitter)
        {
            splitterPos = Mathf.Clamp(Event.current.mousePosition.x, 120f, windowRect.width - 120f);
            Repaint();
            if (Event.current.type == EventType.MouseUp)
                draggingSplitter = false;
        }

        // --- RIGHT PANEL ---
        GUILayout.BeginArea(rightRect);
        rightScrollPos = EditorGUILayout.BeginScrollView(rightScrollPos);

        GUILayout.Space(5);

        if (selectedMapperSO != null)
        {
            EditorGUILayout.ObjectField("Mapper Asset", selectedMapperSO, typeof(AnimationStateMapperSO), false);

            EditorGUI.BeginChangeCheck();
            selectedMapperSO.objectTypeName = EditorGUILayout.TextField("Object Type Name", selectedMapperSO.objectTypeName);

            // --- PATH SELECTION SECTION ---
            GUILayout.Space(10);
            GUILayout.Label("Generated Class Save Path:", EditorStyles.boldLabel);

            // Show the current path in a text field (read-only)
            EditorGUILayout.TextField("Save Path", selectedMapperSO.PathToSaveGeneratedClass);

            // Button to pick new path
            if (GUILayout.Button("Choose Save Path", GUILayout.Height(25)))
            {
                string newPath = EditorUtility.OpenFolderPanel("Choose Save Location for Generated Class",
                    selectedMapperSO.PathToSaveGeneratedClass, "");

                if (!string.IsNullOrEmpty(newPath))
                {
                    // Convert absolute path to relative path (Assets/...)
                    if (newPath.StartsWith(Application.dataPath))
                    {
                        newPath = "Assets" + newPath.Substring(Application.dataPath.Length);
                    }

                    // Ensure path ends with /
                    if (!newPath.EndsWith("/"))
                        newPath += "/";

                    selectedMapperSO.PathToSaveGeneratedClass = newPath;
                    EditorUtility.SetDirty(selectedMapperSO);
                }
            }

            var so = new SerializedObject(selectedMapperSO);
            so.Update();

            // --- DRAG & DROP AREA ---
            GUILayout.Space(10);
            GUILayout.Label("Drag Animation Clips Here", EditorStyles.helpBox);
            Rect dropArea = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drop AnimationClips Here", EditorStyles.centeredGreyMiniLabel);

            if (dropArea.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (Event.current.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        foreach (var obj in DragAndDrop.objectReferences)
                        {
                            if (obj is AnimationClip clip)
                            {
                                // Check for duplicates
                                if (!selectedMapperSO.GetAnimationStateMaps().Any(m => m.clip == clip))
                                {
                                    selectedMapperSO.GetAnimationStateMaps().Add(new AnimationStateMap(clip));
                                    EditorUtility.SetDirty(selectedMapperSO);
                                }
                            }
                        }
                        so.Update(); // Refresh the inspector
                        Event.current.Use();
                    }
                }
            }

            // Only show the animationStateMaps list now
            EditorGUILayout.PropertyField(
                so.FindProperty("animationStateMaps"),
                new GUIContent("Animation State Maps"),
                true);

            if (GUILayout.Button("Generate Animation Keys Class", GUILayout.Height(30)))
            {
                GenerateAnimationKeysClass(selectedMapperSO);
            }

            if (EditorGUI.EndChangeCheck())
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(selectedMapperSO);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Select or create an AnimationStateMapper asset.", MessageType.Info);
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void GenerateAnimationKeysClass(AnimationStateMapperSO mapperSO)
    {
        if (mapperSO == null)
            return;

        if (string.IsNullOrEmpty(mapperSO.objectTypeName))
        {
            Debug.LogError("Please enter the object type name (e.g., 'Player').");
            return;
        }

        if (string.IsNullOrEmpty(mapperSO.PathToSaveGeneratedClass))
        {
            Debug.LogError("Please set a save path using the 'Choose Save Path' button.");
            return;
        }

        string className = mapperSO.GetGeneratedClassName();
        string fileName = className + ".cs";
        string fullPath = System.IO.Path.Combine(mapperSO.PathToSaveGeneratedClass, fileName);

        // Ensure directory exists
        string directoryPath = System.IO.Path.GetDirectoryName(fullPath);
        if (!System.IO.Directory.Exists(directoryPath))
        {
            System.IO.Directory.CreateDirectory(directoryPath);
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("// Auto-generated by AnimationStateMapper");
        sb.AppendLine($"public static class {className}");
        sb.AppendLine("{");

        foreach (var map in mapperSO.GetAnimationStateMaps())
        {
            if (map.isIncludedInUsage && !string.IsNullOrEmpty(map.key))
            {
                string safeKey = map.key.Replace(" ", "").Replace("-", "_");
                sb.AppendLine($"    public const string {safeKey} = \"{map.key}\";");
            }
        }

        sb.AppendLine("}");

        System.IO.File.WriteAllText(fullPath, sb.ToString());
        AssetDatabase.Refresh();

        Debug.Log($"Generated {fullPath}");
    }
}