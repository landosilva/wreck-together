namespace WreckTogether.Shared
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "WreckTogether/Character Definition")]
    public class CharacterDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private GameObject _modelPrefab;
        [SerializeField] private Sprite _portrait;

        public string DisplayName => _displayName;
        public GameObject ModelPrefab => _modelPrefab;
        public Sprite Portrait => _portrait;
    }
}
