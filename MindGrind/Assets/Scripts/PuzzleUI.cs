using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleUI : MonoBehaviour
{
    public GameObject Player;
    public GameObject YouWin;

    public GameObject HistoryJewelsRowPrefab;
    public List<HistoryJewelsRowHandler> HistoryRows;

    private GameObject collectedJewel1 => GameObject.Find("CollectedJewel1");
    private GameObject collectedJewel2 => GameObject.Find("CollectedJewel2");
    private GameObject collectedJewel3 => GameObject.Find("CollectedJewel3");
    private GameObject[] allCollectedJewels => new GameObject[] { collectedJewel1, collectedJewel2, collectedJewel3 };

    public AudioClip LandSound;
    public AudioClip JumpSound;

    public AudioClip WinSound;
    public AudioClip DeathSound;
    public AudioClip LoseSound;

    public AudioClip JewelsWooshingSound;

    public Vector2 LoseJumpImpulse;

    public float MoveForce;
    public float JumpImpulse;

    public float[] CollectedJewelTargetXs = new float[3];
    public float CollectedJewelSpeed = 100f;
    public AnimationCurve SpeedCurve;

    private float timeToTarget;
    private float timeSpawned;
    private Vector3[] originalPosition = new Vector3[3];
    private Vector3[] targetPosition = new Vector3[3];

    private float currentY;
    private float currentX;

    // Start is called before the first frame update
    void Start()
    {
        WaitAndDo(0.2f, () => PlayerController.PlayOneShot(JewelsWooshingSound));

        timeSpawned = Time.time;
        timeToTarget = 100f / CollectedJewelSpeed;

//        FillWithMockData();

        currentY = 60;
        currentX = -8;

        foreach (var row in DungeonPersistentData.Instance.PuzzleHistoryRows)
        {
            var historyRow = Instantiate(HistoryJewelsRowPrefab, new Vector3(currentX, currentY, 0), Quaternion.identity).GetComponent<HistoryJewelsRowHandler>();
            HistoryRows.Add(historyRow);
            for (int i = 0; i < 3; i++)
            {
                historyRow.SetJewel(i, row.Symbols[i]);
                historyRow.SetHint(i, i >= row.Hints.Length ? "" : row.Hints[i]);
            }

            currentY -= 16;
        }

        for (int i = 0; i < 3; i++)
        {
            originalPosition[i] = allCollectedJewels[i].transform.position;
            targetPosition[i] = new Vector3(CollectedJewelTargetXs[i], currentY);
        }
    }

    private void WaitAndDo(float seconds, System.Action action)
    {
        StartCoroutine(WaitAndDoCoroutine(seconds, action));
    }

    private IEnumerator WaitAndDoCoroutine(float seconds, System.Action action)
    {
        yield return new WaitForSeconds(seconds);

        action();
    }
    bool allCollectedJewelsMovementFinished = false;

    private void Update()
    {
        if (allCollectedJewelsMovementFinished)
            return;

        for (int i = 0; i < 3; i++)
        {
            allCollectedJewels[i].transform.position = Vector3.Lerp(originalPosition[i], targetPosition[i], SpeedCurve.Evaluate((Time.time - timeSpawned) / timeToTarget));
            if ((allCollectedJewels[i].transform.position - targetPosition[i]).magnitude < 1)
            {
                allCollectedJewelsMovementFinished = true;

                allCollectedJewels.ToList().ForEach(x => Destroy(x));
            }
        }

        if (allCollectedJewelsMovementFinished)
        {
            var row = DungeonPersistentData.Instance.ValidateCollectedRow();

            var historyRow = Instantiate(HistoryJewelsRowPrefab, new Vector3(currentX, currentY, 0), Quaternion.identity).GetComponent<HistoryJewelsRowHandler>();
            HistoryRows.Add(historyRow);
            for (int i = 0; i < 3; i++)
            {
                historyRow.SetJewel(i, row.Symbols[i]);
                historyRow.SetHint(i, i >= row.Hints.Length ? "" : row.Hints[i]);
            }

            if (DungeonPersistentData.Instance.IsCorrect())
                StartCoroutine(ActivateWin());
            else
                StartCoroutine(ActivateLose());
        }
    }

    IEnumerator ActivateLose()
    {
        PlayerController.PlayOneShot(LoseSound);
        yield return new WaitForSeconds(0.75f);

        var playerRb = Player.GetComponent<Rigidbody2D>();
        playerRb.AddForce(LoseJumpImpulse, ForceMode2D.Impulse);
        Player.transform.rotation = Quaternion.Euler(0, 0, -90);

        PlayerController.PlayOneShot(DeathSound, () => { SceneManager.LoadScene("DungeonStart"); });
    }

    IEnumerator ActivateWin()
    {
        Player.GetComponent<BoxCollider2D>().sharedMaterial = null;
        transform.Find("Tilemap").GetComponent<CompositeCollider2D>().sharedMaterial = null;

        yield return new WaitForSeconds(0.75f);
        var winSound = PlayerController.PlayOneShot(WinSound);
        winSound.loop = true;

        YouWin.SetActive(true);
        var playerRb = Player.GetComponent<Rigidbody2D>();

        playerRb.AddForce(new Vector2(MoveForce, 0), ForceMode2D.Force);

        for (int i = 0; i < 4; i++)
        {
            PlayerController.PlayOneShot(JumpSound);
            playerRb.AddForce(new Vector2(0, JumpImpulse), ForceMode2D.Impulse);
            yield return new WaitForSeconds(0.75f);
            PlayerController.PlayOneShot(LandSound);
            yield return new WaitForSeconds(0.25f);

            if (i == 1)
                winSound.Stop();
        }

        yield return new WaitForSeconds(1f);
        playerRb.velocity = Vector2.zero;
        yield return new WaitForSeconds(1f);

        DungeonPersistentData.Instance.ResetPersistentData();
        SceneManager.LoadScene("Startup");
    }

    void FillWithMockData()
    {
        var symbolTable = new string[] { "A1", "A2", "A3", "B1", "B2", "B3", "C1", "C2", "C3" };
        var hintTable = new string[] { "X", "C", "O", "N" };

        for (int k = 0; k < 2; k++)
        {
            var symbols = new List<string>();
            var hints = new List<string>();

            for (int i = 0; i < 3; i++)
            {
                symbols.Add(symbolTable[Random.Range(0, symbolTable.Length)]);
                hints.Add(hintTable[Random.Range(0, hintTable.Length)]);
            }

            DungeonPersistentData.Instance.PuzzleHistoryRows.Add(
                new DungeonPersistentData.HistoryRow()
                {
                    Symbols = symbols.ToArray(),
                    Hints = hints.ToArray()
                }
            );
        }

        DungeonPersistentData.Instance.CollectedSymbolRow.Symbols = new string[] { "A1", "B1", "C1" };
        DungeonPersistentData.Instance.CorrectSymbolRow.Symbols = new string[] { "A1", "B1", "C1" };
    }
}
