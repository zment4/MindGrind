using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using System;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class DungeonStartMenu : MonoBehaviour
{
    public AudioClip EnterDungeon;
    public AudioClip MenuActivate;
    public AudioClip MenuCancel;
    public AudioClip MenuSelection;

    [System.Serializable]
    public class Label
    {
        public enum LabelJustification
        {
            Left,
            Right
        }
        public string Id;
        public Vector2Int Position;
        public LabelJustification Justification;
    }

    public Tile[] Digits;
    public List<Label> Labels = new List<Label>();

    public bool Active = true;

    public float BlinkOnTime = 1f;
    public float BlinkOffTime = 0.25f;
    private bool isBlinkOnPeriod;

    public Tile TileEmpty;
    public Tile TileSelectionMarker;

    public List<MenuItem> MenuItems = new List<MenuItem>();

    public MenuItem CurrentSelection;

    [System.Serializable]
    public class InputsContainer
    {
        public InputAction Up;
        public InputAction Down;
        public InputAction Left;
        public InputAction Right;
        public InputAction Activate;
        public InputAction Cancel;

        public List<InputAction> AllInputs => new List<InputAction>() { Up, Down, Left, Right, Activate, Cancel };

        internal void Enable() => AllInputs.ForEach(x => x.Enable());
    }

    public InputsContainer Inputs;

    private Tilemap _tilemap;
    private Tilemap tilemap => _tilemap == null ? (_tilemap = GetComponent<Tilemap>()) : _tilemap;
    private AudioSource _audioSource;
    private AudioSource audioSource => _audioSource == null ? (_audioSource = GetComponent<AudioSource>()) : _audioSource;
    
    private Dictionary<string, Action> MenuIdAction;

    // Start is called before the first frame update
    void Start()
    {
        blinkToggleTime = Time.time;
        MenuIdAction = new Dictionary<string, Action>()
        {
            {"Enter", () => {             
                audioSource.clip = EnterDungeon;
                audioSource.Play();
                StartCoroutine(LoadSceneAfterAudio()); } },
            {"Health", () => { BuyHealth(); } },
            {"Magic", () => { BuyMagic(); } },
        };

        Inputs.Up.performed += _ => Select(CurrentSelection.TargetUp);
        Inputs.Down.performed += _ => Select(CurrentSelection.TargetDown);
        Inputs.Left.performed += _ => Select(CurrentSelection.TargetLeft);
        Inputs.Right.performed += _ => Select(CurrentSelection.TargetRight);
        Inputs.Activate.performed += _ => ActivateCurrentSelection();
        Inputs.Enable();

        CurrentSelection = MenuItems.First();

        SetLabel("Health", DungeonPersistentData.Instance.PlayerHealth);
        SetLabel("Magic", DungeonPersistentData.Instance.PlayerMagic);
        SetLabel("Experience", DungeonPersistentData.Instance.PlayerExperience);
        SetLabel("HealthCost", DungeonPersistentData.Instance.HealthCost);
        SetLabel("MagicCost", DungeonPersistentData.Instance.MagicCost);
        SetLabel("PlayerLevel", DungeonPersistentData.Instance.PlayerLevel);
        SetLabel("DungeonLevel", DungeonPersistentData.Instance.DungeonLevel);
    }

    private IEnumerator LoadSceneAfterAudio()
    {
        while (audioSource.isPlaying)
            yield return null;

        SceneManager.LoadScene("GameScene");
    }

    private void SetLabel(string labelId, int number, bool clear = false)
    {
        bool clearLabel = number == -1 || clear;
        if (number == -1) number = 999;

        var label = Labels.Find(x => x.Id == labelId);

        var curX = label.Position.x;
        if (label.Justification == Label.LabelJustification.Left)
            curX += $"{number}".Length - 1;

        do
        {
            var digit = number % 10;
            var tileToSet = clearLabel ? null : Digits[digit];
            tilemap.SetTile(new Vector3Int(curX, label.Position.y, 0), tileToSet);
            number -= digit;
            number /= 10;
            curX -= 1;
        } while (number > 0);
    }

    private void BuyHealth()
    {
        if (DungeonPersistentData.Instance.PlayerHealth == DungeonPersistentData.Instance.HealthMax)
        {
            audioSource.clip = MenuCancel;
            audioSource.Play();
            return;
        }

        var cost = DungeonPersistentData.Instance.HealthCost;
        if (cost <= DungeonPersistentData.Instance.PlayerExperience)
        {
            audioSource.clip = MenuActivate;
            audioSource.Play();

            SetLabel("Experience", DungeonPersistentData.Instance.PlayerExperience, true);

            DungeonPersistentData.Instance.PlayerExperience -= cost;
            DungeonPersistentData.Instance.PlayerHealth += 1;

            SetLabel("Experience", DungeonPersistentData.Instance.PlayerExperience);
            SetLabel("HealthCost", DungeonPersistentData.Instance.HealthCost);
            SetLabel("Health", DungeonPersistentData.Instance.PlayerHealth);
            SetLabel("PlayerLevel", DungeonPersistentData.Instance.PlayerLevel);
        } else
        {
            audioSource.clip = MenuCancel;
            audioSource.Play();
        }
    }

    private void BuyMagic()
    {
        if (DungeonPersistentData.Instance.PlayerMagic == DungeonPersistentData.Instance.MagicMax)
        {
            audioSource.clip = MenuCancel;
            audioSource.Play();
            return;
        }

        var cost = DungeonPersistentData.Instance.MagicCost;
        if (cost <= DungeonPersistentData.Instance.PlayerExperience)
        {
            audioSource.clip = MenuActivate;
            audioSource.Play();

            SetLabel("Experience", DungeonPersistentData.Instance.PlayerExperience, true);

            DungeonPersistentData.Instance.PlayerExperience -= cost;
            DungeonPersistentData.Instance.PlayerMagic += 1;

            SetLabel("Experience", DungeonPersistentData.Instance.PlayerExperience);
            SetLabel("MagicCost", DungeonPersistentData.Instance.MagicCost);
            SetLabel("Magic", DungeonPersistentData.Instance.PlayerMagic);
            SetLabel("PlayerLevel", DungeonPersistentData.Instance.PlayerLevel);
        } else
        {
            audioSource.clip = MenuCancel;
            audioSource.Play();
        }
    }

    private void OnDisable()
    {
        Inputs.AllInputs.ForEach(x => x.Disable());
    }

    private List<Vector2> subMenuPositions = new List<Vector2>() {
        new Vector2(-8,0),
        new Vector2(16,0),
        new Vector2(40,0),
    };
    private void ActivateCurrentSelection()
    {
        if (!Active)
            return;

        if (CurrentSelection.SubMenu != null)
        {
            audioSource.clip = MenuActivate;
            audioSource.Play();

            CurrentSelection.SubMenu.Activate(subMenuPositions[subMenuIndex]);
            CurrentSelection.SubMenu.Select(DungeonPersistentData.Instance.ReminderSymbolRow.Symbols[subMenuIndex]);
            Active = false;

            return;
        }

        MenuIdAction[CurrentSelection.Id]();
    }
    private int subMenuIndex => int.Parse($"{CurrentSelection.Id.Last()}") - 1;
    public void SubMenuReturn(SubMenu subMenu, string SelectedId, bool accepted)
    {
        if (accepted)
        {
            if (!DungeonPersistentData.Instance.ReminderSymbolRow.Symbols.Any(x => x == SelectedId))
            {
                DungeonPersistentData.Instance.ReminderSymbolRow.Symbols[subMenuIndex] = SelectedId;
            } else
            {
                accepted = false;
            }
        }

        Active = true;

        audioSource.clip = accepted ? MenuActivate : MenuCancel;
        audioSource.Play();
    }

    private void Select(string target)
    {
        if (!Active || String.IsNullOrEmpty(target))
            return;

        tilemap.SetTile(Vector3Int.RoundToInt(CurrentSelection.TilemapPosition), TileEmpty);
        CurrentSelection = MenuItems.Find(x => x.Id == target);
        tilemap.SetTile(Vector3Int.RoundToInt(CurrentSelection.TilemapPosition), TileSelectionMarker);
        blinkToggleTime = Time.time + BlinkOnTime;
        isBlinkOnPeriod = false;

        audioSource.clip = MenuSelection;
        audioSource.Play();
    }

    private float blinkToggleTime;
    // Update is called once per frame
    void Update()
    {
        if (!Active)
        {
            tilemap.SetTile(Vector3Int.RoundToInt(CurrentSelection.TilemapPosition), TileEmpty);
            return;
        }

        if (blinkToggleTime < Time.time)
        {
            blinkToggleTime += isBlinkOnPeriod ? BlinkOnTime : BlinkOffTime;
            tilemap.SetTile(Vector3Int.RoundToInt(CurrentSelection.TilemapPosition), isBlinkOnPeriod ? TileSelectionMarker : TileEmpty);
            isBlinkOnPeriod = !isBlinkOnPeriod;
        }
    }
}
