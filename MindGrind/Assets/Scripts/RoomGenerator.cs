using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
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

    public void ActivateExit(int exitNumber)
    {
        EnterRoom(currentRoom.Exits.Find(x => x.Id == exitNumber).Target);
    }

    private void EnterRoom(RoomClass.ExitClass.TargetClass target)
    {
    }

    private void ShowRoom(RoomClass room)
    {
        for (int i = 0; i < room.VariationIndex.Length; i++)
        {
            AllSegments[i].Select(room.VariationIndex[i]);
        }
        SetAltarSegment(room.AltarSegment, room.Symbol);
    }

    private int currentRoomId = 0;
    private RoomClass currentRoom => Rooms.Find(x => x.Id == currentRoomId);  
    private List<RoomClass> Rooms = new List<RoomClass>();

    private List<string> possibleSymbols = new List<string>() { "A1", "A2", "A3", "B1", "B2", "B3", "C1", "C2", "C3" };
    private List<string> availableSymbols;

    public bool bGenerateRoom = false;

    public SegmentController Segment1;
    public SegmentController Segment2;
    public SegmentController Segment3;
    public SegmentController Segment4;
    public SegmentController Segment5;
    public SegmentController Segment6;

    public GameObject Altar;

    private SegmentController[] AllSegments => new SegmentController[] {
        Segment1, Segment2, Segment3, Segment4, Segment5, Segment6
    };

    public void Awake()
    {
    }

    public void OnValidate()
    {
        if (!bGenerateRoom)
            return;
        bGenerateRoom = false;

        GenerateDungeon(Time.frameCount);

        ShowRoom(Rooms.Last());
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

        var newRoom = new RoomClass();

        List<int> availableSegments = Enumerable.Range(0, 6).ToList();
        List<int> availableExits = Enumerable.Range(0, 10).ToList();

        GenerateAltarSegment(availableSegments, availableExits, newRoom);

        GenerateExits(Random.Range(1, 4), availableSegments, availableExits, newRoom);

        GenerateNoExits(availableSegments, newRoom);

        newRoom.Generated = true;

        Rooms.Add(newRoom);
    }

    private void GenerateNoExits(List<int> availableSegments, RoomClass newRoom)
    {
        foreach (var segment in availableSegments)
        {
            newRoom.VariationIndex[segment] = 8;
        }
    }

    private void GenerateExits(int numOfExits, List<int> availableSegments, List<int> availableExits, RoomClass newRoom)
    {
        var exitIds = new List<int>();

        for (int i = 0; i < numOfExits; i++)
        {
            var exitId = availableExits[Random.Range(0, availableExits.Count)];
            availableExits.Remove(exitId);
            exitIds.Add(exitId);
        }

        foreach (var exitId in exitIds)
        {
            newRoom.Exits.Add(new RoomClass.ExitClass()
            {
                Id = exitId,
                Target = new RoomClass.ExitClass.TargetClass()
                {
                    ExitId = GetRandomOppositeExit(exitId),
                    RoomId = GetNewRoomId()
                }
            });
        }

        var exitPairs = new int[10] { 9, 1, 3, 2, 5, 4, 6, 8, 7, 0 };
        var segmentTable = new int[10] { 0, 1, 2, 2, 5, 5, 4, 3, 3, 0 };
        var exitToVariationIndex = new int[10,2]
        {
            {0, 2}, {0, 0}, {0, 3}, {5, 3}, {5, 6}, 
            {4, 6}, {4, 4}, {4, 7}, {1, 7}, {1, 2},
        };

        while(exitIds.Count > 0)
        {
            var exitId = exitIds[0];
            exitIds.Remove(exitId);

            var exitPair = exitPairs[exitId];
            var segmentId = segmentTable[exitId];

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
        var altarSegment = Random.Range(0, 6);
        availableSegments.Remove(altarSegment);
        newRoom.VariationIndex[altarSegment] = 9;
        newRoom.AltarSegment = altarSegment;
        newRoom.Symbol = possibleSymbols[Random.Range(0, possibleSymbols.Count)];
        availableSymbols.Remove(newRoom.Symbol);

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

        Altar.GetComponent<AltarController>().Select(symbol);

        Altar.SetActive(true);
    }

    private int GetNewRoomId()
    {
        var newRoom = new RoomClass();
        var newId = 0;

        while (Rooms.Any(x => x.Id == newId)) 
            newId = Random.Range(0, int.MaxValue);

        Rooms.Add(newRoom);

        return newRoom.Id;
    }

    public void GenerateRoom(ExitDirectionEnum requiredExitDirection, RoomClass.ExitClass.TargetClass requiredExitTarget)
    {

    }
}
