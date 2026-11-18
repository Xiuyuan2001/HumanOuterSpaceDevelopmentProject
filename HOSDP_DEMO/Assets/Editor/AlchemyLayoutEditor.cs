using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

internal static class AlchemyLayoutEditor
{
    private const string Menu = "CONTEXT/UIManager/Sync Scene Layout From Config";
    private const string UndoLabel = "Sync Alchemy Layout";

    [MenuItem(Menu, true)]
    private static bool CanSync(MenuCommand command)
    {
        var manager = (UIManager)command.context;
        return !Application.isPlaying && manager.TryGetComponent<AlchemyUI>(out _);
    }

    [MenuItem(Menu)]
    private static void Sync(MenuCommand command)
    {
        var layout = ((UIManager)command.context).GetComponent<AlchemyUI>();
        var undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(UndoLabel);
        Undo.RegisterFullObjectHierarchyUndo(layout.gameObject, UndoLabel);
        layout.BuildPreview(CreatePrefabChild);

        foreach (var component in layout.GetComponentsInChildren<Component>(true))
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(component))
                continue;

            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            if (component is Transform)
                PrefabUtility.RecordPrefabInstancePropertyModifications(component.gameObject);
        }

        EditorSceneManager.MarkSceneDirty(layout.gameObject.scene);
        Undo.CollapseUndoOperations(undoGroup);
    }

    private static GameObject CreatePrefabChild(GameObject prefab, Transform parent)
    {
        var child = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        Undo.RegisterCreatedObjectUndo(child, UndoLabel);
        return child;
    }
}
