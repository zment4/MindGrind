using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JewelSprite : MonoBehaviour
{
    public int ReminderJewelIndex = 0;

    [System.Serializable]
    public class SymbolSpriteTuple
    {
        public string Symbol;
        public Sprite Sprite;
    }
    public List<SymbolSpriteTuple> SymbolToSprite;
    private SpriteRenderer _spriteRenderer = null;
    private SpriteRenderer spriteRenderer => _spriteRenderer == null ? (_spriteRenderer = GetComponent<SpriteRenderer>()) : _spriteRenderer;

    private DungeonPersistentData persistentData => DungeonPersistentData.Instance;

    public string Symbol;

    // Start is called before the first frame update
    void Start()
    {
        Select(persistentData.ReminderSymbolRow.Symbols[ReminderJewelIndex]);
    }

    public void Select(string symbol)
    {
        if (spriteRenderer == null ||
            !SymbolToSprite.Any(x => x.Symbol == symbol))
            return;

        spriteRenderer.sprite = SymbolToSprite.Find(x => x.Symbol == symbol).Sprite;
        Symbol = symbol;
        persistentData.ReminderSymbolRow.Symbols[ReminderJewelIndex] = Symbol;
    }

    private void Update()
    {
        if (persistentData.ReminderSymbolRow.Symbols[ReminderJewelIndex] != Symbol)
        {
            Select(persistentData.ReminderSymbolRow.Symbols[ReminderJewelIndex]);
        }
    }
}
