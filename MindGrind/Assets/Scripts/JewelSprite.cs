using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JewelSprite : MonoBehaviour
{
    public int ReminderJewelIndex = 0;
    public int CollectedJewelIndex = 0;

    public List<SymbolSpriteTuple> SymbolToSprite;
    private SpriteRenderer _spriteRenderer = null;
    private SpriteRenderer spriteRenderer => _spriteRenderer == null ? (_spriteRenderer = GetComponent<SpriteRenderer>()) : _spriteRenderer;

    private DungeonPersistentData persistentData => DungeonPersistentData.Instance;

    public string Symbol;

    // Start is called before the first frame update
    void Start()
    {
        if (ReminderJewelIndex >= 0)
        {
            Select(persistentData.ReminderSymbolRow.Symbols[ReminderJewelIndex]);
        }
        if (CollectedJewelIndex >= 0)
        {
            Select(persistentData.CollectedSymbolRow.Symbols[CollectedJewelIndex]);
        }
    }

    public void Select(string symbol)
    {
        if (symbol == "")
        {
            Symbol = "";
            if (spriteRenderer != null)
                spriteRenderer.sprite = null;
        }

        if (spriteRenderer == null ||
            !SymbolToSprite.Any(x => x.Symbol == symbol))
        return;

        spriteRenderer.sprite = SymbolToSprite.Find(x => x.Symbol == symbol).Sprite;
        Symbol = symbol;
    }

    private void UpdateFromJewelIndex()
    {
        if (ReminderJewelIndex >= 0)
        {
            if (persistentData.ReminderSymbolRow.Symbols[ReminderJewelIndex] != Symbol)
            {
                Select(persistentData.ReminderSymbolRow.Symbols[ReminderJewelIndex]);
            }
        }
        if (CollectedJewelIndex >= 0)
        {
            if (persistentData.ReminderSymbolRow.Symbols[CollectedJewelIndex] != Symbol)
            {
                Select(persistentData.CollectedSymbolRow.Symbols[CollectedJewelIndex]);
            }
        }
    }
    private void Update()
    {
        UpdateFromJewelIndex();
    }
}

[System.Serializable]
public class SymbolSpriteTuple
{
    public string Symbol;
    public Sprite Sprite;
}