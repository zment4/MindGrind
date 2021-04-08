using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonPersistentData : MonoBehaviour
{
    public class HistoryRow
    {
        public string[] Symbols = new string[3];
        public string[] Hints = new string[3];
    }

    public class SymbolRow
    {
        public string[] Symbols = new string[] { "A1", "B2", "C3" };
    }

    public int DungeonSeed { get; set; }
    public int DungeonLevel { get; set; }
    public int PlayerLevel => (PlayerHealth - 1) + (PlayerMagic - 1) + 1;
    public int PlayerHealth { get; set; }
    public int PlayerMagic { get; set; }
    public int PlayerExperience { get; set; }
    public SymbolRow ReminderSymbolRow { get; set; }
    public List<HistoryRow> PuzzleHistoryRows { get; set; }
    public int HealthCost => PlayerHealth == HealthMax ? -1 : 5 * CostTable[PlayerHealth - 1];
    public int MagicCost => PlayerMagic == MagicMax ? -1 : 10 * CostTable[PlayerMagic - 1];

    public int HealthMax => 20;
    public int MagicMax => 10;
    private int[] CostTable => Enumerable.Range(1, Mathf.Max(HealthMax, MagicMax)).Select((x, i) => Mathf.RoundToInt(x * Mathf.Pow(1.05f, i))).ToArray();
    public static DungeonPersistentData Instance = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            DestroyImmediate(gameObject);
            return;
        }

        Instance = this;

        DungeonSeed = new System.Random((int)System.DateTime.UtcNow.Ticks).Next(int.MinValue, int.MaxValue);
        DungeonLevel = 1;
        PlayerHealth = 1;
        PlayerMagic = 1;
        PlayerExperience = 10000;
        ReminderSymbolRow = new SymbolRow();
        PuzzleHistoryRows = new List<HistoryRow>();

        DontDestroyOnLoad(gameObject);
    }
}
