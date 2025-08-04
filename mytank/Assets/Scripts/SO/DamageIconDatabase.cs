// DamageIconDatabase.cs

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageIconDatabase", menuName = "Tank Game/Damage Icon Database")]
public class DamageIconDatabase : ScriptableObject
{
    [System.Serializable]
    public class DamageIconPair
    {
        public DamageType type;
        public Sprite icon;
        public Color damageColor;
    }

    public List<DamageIconPair> iconMappings = new List<DamageIconPair>();
    private Dictionary<DamageType, Sprite> _iconDict;
    private Dictionary<DamageType, Color> _colorDict;

    public void Initialize()
    {
        _iconDict = iconMappings.ToDictionary(x => x.type, x => x.icon);
        _colorDict = iconMappings.ToDictionary(x => x.type, x => x.damageColor);
    }

    public Sprite GetIcon(DamageType type)
    {
        return _iconDict.TryGetValue(type, out var icon) ? icon : null;
    }

    public Color GetColor(DamageType type)
    {
        return _colorDict.TryGetValue(type, out var color) ? color : Color.white;
    }
}
