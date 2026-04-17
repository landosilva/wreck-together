namespace WreckTogether.Editor.FavoriteAssets
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    internal static class FavoriteAssetsPreferences
    {
        private const string FilePath = "UserSettings/FavoriteAssets.json";

        public static List<string> Guids { get; private set; } = new();

        static FavoriteAssetsPreferences() => Load();

        public static bool Contains(string guid) => Guids.Contains(guid);

        public static void Add(string guid)
        {
            if (string.IsNullOrEmpty(guid) || Guids.Contains(guid)) return;
            Guids.Add(guid);
            Save();
        }

        public static void Remove(string guid)
        {
            if (Guids.Remove(guid)) Save();
        }

        private static void Load()
        {
            if (!File.Exists(FilePath)) return;
            try
            {
                var data = JsonUtility.FromJson<SerializedData>(File.ReadAllText(FilePath));
                Guids = data?.Guids ?? new List<string>();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[FavoriteAssets] Failed to read preferences: {e.Message}");
            }
        }

        private static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                File.WriteAllText(FilePath, JsonUtility.ToJson(new SerializedData { Guids = new List<string>(Guids) }, prettyPrint: true));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[FavoriteAssets] Failed to save preferences: {e.Message}");
            }
        }

        [System.Serializable]
        private class SerializedData
        {
            public List<string> Guids = new();
        }
    }
}
