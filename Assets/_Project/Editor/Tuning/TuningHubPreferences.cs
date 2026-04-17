namespace WreckTogether.Editor.Tuning
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    internal static class TuningHubPreferences
    {
        private const string FilePath = "UserSettings/TuningHubPreferences.json";

        public static HashSet<string> PinnedGuids { get; private set; } = new();
        public static HashSet<string> HiddenAssetGuids { get; private set; } = new();
        public static HashSet<string> HiddenNamespaces { get; private set; } = new();
        public static HashSet<string> CollapsedNamespaces { get; private set; } = new();
        public static List<string> NamespaceOrder { get; private set; } = new();

        static TuningHubPreferences() => Load();

        public static bool IsPinned(string guid) => PinnedGuids.Contains(guid);
        public static bool IsAssetHidden(string guid) => HiddenAssetGuids.Contains(guid);
        public static bool IsNamespaceHidden(string ns) => HiddenNamespaces.Contains(ns);
        public static bool IsNamespaceCollapsed(string ns) => CollapsedNamespaces.Contains(ns);

        public static void TogglePin(string guid)
        {
            if (!PinnedGuids.Add(guid)) PinnedGuids.Remove(guid);
            Save();
        }

        public static void SetNamespaceHidden(string ns, bool hidden)
        {
            if (hidden) HiddenNamespaces.Add(ns);
            else HiddenNamespaces.Remove(ns);
            Save();
        }

        public static void SetAssetHidden(string guid, bool hidden)
        {
            if (hidden) HiddenAssetGuids.Add(guid);
            else HiddenAssetGuids.Remove(guid);
            Save();
        }

        public static void SetNamespaceCollapsed(string ns, bool collapsed)
        {
            if (collapsed) CollapsedNamespaces.Add(ns);
            else CollapsedNamespaces.Remove(ns);
            Save();
        }

        public static void MoveNamespace(string ns, int direction)
        {
            var current = NamespaceOrder.IndexOf(ns);
            if (current < 0)
            {
                NamespaceOrder.Add(ns);
                current = NamespaceOrder.Count - 1;
            }
            var target = Mathf.Clamp(current + direction, 0, NamespaceOrder.Count - 1);
            if (target == current) return;

            NamespaceOrder.RemoveAt(current);
            NamespaceOrder.Insert(target, ns);
            Save();
        }

        public static void EnsureNamespaceTracked(string ns)
        {
            if (!NamespaceOrder.Contains(ns))
            {
                NamespaceOrder.Add(ns);
                Save();
            }
        }

        private static void Load()
        {
            if (!File.Exists(FilePath)) return;

            try
            {
                var data = JsonUtility.FromJson<SerializedData>(File.ReadAllText(FilePath));
                if (data == null) return;
                PinnedGuids         = new HashSet<string>(data.Pinned              ?? new List<string>());
                HiddenAssetGuids    = new HashSet<string>(data.HiddenAssetGuids    ?? new List<string>());
                HiddenNamespaces    = new HashSet<string>(data.HiddenNamespaces    ?? new List<string>());
                CollapsedNamespaces = new HashSet<string>(data.CollapsedNamespaces ?? new List<string>());
                NamespaceOrder      = data.NamespaceOrder                          ?? new List<string>();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[TuningHub] Failed to read preferences: {e.Message}");
            }
        }

        private static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                var data = new SerializedData
                {
                    Pinned              = new List<string>(PinnedGuids),
                    HiddenAssetGuids    = new List<string>(HiddenAssetGuids),
                    HiddenNamespaces    = new List<string>(HiddenNamespaces),
                    CollapsedNamespaces = new List<string>(CollapsedNamespaces),
                    NamespaceOrder      = new List<string>(NamespaceOrder),
                };
                File.WriteAllText(FilePath, JsonUtility.ToJson(data, prettyPrint: true));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[TuningHub] Failed to save preferences: {e.Message}");
            }
        }

        [System.Serializable]
        private class SerializedData
        {
            public List<string> Pinned = new();
            public List<string> HiddenAssetGuids = new();
            public List<string> HiddenNamespaces = new();
            public List<string> CollapsedNamespaces = new();
            public List<string> NamespaceOrder = new();
        }
    }
}
