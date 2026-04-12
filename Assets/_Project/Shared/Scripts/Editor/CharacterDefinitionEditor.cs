namespace WreckTogether.Shared.Editor
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(CharacterDefinition))]
    public class CharacterDefinitionEditor : Editor
    {
        private SerializedProperty _displayName;
        private SerializedProperty _modelPrefab;
        private SerializedProperty _portrait;

        private void OnEnable()
        {
            _displayName = serializedObject.FindProperty("_displayName");
            _modelPrefab = serializedObject.FindProperty("_modelPrefab");
            _portrait = serializedObject.FindProperty("_portrait");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_displayName);
            EditorGUILayout.PropertyField(_modelPrefab);
            EditorGUILayout.PropertyField(_portrait);

            // Portrait preview
            var sprite = _portrait.objectReferenceValue as Sprite;
            if (sprite != null)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("Portrait Preview", EditorStyles.boldLabel);

                var rect = GUILayoutUtility.GetRect(128, 128, GUILayout.ExpandWidth(false));
                rect.x = (EditorGUIUtility.currentViewWidth - 128) * 0.5f;

                var texRect = sprite.textureRect;
                var texCoords = new Rect(
                    texRect.x / sprite.texture.width,
                    texRect.y / sprite.texture.height,
                    texRect.width / sprite.texture.width,
                    texRect.height / sprite.texture.height
                );

                GUI.DrawTextureWithTexCoords(rect, sprite.texture, texCoords);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
