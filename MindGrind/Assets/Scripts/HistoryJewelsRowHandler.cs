using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HistoryJewelsRowHandler : MonoBehaviour
{
    private JewelSprite[] JewelSprites => new JewelSprite[] {
        transform.Find("HistoryJewel1").GetComponent<JewelSprite>(),
        transform.Find("HistoryJewel2").GetComponent<JewelSprite>(),
        transform.Find("HistoryJewel3").GetComponent<JewelSprite>(),
    };
    private HintSprite[] HintSprites => new HintSprite[] {
        transform.Find("HistoryHint1").GetComponent<HintSprite>(),
        transform.Find("HistoryHint2").GetComponent<HintSprite>(),
        transform.Find("HistoryHint3").GetComponent<HintSprite>(),
    };

    public void SetJewel(int index, string symbol)
    {
        JewelSprites[index].Select(symbol);
    }
    public void SetHint(int index, string symbol)
    {
        HintSprites[index].Symbol = symbol;
    }
}
