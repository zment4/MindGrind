using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintSprite: MonoBehaviour
{
    public string Symbol
    {
        set {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (value == "")
            {
                spriteRenderer.sprite = null;
                return;
            }

            var sprite = SymbolToSprite.Find(x => x.Symbol == value).Sprite;
            spriteRenderer.sprite = sprite;
        }
    }

    public List<SymbolSpriteTuple> SymbolToSprite;
}
