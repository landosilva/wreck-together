namespace WreckTogether.Editor.FavoriteAssets
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// An asset tray: drag any asset in from the Project window, click rows to
    /// ping / double-click to open, drag out of the window to drop into the
    /// inspector or scene. State persists per-user in UserSettings.
    /// </summary>
    public class FavoriteAssetsWindow : EditorWindow
    {
        private const float DragStartDistance = 6f;

        private VisualElement _list;
        private VisualElement _listContainer;
        private Label _emptyState;

        private string _draggingGuid;
        private Vector2 _mouseDownPos;

        [MenuItem("Wreck Together/Favorite Assets")]
        public static void Open()
        {
            var window = GetWindow<FavoriteAssetsWindow>();
            window.titleContent = new GUIContent("Favorites", EditorGUIUtility.IconContent("d_Favorite").image);
            window.minSize = new Vector2(220, 200);
            window.Show();
        }

        private void CreateGUI()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Project/Editor/FavoriteAssets/FavoriteAssetsWindow.uxml");
            var uss  = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/_Project/Editor/FavoriteAssets/FavoriteAssetsWindow.uss");
            uxml.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(uss);

            _list          = rootVisualElement.Q<VisualElement>("list");
            _listContainer = rootVisualElement.Q<VisualElement>("root");
            _emptyState    = rootVisualElement.Q<Label>("empty-state");

            RegisterDropZone(_listContainer);
            Rebuild();
        }

        private void OnFocus() => Rebuild();

        // ---------- Rendering ----------

        private void Rebuild()
        {
            if (_list == null) return;
            _list.Clear();

            var guids = FavoriteAssetsPreferences.Guids;
            _emptyState.style.display = guids.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;

            foreach (var guid in guids)
                _list.Add(CreateRow(guid));
        }

        private VisualElement CreateRow(string guid)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadMainAssetAtPath(path);

            var row = new VisualElement();
            row.AddToClassList("favs-row");
            if (asset == null) row.AddToClassList("favs-row--missing");

            var icon = new Image
            {
                image = asset != null ? AssetDatabase.GetCachedIcon(path) : EditorGUIUtility.IconContent("console.warnicon.sml").image,
                scaleMode = ScaleMode.ScaleToFit,
            };
            icon.AddToClassList("favs-row-icon");
            row.Add(icon);

            var label = new Label(asset != null ? asset.name : "(missing asset)");
            label.AddToClassList("favs-row-label");
            label.tooltip = string.IsNullOrEmpty(path) ? $"Unknown GUID: {guid}" : path;
            row.Add(label);

            var remove = new Button(() =>
            {
                FavoriteAssetsPreferences.Remove(guid);
                Rebuild();
            }) { text = "×", tooltip = "Remove from favorites" };
            remove.AddToClassList("favs-row-remove");
            row.Add(remove);

            WireRowInteractions(row, guid, asset);
            return row;
        }

        private void WireRowInteractions(VisualElement row, string guid, Object asset)
        {
            if (asset == null) return;

            row.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.target is Button) return;
                if (evt.button != 0) return;

                if (evt.clickCount == 2)
                {
                    AssetDatabase.OpenAsset(asset);
                    evt.StopPropagation();
                    return;
                }

                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
                _draggingGuid = guid;
                _mouseDownPos = evt.mousePosition;
            });

            row.RegisterCallback<MouseMoveEvent>(evt =>
            {
                if (_draggingGuid != guid) return;
                if ((evt.mousePosition - _mouseDownPos).sqrMagnitude < DragStartDistance * DragStartDistance) return;

                StartDragOut(asset);
                _draggingGuid = null;
            });

            row.RegisterCallback<MouseUpEvent>(_ => _draggingGuid = null);
        }

        private static void StartDragOut(Object asset)
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = new[] { asset };
            DragAndDrop.paths = new[] { AssetDatabase.GetAssetPath(asset) };
            DragAndDrop.StartDrag(asset.name);
        }

        // ---------- Drop zone ----------

        private void RegisterDropZone(VisualElement zone)
        {
            zone.RegisterCallback<DragEnterEvent>(_ => zone.AddToClassList("favs-list--drag-over"));
            zone.RegisterCallback<DragLeaveEvent>(_ => zone.RemoveFromClassList("favs-list--drag-over"));

            zone.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                DragAndDrop.visualMode = HasAnyAssetInDrag() ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                evt.StopPropagation();
            });

            zone.RegisterCallback<DragPerformEvent>(evt =>
            {
                DragAndDrop.AcceptDrag();
                var added = AddDraggedAssets();
                zone.RemoveFromClassList("favs-list--drag-over");
                if (added > 0) Rebuild();
                evt.StopPropagation();
            });
        }

        private static bool HasAnyAssetInDrag()
        {
            foreach (var path in DragAndDrop.paths)
                if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path)))
                    return true;
            return false;
        }

        private static int AddDraggedAssets()
        {
            var added = 0;
            var seen = new HashSet<string>();
            foreach (var path in DragAndDrop.paths)
            {
                if (string.IsNullOrEmpty(path)) continue;
                var guid = AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrEmpty(guid) || !seen.Add(guid)) continue;
                if (FavoriteAssetsPreferences.Contains(guid)) continue;

                FavoriteAssetsPreferences.Add(guid);
                added++;
            }
            return added;
        }
    }
}
