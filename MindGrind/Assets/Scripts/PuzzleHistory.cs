using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleHistory : MonoBehaviour
{
    public GameObject HistoryJewelsRowPrefab;
    public List<HistoryJewelsRowHandler> HistoryRows;

    private void Start()
    {
        var y = 0;

        foreach (var row in DungeonPersistentData.Instance.PuzzleHistoryRows)
        {
            var historyRow = Instantiate(HistoryJewelsRowPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<HistoryJewelsRowHandler>();
            historyRow.transform.localPosition = new Vector3(0, y, 0);
            HistoryRows.Add(historyRow);
            for (int i = 0; i < 3; i++)
            {
                historyRow.SetJewel(i, row.Symbols[i]);
                historyRow.SetHint(i, i >= row.Hints.Length ? "" : row.Hints[i]);
            }

            y -= 16;
        }
    }

    void FillWithMockData()
    {
        var symbolTable = new string[] { "A1", "A2", "A3", "B1", "B2", "B3", "C1", "C2", "C3" };
        var hintTable = new string[] { "X", "C", "O", "N" };

        for (int k = 0; k < 8; k++)
        {
            var symbols = new List<string>();
            var hints = new List<string>();

            for (int i = 0; i < 3; i++)
            {
                symbols.Add(symbolTable[Random.Range(0, symbolTable.Length)]);
                hints.Add(hintTable[Random.Range(0, hintTable.Length)]);
            }
        }
    }
}
