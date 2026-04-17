namespace WreckTogether.Editor.Tuning
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class TuningHubWindow : EditorWindow
    {
        private const string SelectedGuidKey = "WreckTogether.TuningHub.SelectedGuid";
        private const string PinnedGroupKey  = "★ Pinned";

        private VisualElement _tree;
        private Label _inspectorTitle;
        private IMGUIContainer _inspectorImgui;
        private Button _hiddenToggle;
        private Button _refreshButton;

        private UnityEditor.Editor _currentEditor;
        private string _selectedGuid;
        private List<TuningEntry> _entries = new();
        private bool _showingHidden;

        [MenuItem("Wreck Together/Tuning Hub %#t")]
        public static void Open()
        {
            var window = GetWindow<TuningHubWindow>();
            window.titleContent = new GUIContent("Tuning Hub", EditorGUIUtility.IconContent("d_SettingsIcon").image);
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        private void CreateGUI()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Project/Editor/Tuning/TuningHubWindow.uxml");
            var uss  = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/_Project/Editor/Tuning/TuningHubWindow.uss");
            uxml.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(uss);

            _tree           = rootVisualElement.Q<VisualElement>("tree");
            _inspectorTitle = rootVisualElement.Q<Label>("inspector-title");
            _inspectorImgui = rootVisualElement.Q<IMGUIContainer>("inspector-imgui");
            _hiddenToggle   = rootVisualElement.Q<Button>("hidden-toggle");
            _refreshButton  = rootVisualElement.Q<Button>("refresh-button");

            _hiddenToggle.Add(new Image { image = EditorGUIUtility.IconContent("d_scenevis_hidden_hover").image });
            _refreshButton.Add(new Image { image = EditorGUIUtility.IconContent("d_Refresh").image });

            _inspectorImgui.onGUIHandler = DrawSelectedInspector;
            _refreshButton.clicked += Rebuild;
            _hiddenToggle.clicked  += ToggleShowHidden;

            UpdateHiddenToggleState();
            _selectedGuid = EditorPrefs.GetString(SelectedGuidKey, null);
            Rebuild();
        }

        private void OnDisable()
        {
            if (_currentEditor != null) DestroyImmediate(_currentEditor);
        }

        private void ToggleShowHidden()
        {
            _showingHidden = !_showingHidden;
            UpdateHiddenToggleState();
            Rebuild();
        }

        private void UpdateHiddenToggleState()
        {
            _hiddenToggle.EnableInClassList("tuning-icon-button--active", _showingHidden);
            _hiddenToggle.tooltip = _showingHidden
                ? "Showing hidden items — click to hide them again"
                : "Show hidden items (dimmed)";
        }

        // ---------- Build ----------

        private void Rebuild()
        {
            _entries = TuningHubIndex.Build();
            foreach (var ns in _entries.Select(e => e.Namespace).Distinct())
                TuningHubPreferences.EnsureNamespaceTracked(ns);

            _tree.Clear();
            BuildPinnedGroup();
            foreach (var ns in OrderedNamespaces())
                BuildNamespaceGroup(ns, _entries.Where(e => e.Namespace == ns));

            if (!string.IsNullOrEmpty(_selectedGuid) && _entries.Any(e => e.Guid == _selectedGuid))
                Select(_entries.First(e => e.Guid == _selectedGuid), refreshTree: false);
        }

        private IEnumerable<string> OrderedNamespaces()
        {
            var present = new HashSet<string>(_entries.Select(e => e.Namespace));
            foreach (var ns in TuningHubPreferences.NamespaceOrder)
            {
                if (!present.Contains(ns)) continue;
                if (TuningHubPreferences.IsNamespaceHidden(ns) && !_showingHidden) continue;
                yield return ns;
            }
        }

        private void BuildPinnedGroup()
        {
            var pinned = _entries
                .Where(e => TuningHubPreferences.IsPinned(e.Guid))
                .Where(e => !IsEntryHiddenForCurrentView(e))
                .ToList();
            if (pinned.Count == 0) return;

            var (wrapper, foldout, _) = CreateGroup(PinnedGroupKey, pinned.Count, isPinnedGroup: true, isHiddenNamespace: false, namespaceName: null);
            foreach (var entry in pinned.OrderBy(e => e.AssetName))
                foldout.Add(CreateItem(entry));
            _tree.Add(wrapper);
        }

        private void BuildNamespaceGroup(string ns, IEnumerable<TuningEntry> entries)
        {
            var visible = entries.Where(e => !IsEntryHiddenForCurrentView(e)).OrderBy(e => e.AssetName).ToList();
            if (visible.Count == 0) return;

            var isHiddenNs = TuningHubPreferences.IsNamespaceHidden(ns);
            var (wrapper, foldout, _) = CreateGroup(ns, visible.Count, isPinnedGroup: false, isHiddenNamespace: isHiddenNs, namespaceName: ns);
            foreach (var entry in visible)
                foldout.Add(CreateItem(entry));
            _tree.Add(wrapper);
        }

        private bool IsEntryHiddenForCurrentView(TuningEntry entry)
        {
            if (_showingHidden) return false;
            if (TuningHubPreferences.IsAssetHidden(entry.Guid)) return true;
            if (TuningHubPreferences.IsNamespaceHidden(entry.Namespace)) return true;
            return false;
        }

        // ---------- Group (foldout) ----------

        private (VisualElement wrapper, Foldout foldout, VisualElement headerRow) CreateGroup(
            string name, int count, bool isPinnedGroup, bool isHiddenNamespace, string namespaceName)
        {
            var wrapper = new VisualElement();
            wrapper.AddToClassList("tuning-group");
            if (isPinnedGroup)     wrapper.AddToClassList("tuning-group--pinned");
            if (isHiddenNamespace) wrapper.AddToClassList("tuning-group--hidden");

            var foldout = new Foldout
            {
                text = $"{name}  ({count})",
                value = !TuningHubPreferences.IsNamespaceCollapsed(name),
            };
            foldout.RegisterValueChangedCallback(e =>
                TuningHubPreferences.SetNamespaceCollapsed(name, !e.newValue));

            var toggle = foldout.Q<Toggle>();
            VisualElement headerRow = null;

            if (toggle != null && namespaceName != null)
            {
                headerRow = new VisualElement();
                headerRow.AddToClassList("tuning-group-header-row");
                headerRow.pickingMode = PickingMode.Ignore; // let clicks reach the Toggle beneath

                var spacer = new VisualElement();
                spacer.AddToClassList("tuning-group-spacer");
                spacer.pickingMode = PickingMode.Ignore;
                headerRow.Add(spacer);

                var hideBtn = CreateHideButton(
                    isCurrentlyHidden: isHiddenNamespace,
                    onToggle: () =>
                    {
                        TuningHubPreferences.SetNamespaceHidden(namespaceName, !isHiddenNamespace);
                        Rebuild();
                    });
                headerRow.Add(hideBtn);

                toggle.style.flexDirection = FlexDirection.Row;
                toggle.Add(headerRow);
            }

            wrapper.Add(foldout);
            return (wrapper, foldout, headerRow);
        }

        // ---------- Items ----------

        private VisualElement CreateItem(TuningEntry entry)
        {
            var row = new VisualElement();
            row.AddToClassList("tuning-item");
            if (entry.Guid == _selectedGuid) row.AddToClassList("tuning-item--selected");
            if (TuningHubPreferences.IsAssetHidden(entry.Guid)) row.AddToClassList("tuning-item--hidden");

            var pinButton = new Button();
            pinButton.AddToClassList("tuning-item-pin");
            pinButton.text = TuningHubPreferences.IsPinned(entry.Guid) ? "★" : "☆";
            if (TuningHubPreferences.IsPinned(entry.Guid))
                pinButton.AddToClassList("tuning-item-pin--on");
            pinButton.clicked += () =>
            {
                TuningHubPreferences.TogglePin(entry.Guid);
                Rebuild();
            };
            row.Add(pinButton);

            var label = new Label(entry.AssetName);
            label.AddToClassList("tuning-item-label");
            label.tooltip = entry.AssetPath;
            row.Add(label);

            row.Add(CreatePingButton(entry));

            var isHidden = TuningHubPreferences.IsAssetHidden(entry.Guid);
            var hideBtn = CreateHideButton(
                isCurrentlyHidden: isHidden,
                onToggle: () =>
                {
                    TuningHubPreferences.SetAssetHidden(entry.Guid, !isHidden);
                    Rebuild();
                });
            row.Add(hideBtn);

            row.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.target is Button) return;

                if (evt.clickCount == 2)
                {
                    PingAsset(entry);
                    evt.StopPropagation();
                    return;
                }

                Select(entry, refreshTree: true);
            });

            return row;
        }

        private static Button CreatePingButton(TuningEntry entry)
        {
            var btn = new Button(() => PingAsset(entry))
            {
                tooltip = "Show in Project",
            };
            btn.AddToClassList("tuning-row-btn");
            btn.Add(new Image { image = EditorGUIUtility.IconContent("d_Search Icon").image });
            return btn;
        }

        private static void PingAsset(TuningEntry entry)
        {
            if (entry.Asset == null) return;
            EditorGUIUtility.PingObject(entry.Asset);
            Selection.activeObject = entry.Asset;
        }

        private static Button CreateHideButton(bool isCurrentlyHidden, System.Action onToggle)
        {
            var btn = new Button(onToggle)
            {
                tooltip = isCurrentlyHidden ? "Unhide" : "Hide",
            };
            btn.AddToClassList("tuning-row-btn");
            var iconName = isCurrentlyHidden ? "d_scenevis_hidden_hover" : "d_scenevis_visible_hover";
            btn.Add(new Image { image = EditorGUIUtility.IconContent(iconName).image });
            return btn;
        }

        private void Select(TuningEntry entry, bool refreshTree)
        {
            _selectedGuid = entry.Guid;
            EditorPrefs.SetString(SelectedGuidKey, _selectedGuid);

            if (_currentEditor != null) DestroyImmediate(_currentEditor);
            _currentEditor = UnityEditor.Editor.CreateEditor(entry.Asset);

            _inspectorTitle.text = $"{entry.AssetName}   ({entry.Namespace})";
            _inspectorImgui.MarkDirtyRepaint();

            if (refreshTree) HighlightSelection();
        }

        private void HighlightSelection()
        {
            foreach (var item in _tree.Query<VisualElement>(className: "tuning-item").ToList())
            {
                item.RemoveFromClassList("tuning-item--selected");
                var label = item.Q<Label>(className: "tuning-item-label");
                if (label != null && label.tooltip == AssetDatabase.GUIDToAssetPath(_selectedGuid))
                    item.AddToClassList("tuning-item--selected");
            }
        }

        private void DrawSelectedInspector()
        {
            if (_currentEditor == null) return;
            _currentEditor.OnInspectorGUI();
        }
    }
}
