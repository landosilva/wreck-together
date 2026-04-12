namespace WreckTogether.Shared
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "WreckTogether/Character Database")]
    public class CharacterDatabase : ScriptableObject
    {
        [SerializeField] private CharacterDefinition[] _characters;

        public int Count => _characters.Length;

        public CharacterDefinition Get(int index)
        {
            if (index < 0 || index >= _characters.Length)
            {
                Debug.LogWarning($"[CharacterDatabase] Index {index} out of range, falling back to 0.");
                return _characters[0];
            }
            return _characters[index];
        }

        public CharacterDefinition[] GetAll() => _characters;
    }
}
