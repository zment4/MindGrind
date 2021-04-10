using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomGenerator : MonoBehaviour
{
    public AudioClip ActivateLastExitSound;

    public List<JewelSprite> CollectedJewelSprites = new List<JewelSprite>();

    public GameObject EnemyBat;

    public class RoomClass
    {
        public int Id;
        public List<ExitClass> Exits = new List<ExitClass>();
        public bool Generated;
        public int[] VariationIndex = new int[6];
        public int AltarSegment;
        public string Symbol;

        public class ExitClass
        {
            public class TargetClass
            {
                public int RoomId;
                public int ExitId;
            }

            public int Id; // Up on topleft corner is 0, 10 is Left on top-left corner
            public TargetClass Target;
        }
    }

    private List<IEnemy> enemies = new List<IEnemy>();

    public void ActivateExit(int exitNumber, Vector3 deltaVector)
    {
        if (DungeonPersistentData.Instance.CollectedSymbolRow.Symbols.Count(x => x != "") == 3)
        {
            GameObject.Find("Player").GetComponent<Rigidbody2D>().gravityScale = 0;
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = ActivateLastExitSound;
            audioSource.Play();
            StartCoroutine(WaitAudioAndLoadScene(audioSource));
            return;
        }
        EnterRoom(CurrentRoom.Exits.Find(x => x.Id == exitNumber).Target, deltaVector);
    }

    IEnumerator WaitAudioAndLoadScene(AudioSource audioSource)
    {
        while (audioSource.isPlaying) yield return null;

        SceneManager.LoadScene("PuzzleUI");
    }

    private void EnterRoom(RoomClass.ExitClass.TargetClass target, Vector3 deltaVector)
    {
        enemies.ForEach(x => { if (x != null && x.GameObject) Destroy(x.GameObject); });
        enemies.Clear();

        ShowRoom(Rooms.Find(x => x.Id == target.RoomId));
        Player.transform.position = SpawnHandler.GetSpawnPoint(target.ExitId) + deltaVector;
        CollectedJewelSprites.ForEach(x => x.GetComponent<Rigidbody2D>().position = Player.transform.position);

        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        if (EnemyBat == null)
            return;

        var rand = new System.Random(CurrentRoom.Id);
        var enemyCount = rand.Next(0, 3);

        for (int i = 0; i < enemyCount; i++)
        {
            var enemyBatGo = Instantiate(EnemyBat, new Vector3((float)rand.NextDouble() * 256f - 128f, (float)rand.NextDouble() * 208f - 104f, 0), Quaternion.identity);
            var enemyBat = enemyBatGo.GetComponent<EnemyBat>();
            enemyBat.Level = DungeonPersistentData.Instance.DungeonLevel;
            enemies.Add(enemyBat);
        }
    }

    private void ShowRoom(RoomClass room)
    {
        for (int i = 0; i < room.VariationIndex.Length; i++)
        {
            AllSegments[i].Select(room.VariationIndex[i]);
        }
        SetAltarSegment(room.AltarSegment, room.Symbol);

        currentRoomId = room.Id;
    }

    private int currentRoomId = 0;
    public RoomClass CurrentRoom => Rooms.Find(x => x.Id == currentRoomId);  
    private List<RoomClass> Rooms = new List<RoomClass>();

    private List<string> possibleSymbols = new List<string>() { "A1", "A2", "A3", "B1", "B2", "B3", "C1", "C2", "C3" };
    private List<string> availableSymbols;

    public SegmentController Segment1;
    public SegmentController Segment2;
    public SegmentController Segment3;
    public SegmentController Segment4;
    public SegmentController Segment5;
    public SegmentController Segment6;

    public GameObject Altar;

    public GameObject Player;
    public SpawnHandler SpawnHandler;

    private SegmentController[] AllSegments => new SegmentController[] {
        Segment1, Segment2, Segment3, Segment4, Segment5, Segment6
    };

    public void Awake()
    {
        GenerateDungeon(DungeonPersistentData.Instance.DungeonSeed);

        ShowRoom(Rooms[0]);
    }

    public enum ExitDirectionEnum
    {
        Up,
        Down,
        Left,
        Right
    }

    List<int> UpExits = new List<int>() { 0, 1, 2 };
    List<int> DownExits = new List<int>() { 5, 6, 7 };
    List<int> LeftExits = new List<int>() { 8, 9 };
    List<int> RightExits = new List<int>() { 3, 4 };

    public void GenerateDungeon(int seed)
    {
        Rooms.Clear();

        Random.InitState(seed);

        availableSymbols = possibleSymbols.ToList();

        GenerateRoom(null, null);
    }

    private void GenerateNoExits(List<int> availableSegments, RoomClass newRoom)
    {
        foreach (var segment in availableSegments)
        {
            newRoom.VariationIndex[segment] = 8;
        }
    }

    private int[] exitPairs = new int[10] { 9, 1, 3, 2, 5, 4, 6, 8, 7, 0 };
    private int[] exitToSegment = new int[10] { 0, 1, 2, 2, 5, 5, 4, 3, 3, 0 };
    private int[,] exitToVariationIndex = new int[10, 2]
    {
            {0, 2}, {0, 0}, {0, 3}, {5, 3}, {5, 6},
            {4, 6}, {4, 4}, {4, 7}, {1, 7}, {1, 2},
    };

    private void GenerateExits(RoomClass.ExitClass requiredExit, int numOfExits, List<int> availableSegments, List<int> availableExits, RoomClass newRoom)
    {
        var exitIds = new List<int>();

        if (requiredExit != null)
        {
            newRoom.Exits.Add(requiredExit);

            availableExits.Remove(requiredExit.Id);
            exitIds.Add(requiredExit.Id);
        }

        for (int i = 0; i < numOfExits; i++)
        {
            var exitId = availableExits[Random.Range(0, availableExits.Count)];
            availableExits.Remove(exitId);
            exitIds.Add(exitId);
        }

        foreach (var exitId in exitIds)
        {
            if (requiredExit != null && 
                requiredExit.Id == exitId) 
                continue;

            newRoom.Exits.Add(new RoomClass.ExitClass()
            {
                Id = exitId,
                Target = new RoomClass.ExitClass.TargetClass()
                {
                    ExitId = GetRandomOppositeExit(exitId),
                    RoomId = CreateRoom().Id
                }
            });
        }

        while(exitIds.Count > 0)
        {
            var exitId = exitIds[0];
            exitIds.Remove(exitId);

            var exitPair = exitPairs[exitId];
            var segmentId = exitToSegment[exitId];

            availableSegments.Remove(segmentId);

            var variationIndexSelector = 0;
            if (exitIds.Any(x => x == exitPair))
            {
                exitIds.Remove(exitPair);
                variationIndexSelector = 1;
            }

            newRoom.VariationIndex[segmentId] = exitToVariationIndex[exitId, variationIndexSelector];
        }
    }

    internal void ActivateExitGlow()
    {
        transform.Find("ExitGlow").gameObject.SetActive(true);
    }

    private int GetRandomOppositeExit(int exitId)
    {
        List<int> returnExits = null;

        if (UpExits.Any(x => x == exitId))
            returnExits = DownExits;

        if (DownExits.Any(x => x == exitId))
            returnExits = UpExits;

        if (LeftExits.Any(x => x == exitId))
            returnExits = RightExits;

        if (RightExits.Any(x => x == exitId))
            returnExits = LeftExits;

        return returnExits[Random.Range(0, returnExits.Count)];
    }
    private void GenerateAltarSegment(List<int> availableSegments, List<int> availableExits, RoomClass newRoom)
    {
        var altarSegment = availableSegments[Random.Range(0, availableSegments.Count)];
        availableSegments.Remove(altarSegment);
        newRoom.VariationIndex[altarSegment] = 9;
        newRoom.AltarSegment = altarSegment;

        switch (altarSegment)
        {
            case 0:
                availableExits.Remove(0);
                availableExits.Remove(9);
                break;
            case 1:
                availableExits.Remove(1);
                break;
            case 2:
                availableExits.Remove(2);
                availableExits.Remove(3);
                break;
            case 3:
                availableExits.Remove(8);
                availableExits.Remove(7);
                break;
            case 4:
                availableExits.Remove(6);
                break;
            case 5:
                availableExits.Remove(5);
                availableExits.Remove(4);
                break;
        }
    }

    private void SetAltarSegment(int altarSegment, string symbol)
    {
        var altarPositions = new Vector3[] {
            new Vector3(0, 0),
            new Vector3(80, 0),
            new Vector3(160, 0),
            new Vector3(0, -96),
            new Vector3(80, -96),
            new Vector3(160, -96),
        };
        Altar.transform.position = altarPositions[altarSegment];

        Debug.Log(symbol);

        Altar.GetComponent<AltarController>().Select(symbol);

        Altar.SetActive(true);
    }

    private RoomClass CreateRoom()
    {
        var newRoom = new RoomClass();
        var newId = Random.Range(0, int.MaxValue);

        while (Rooms.Any(x => x.Id == newId)) 
            newId = Random.Range(0, int.MaxValue);

        newRoom.Id = newId;

        newRoom.Symbol = availableSymbols[Random.Range(0, availableSymbols.Count)];
        availableSymbols.Remove(newRoom.Symbol);

        Rooms.Add(newRoom);

        return newRoom;
    }

    public void GenerateRoom(RoomClass roomToGenerate, RoomClass.ExitClass requiredExit)
    {
        if (roomToGenerate == null)
        {
            roomToGenerate = CreateRoom();
        }

        if (roomToGenerate.Generated)
            return;

        List<int> availableSegments = Enumerable.Range(0, 6).ToList();
        List<int> availableExits = Enumerable.Range(0, 10).ToList();

        if (requiredExit != null)
        {
            availableExits.Remove(requiredExit.Id);
            availableSegments.Remove(exitToSegment[requiredExit.Id]);
        }

        GenerateAltarSegment(availableSegments, availableExits, roomToGenerate);

        var numExits = Mathf.Min(Random.Range(1, 4), availableSymbols.Count);
        GenerateExits(requiredExit, numExits, availableSegments, availableExits, roomToGenerate);

        GenerateNoExits(availableSegments, roomToGenerate);

        roomToGenerate.Generated = true;

        foreach (var exit in roomToGenerate.Exits)
        {
            var newRoom = Rooms.Find(x => x.Id == exit.Target.RoomId);
            GenerateRoom(newRoom, new RoomClass.ExitClass()
            {
                Id = exit.Target.ExitId,
                Target = new RoomClass.ExitClass.TargetClass() {
                    ExitId = exit.Id,
                    RoomId = roomToGenerate.Id
                }
            });
        }
    }
}
