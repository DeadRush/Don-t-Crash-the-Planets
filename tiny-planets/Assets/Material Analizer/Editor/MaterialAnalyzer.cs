using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


/// ---------------------------------------------------------------------------
/// <summary>
/// Analyzes the currently selected gameobjects in scene recursively and lists all materials in an EditorWindow.
/// The list allows to (mutli)select materials which automatically selects the scene game objects which use it.
/// Additionally every list item provides a button to jump to the material asset in project window.
/// </summary>
/// ---------------------------------------------------------------------------
public class MaterialAnalyzer : EditorWindow
{
    private Vector2 matNameSize;
    private float maxMatNameSize;

    private Vector2 shaderPathSize;
    private float maxShaderPathSize;

    private Vector2 matPathSize;
    private float maxMatPathSize;

    /// -----------------------------------------------------------------------
    /// <summary>
    /// Defines a single list item encapsulating a set of game objects and a selection state.
    /// </summary>
    /// -----------------------------------------------------------------------
    private class ListItem
    {
        public ListItem(bool selected = false)
        {
            this.Selected = selected;
        }

        public HashSet<GameObject> GameObjects = new HashSet<GameObject>();
        public bool Selected;
    };

    /// -----------------------------------------------------------------------
    /// <summary>
    /// Material comparer calls the material name comparer.
    /// </summary>
    /// -----------------------------------------------------------------------
    private class MaterialComp : IComparer<Material>
    {
        public int Compare(Material x, Material y)
        {
            return x.name.CompareTo(y.name);
        }
    }
    /// <summary>
    /// Stores list items by material instance.
    /// </summary>
    private SortedDictionary<Material, ListItem> mSelectionMaterials = new SortedDictionary<Material, ListItem>(new MaterialComp());

    /// <summary>
    /// The current scroll position.
    /// </summary>
    private Vector2 mScrollPosition;

    /// <summary>
    /// A text dump of the material hierarchy.
    /// </summary>
    //private string mMaterialHierarchyDump = string.Empty;

    public static bool reload = false;
    /// METHODS ===============================================================
    /// -----------------------------------------------------------------------
    /// <summary>
    /// Adds menu named "Analyze Scene" to the "Debug" menu which creates and initializes a new instance of this class.
    /// </summary>
    /// -----------------------------------------------------------------------
    [MenuItem("Tools/Material Analyzer...", false, 60)]
    public static void Init()
    {
        if (Selection.transforms.Length == 0)
        {
            Debug.LogWarning("Please select the object(s) you wish to analyze.");
            return;
        }

        GetWindow(typeof(MaterialAnalyzer), false, "Materials");

        reload = true;
    }
    void OnInspectorUpdate()
    {
        this.Repaint();
    }
    /// -----------------------------------------------------------------------
    /// <summary>
    /// Draws the GUI window.
    /// </summary>
    /// -----------------------------------------------------------------------
    private void OnGUI()
    {
        if (reload == true)
        {
            analyzeSelection();

            matNameSize = new Vector2(0, 0);
            maxMatNameSize = 0;

            shaderPathSize = new Vector2(0, 0);
            maxShaderPathSize = 0;

            matPathSize = new Vector2(0, 0);
            maxMatPathSize = 0;

            reload = false;
        }

        GUILayout.Space(10);

        GUILayout.Label("Materials: " + mSelectionMaterials.Count.ToString(), EditorStyles.boldLabel);

        GUILayout.Space(5);


        GUIContent someContent = new GUIContent();

        GUIStyle someStyle = new GUIStyle();

        someStyle.alignment = TextAnchor.MiddleLeft;
        someStyle.normal.textColor = Color.white;
        someStyle.normal.background = Resources.Load("icons/box") as Texture2D;
        someStyle.fontSize = 14;
        someStyle.fixedHeight = 33;


        mScrollPosition = EditorGUILayout.BeginScrollView(mScrollPosition);

        foreach (KeyValuePair<Material, ListItem> item in mSelectionMaterials)
        {
            GUILayout.BeginHorizontal();

            //Material icon
            //----------------------------------------------------------------------------------------------------------------------------------------

            someContent.text = null;
            someContent.tooltip = "Select the material in the project view";
            someContent.image = AssetPreview.GetMiniTypeThumbnail(typeof(Material));

            if (GUILayout.Button(someContent, GUILayout.Width(32), GUILayout.Height(32)))
            {
                // unselect all selected and select the material instance in project
                foreach (ListItem listItem in mSelectionMaterials.Values)
                    listItem.Selected = false;

                Selection.activeObject = item.Key;
            }

            //Mesh icon
            //----------------------------------------------------------------------------------------------------------------------------------------
            someContent.text = null;
            someContent.tooltip = "Select the meshes in the hierarchy.";
            someContent.image = AssetPreview.GetMiniTypeThumbnail(typeof(MeshFilter));


            if (GUILayout.Button(someContent, GUILayout.Width(32), GUILayout.Height(32)))
            {
                processListItemClick(item.Value);
            }

            //Material name
            //----------------------------------------------------------------------------------------------------------------------------------------
            someContent.text = "  " + item.Key.name + "  ";
            someContent.tooltip = "";
            someContent.image = null;

            matNameSize = someStyle.CalcSize(someContent);
            maxMatNameSize = Mathf.Max(maxMatNameSize, matNameSize.x);

            GUILayout.Label(someContent, someStyle, GUILayout.Width(maxMatNameSize), GUILayout.Height(32));


            GUILayout.Space(5);


            //Shader path
            //----------------------------------------------------------------------------------------------------------------------------------------
            someContent.text = item.Key.shader != null ? "  " + item.Key.shader.name + "  " : "  <MISSING>  ";
            someContent.tooltip = "";
            someContent.image = null;

            shaderPathSize = someStyle.CalcSize(someContent);
            maxShaderPathSize = Mathf.Max(maxShaderPathSize, shaderPathSize.x);


            GUILayout.Label(someContent, someStyle, GUILayout.Width(maxShaderPathSize), GUILayout.Height(32));


            GUILayout.Space(5);

            //Material path in project
            //----------------------------------------------------------------------------------------------------------------------------------------

            someContent.text = "  " + AssetDatabase.GetAssetPath(item.Key.GetInstanceID()).Replace(".mat", "").Replace("Assets/", "") + "  ";
            //someContent.text = someContent.text.Replace(".mat", "");
            someContent.tooltip = "";
            someContent.image = null;

            matPathSize = someStyle.CalcSize(someContent);
            maxMatPathSize = Mathf.Max(maxMatPathSize, matPathSize.x);

            GUILayout.Label(someContent, someStyle, GUILayout.Width(maxMatPathSize), GUILayout.Height(32));

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }

    /// -----------------------------------------------------------------------
    /// <summary>
    /// Processes the list item click.
    /// </summary>
    /// <param name='itemClicked'>
    /// The item clicked.
    /// </param>
    /// -----------------------------------------------------------------------
    private void processListItemClick(ListItem itemClicked)
    {
        Event e = Event.current;

        // if shift/control is pressed just add this element
        if (e.control)
        {
            itemClicked.Selected = !itemClicked.Selected;
            updateSceneSelection();
        }
        else
        {
            // unselect all selected and select this
            foreach (ListItem listItem in mSelectionMaterials.Values)
                listItem.Selected = false;

            itemClicked.Selected = true;
            updateSceneSelection();
        }
    }

    /// -----------------------------------------------------------------------
    /// <summary>
    /// Starts recursive analyze process iterating through every selected GameObject.
    /// </summary>
    /// -----------------------------------------------------------------------
    private void analyzeSelection()
    {
        mSelectionMaterials.Clear();

        StringBuilder dump = new StringBuilder();
        foreach (Transform transform in Selection.transforms)
            analyzeGameObject(transform.gameObject, dump, "");

        //mMaterialHierarchyDump = dump.ToString();
    }

    /// -----------------------------------------------------------------------
    /// <summary>
    /// Analyzes the given game object.
    /// </summary>
    /// <param name='gameObject'>
    /// The game object to analyze.
    /// </param>
    /// -----------------------------------------------------------------------
    private void analyzeGameObject(GameObject gameObject, StringBuilder dump, string indent)
    {
        dump.Append(indent + gameObject.name + "\n");

        foreach (Component component in gameObject.GetComponents<Component>())
            analyzeComponent(component, dump, indent + "    ");

        foreach (Transform child in gameObject.transform)
            analyzeGameObject(child.gameObject, dump, indent + "    ");
    }

    /// -----------------------------------------------------------------------
    /// <summary>
    /// Analyzes the given component.
    /// </summary>
    /// <param name='component'>
    /// The component to analyze.
    /// </param>
    /// -----------------------------------------------------------------------
    private void analyzeComponent(Component component, StringBuilder dump, string indent)
    {
        // early out if component is missing
        if (component == null)
            return;

        List<Material> materials = new List<Material>();
        switch (component.GetType().ToString())
        {
            case "UnityEngine.MeshRenderer":
                {
                    MeshRenderer mr = component as MeshRenderer;
                    foreach (Material mat in mr.sharedMaterials)
                        materials.Add(mat);
                }
                break;
            case "UnityEngine.ParticleSystemRenderer":
                {
                    ParticleSystemRenderer pr = component as ParticleSystemRenderer;
                    foreach (Material mat in pr.sharedMaterials)
                        materials.Add(mat);
                }
                break;
            default:
                break;
        }

        bool materialMissing = false;
        foreach (Material mat in materials)
        {
            if (mat == null)
            {
                materialMissing = true;
                dump.Append(indent + "> MISSING\n");
            }
            else
            {
                ListItem item;
                mSelectionMaterials.TryGetValue(mat, out item);
                if (item == null)
                {
                    item = new ListItem();
                    mSelectionMaterials.Add(mat, item);
                }
                item.GameObjects.Add(component.gameObject);

                string matName = mat.shader != null ?
                mat.name + " <" + mat.shader.name + ">" :
                mat.name + " <MISSING>";
                dump.Append(indent + "> " + matName + "\n");
            }
        }

        if (materialMissing)
            Debug.LogWarning("Material(s) missing in game object '" + component.gameObject + "'!");
    }

    private void updateSceneSelection()
    {
        HashSet<UnityEngine.Object> sceneObjectsToSelect = new HashSet<UnityEngine.Object>();
        foreach (ListItem item in mSelectionMaterials.Values)
            if (item.Selected)
                foreach (GameObject go in item.GameObjects)
                    sceneObjectsToSelect.Add(go);

        UnityEngine.Object[] array = new UnityEngine.Object[sceneObjectsToSelect.Count];
        sceneObjectsToSelect.CopyTo(array);
        Selection.objects = array;
    }
}