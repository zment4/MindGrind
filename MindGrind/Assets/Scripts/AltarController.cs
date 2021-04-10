using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AltarController : MonoBehaviour
{
    public RoomGenerator RoomGenerator;

    [System.Serializable]
    public class SymbolSpriteTuple
    {
        public string Symbol;
        public Sprite Sprite;
    }

    public List<SymbolSpriteTuple> SymbolToSprite;
    public SpriteRenderer SpriteRenderer;
    public string Symbol;
    public Vector3 JewelPosition => SpriteRenderer.transform.position;

    public void Select(string symbol)
    {
        if (symbol == "")
        {
            Symbol = "";
            if (SpriteRenderer != null)
                SpriteRenderer.sprite = null;

            return;
        }

        if (SpriteRenderer == null ||
            !SymbolToSprite.Any(x => x.Symbol == symbol))
            return;

        SpriteRenderer.sprite = SymbolToSprite.Find(x => x.Symbol == symbol).Sprite;
        Symbol = symbol;
    }

    public void HideSymbol()
    {
        if (SpriteRenderer == null) return;

        SpriteRenderer.gameObject.SetActive(false);
    }

    public void ShowSymbol()
    {
        if (SpriteRenderer == null) return;

        SpriteRenderer.gameObject.SetActive(true);
    }

    public void ShowText()
    {
        transform.Find("DownToGet").gameObject.SetActive(true);
    }

    public void HideText()
    {
        transform.Find("DownToGet").gameObject.SetActive(false);
    }
}
