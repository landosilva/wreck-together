namespace WreckTogether.Editor.Tuning
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    internal readonly struct TuningEntry
    {
        public readonly string Guid;
        public readonly string AssetPath;
        public readonly string AssetName;
        public readonly string Namespace;
        public readonly ScriptableObject Asset;

        public TuningEntry(string guid, ScriptableObject asset)
        {
            Guid = guid;
            Asset = asset;
            AssetPath = AssetDatabase.GetAssetPath(asset);
            AssetName = asset.name;
            Namespace = string.IsNullOrEmpty(asset.GetType().Namespace)
                ? "(no namespace)"
                : asset.GetType().Namespace;
        }
    }

    internal static class TuningHubIndex
    {
        private static readonly string[] SearchFolders = { "Assets/_Project" };

        public static List<TuningEntry> Build()
        {
            var entries = new List<TuningEntry>();
            var guids = AssetDatabase.FindAssets("t:ScriptableObject", SearchFolders);

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset == null) continue;
                entries.Add(new TuningEntry(guid, asset));
            }

            return entries;
        }
    }
}
