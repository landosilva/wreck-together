namespace WreckTogether.Shared.Editor
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public static class CharacterPortraitGenerator
    {
        private const int PortraitSize = 256;
        private const string OutputFolder = "Assets/_Project/Shared/Data/Characters/Portraits";

        [MenuItem("Wreck Together/Generate Character Portraits")]
        public static void GenerateAll()
        {
            var guids = AssetDatabase.FindAssets("t:CharacterDatabase");
            if (guids.Length == 0)
            {
                Debug.LogError("[PortraitGen] No CharacterDatabase asset found.");
                return;
            }

            var db = AssetDatabase.LoadAssetAtPath<CharacterDatabase>(AssetDatabase.GUIDToAssetPath(guids[0]));
            var characters = db.GetAll();

            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            int generated = 0;
            foreach (var character in characters)
            {
                if (character.ModelPrefab == null)
                {
                    Debug.LogWarning($"[PortraitGen] {character.DisplayName} has no model prefab, skipping.");
                    continue;
                }

                var sprite = RenderPortrait(character);
                if (sprite != null)
                {
                    AssignPortrait(character, sprite);
                    generated++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[PortraitGen] Generated {generated} portrait(s).");
        }

        [MenuItem("Assets/Wreck Together/Generate Portrait", true)]
        private static bool GenerateSingleValidate()
        {
            return Selection.activeObject is CharacterDefinition;
        }

        [MenuItem("Assets/Wreck Together/Generate Portrait")]
        public static void GenerateSingle()
        {
            if (Selection.activeObject is not CharacterDefinition character)
                return;

            if (character.ModelPrefab == null)
            {
                Debug.LogError($"[PortraitGen] {character.DisplayName} has no model prefab.");
                return;
            }

            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            var sprite = RenderPortrait(character);
            if (sprite != null)
            {
                AssignPortrait(character, sprite);
                AssetDatabase.SaveAssets();
                Debug.Log($"[PortraitGen] Generated portrait for {character.DisplayName}.");
            }
        }

        private static Sprite RenderPortrait(CharacterDefinition character)
        {
            // Create isolated rendering objects
            var renderTexture = new RenderTexture(PortraitSize, PortraitSize, 24, RenderTextureFormat.ARGB32);
            renderTexture.antiAliasing = 4;

            var cameraGo = new GameObject("_PortraitCamera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.targetTexture = renderTexture;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.15f, 0.15f, 0.2f, 1f);
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 50f;
            camera.fieldOfView = 30f;

            // Add directional light
            var lightGo = new GameObject("_PortraitLight");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = Color.white;
            lightGo.transform.rotation = Quaternion.Euler(30f, -30f, 0f);

            // Add fill light from the other side
            var fillLightGo = new GameObject("_PortraitFillLight");
            var fillLight = fillLightGo.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.intensity = 0.4f;
            fillLight.color = new Color(0.7f, 0.8f, 1f);
            fillLightGo.transform.rotation = Quaternion.Euler(10f, 150f, 0f);

            // Instantiate the model facing camera
            var modelInstance = Object.Instantiate(character.ModelPrefab);
            modelInstance.transform.position = Vector3.zero;
            modelInstance.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            Sprite result = null;

            try
            {
                // Calculate bounds of the full model
                var bounds = CalculateBounds(modelInstance);

                // Frame camera on upper body / head region for a portrait feel
                var center = bounds.center;
                var height = bounds.size.y;

                // Focus on upper 30% of the model (chest and head)
                var portraitCenter = new Vector3(center.x, center.y + height * 0.15f, center.z);
                var frameSize = height * 0.55f;

                var distance = frameSize / (2f * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad));
                // Position camera in front of model (negative Z), slightly elevated and angled
                camera.transform.position = portraitCenter + new Vector3(0.1f, 0.1f, -distance * 1.1f);
                camera.transform.LookAt(portraitCenter);

                // Render
                camera.Render();

                // Read pixels
                var previousRT = RenderTexture.active;
                RenderTexture.active = renderTexture;

                var texture = new Texture2D(PortraitSize, PortraitSize, TextureFormat.RGBA32, false);
                texture.ReadPixels(new Rect(0, 0, PortraitSize, PortraitSize), 0, 0);
                texture.Apply();

                RenderTexture.active = previousRT;

                // Save as PNG
                var pngBytes = texture.EncodeToPNG();
                var fileName = $"Portrait_{character.DisplayName}.png";
                var filePath = Path.Combine(OutputFolder, fileName);
                File.WriteAllBytes(filePath, pngBytes);

                Object.DestroyImmediate(texture);

                // Import and configure as sprite
                AssetDatabase.Refresh();
                var importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.alphaIsTransparency = true;
                    importer.maxTextureSize = PortraitSize;
                    importer.mipmapEnabled = false;
                    importer.SaveAndReimport();
                }

                result = AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
            }
            finally
            {
                // Cleanup
                Object.DestroyImmediate(modelInstance);
                Object.DestroyImmediate(cameraGo);
                Object.DestroyImmediate(lightGo);
                Object.DestroyImmediate(fillLightGo);
                renderTexture.Release();
                Object.DestroyImmediate(renderTexture);
            }

            return result;
        }

        private static Bounds CalculateBounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(go.transform.position, Vector3.one);
            }

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private static void AssignPortrait(CharacterDefinition character, Sprite sprite)
        {
            var so = new SerializedObject(character);
            var prop = so.FindProperty("_portrait");
            prop.objectReferenceValue = sprite;
            so.ApplyModifiedProperties();
        }
    }
}
