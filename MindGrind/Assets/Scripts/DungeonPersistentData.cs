using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class DungeonPersistentData : MonoBehaviour
{
    public class HistoryRow
    {
        public string[] Symbols = new string[3];
        public string[] Hints = new string[3];
    }

    internal void ResetPersistentData()
    {
        _instance = null;
        Destroy(gameObject);
        Debug.Log(Instance);
    }

    public class SymbolRow
    {
        public string[] Symbols = new string[] { "A1", "B2", "C3" };

        public void Clear() => Symbols = new string[] { "", "", "" };
    }

    public int DungeonSeed { get; set; }
    public int DungeonLevel => PuzzleHistoryRows.Count + 1;
    public int PlayerLevel => (PlayerHealth - 1) + (PlayerMagic - 1) + 1;
    public int PlayerHealth { get; set; }
    public int PlayerMagic { get; set; }
    public int PlayerExperience { get; set; }
    public SymbolRow ReminderSymbolRow { get; set; }
    public SymbolRow CollectedSymbolRow { get; set; }
    public SymbolRow CorrectSymbolRow { get; set; }

    public List<HistoryRow> PuzzleHistoryRows { get; set; }
    public int HealthCost => PlayerHealth == HealthMax ? -1 : 5 * CostTable[PlayerHealth - 1];
    public int MagicCost => PlayerMagic == MagicMax ? -1 : 10 * CostTable[PlayerMagic - 1];

    public int HealthMax => 20;
    public int MagicMax => 10;
    private int[] CostTable => Enumerable.Range(1, Mathf.Max(HealthMax, MagicMax)).Select((x, i) => Mathf.RoundToInt(x * Mathf.Pow(1.05f, i))).ToArray();
    private static DungeonPersistentData _instance;
    public static DungeonPersistentData Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject();
                go.name = "DungeonPersistenData";
                _instance = go.AddComponent<DungeonPersistentData>();

                _instance.Initialize();
            }

            return _instance;
        }
    }

    private void Initialize()
    {
        AudioListener.volume = 0.3f;

        if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DungeonSeed = new System.Random((int)System.DateTime.UtcNow.Ticks).Next(int.MinValue, int.MaxValue);

        PlayerHealth = 1;
        PlayerMagic = 1;
        PlayerExperience = 0;
        ReminderSymbolRow = new SymbolRow();
        CollectedSymbolRow = new SymbolRow();
        CollectedSymbolRow.Clear();

        PuzzleHistoryRows = new List<HistoryRow>();

        CorrectSymbolRow = new SymbolRow();
        GenerateCorrectSymbolRow();

        DontDestroyOnLoad(gameObject);
    }

    public string[] symbolTable => new string[] { "A1", "A2", "A3", "B1", "B2", "B3", "C1", "C2", "C3" };
    public string[] hintTable => new string[] { "X", "C", "O", "N" };

    private void GenerateCorrectSymbolRow()
    {
        var availableSymbols = symbolTable.ToList();
        for (int i = 0; i < 3; i++)
        {
            CorrectSymbolRow.Symbols[i] = availableSymbols[UnityEngine.Random.Range(0, availableSymbols.Count)];
            availableSymbols.Remove(CorrectSymbolRow.Symbols[i]);
        }
    }

    internal HistoryRow ValidateCollectedRow()
    {
        var historyRow = new HistoryRow()
        {
            Symbols = CollectedSymbolRow.Symbols.ToArray(),
            Hints = GenerateHints(CollectedSymbolRow)
        };

        PuzzleHistoryRows.Add(historyRow);

        return historyRow;
    }

    internal bool IsCorrect()
    {
        return CorrectSymbolRow.Symbols[0] == CollectedSymbolRow.Symbols[0] &&
               CorrectSymbolRow.Symbols[1] == CollectedSymbolRow.Symbols[1] &&
               CorrectSymbolRow.Symbols[2] == CollectedSymbolRow.Symbols[2];
    }

    private string[] GenerateHints(SymbolRow collectedSymbolRow)
    {
        var hints = new List<string>();

        var correctSymbolsToHandle = CorrectSymbolRow.Symbols.ToList();
        var currentSymbolsToHandle = collectedSymbolRow.Symbols.ToList();

        var correctLineSelection = CorrectSymbolRow.Symbols.Select((value, i) => new { i, value });
        // Check for full on correct positions
        foreach (var item in correctLineSelection)
        {
            if (item.value == collectedSymbolRow.Symbols[item.i])
            {
                hints.Add("X");
                correctSymbolsToHandle.Remove(item.value);
                currentSymbolsToHandle.Remove(item.value);
            }
        }

        // Check for correct symbol but wrong position
        foreach (var item in correctLineSelection.Where(x => correctSymbolsToHandle.Any(y => y == x.value)))
        {
            if (collectedSymbolRow.Symbols.Where(x => currentSymbolsToHandle.Any(y => y == x)).Any(x => x == item.value))
            {
                hints.Add("O");
                correctSymbolsToHandle.Remove(item.value);
                currentSymbolsToHandle.Remove(item.value);
            }
        }

        // Check for correct character 
        foreach (var item in correctLineSelection.Where(x => correctSymbolsToHandle.Any(y => y == x.value)))
        {
            if (collectedSymbolRow.Symbols.Where(x => currentSymbolsToHandle.Any(y => y == x)).Any(x => x[0] == item.value[0]))
            {
                hints.Add("C");
                correctSymbolsToHandle.Remove(item.value);
                currentSymbolsToHandle.Remove(item.value);
            }
        }

        // Check for correct number
        foreach (var item in correctLineSelection.Where(x => correctSymbolsToHandle.Any(y => y == x.value)))
        {
            if (collectedSymbolRow.Symbols.Where(x => currentSymbolsToHandle.Any(y => y == x)).Any(x => x[1] == item.value[1]))
            {
                hints.Add("N");
                correctSymbolsToHandle.Remove(item.value);
                currentSymbolsToHandle.Remove(item.value);
            }
        }

        return hints.ToArray();
    }

    private void Update()
    {
        if (Keyboard.current.allKeys.Where(x => x.keyCode == Key.M).First().wasPressedThisFrame)
        {
            AudioListener.volume = AudioListener.volume > 0 ? 0 : 0.3f;
        }
    }
}
