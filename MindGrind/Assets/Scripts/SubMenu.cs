using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class SubMenu : MonoBehaviour
{
    private Tilemap _tilemap;
    private Tilemap tilemap => _tilemap == null ? (_tilemap = GetComponent<Tilemap>()) : _tilemap;
    private DungeonStartMenu _parent = null;
    public DungeonStartMenu parent => _parent == null ? (_parent = transform.parent.GetComponent<DungeonStartMenu>()) : _parent;

    private MenuItem CurrentSelection;
    public List<MenuItem> MenuItems = new List<MenuItem>();

    public void SubscribeEvents()
    {
        parent.Inputs.Up.performed += UpAction;
        parent.Inputs.Down.performed += DownAction;
        parent.Inputs.Left.performed += LeftAction;
        parent.Inputs.Right.performed += RightAction;
        parent.Inputs.Activate.performed += ActivateAction;
        parent.Inputs.Cancel.performed += CancelAction;
    }

    private void UpAction(InputAction.CallbackContext context) => Select(CurrentSelection.TargetUp);
    private void DownAction(InputAction.CallbackContext context) => Select(CurrentSelection.TargetDown);
    private void LeftAction(InputAction.CallbackContext context) => Select(CurrentSelection.TargetLeft);
    private void RightAction(InputAction.CallbackContext context) => Select(CurrentSelection.TargetRight);
    private void ActivateAction(InputAction.CallbackContext context) => ReturnToParent(true);
    private void CancelAction(InputAction.CallbackContext context) => ReturnToParent(false);

    private void ReturnToParent(bool accepted)
    {
        gameObject.SetActive(false);
        UnsubscribeEvents();
        tilemap.SetTile(Vector3Int.RoundToInt(CurrentSelection.TilemapPosition), parent.TileEmpty);
        parent.SubMenuReturn(this, CurrentSelection.Id, accepted);
    }

    private void UnsubscribeEvents()
    {
        parent.Inputs.Up.performed -= UpAction;
        parent.Inputs.Down.performed -= DownAction;
        parent.Inputs.Left.performed -= LeftAction;
        parent.Inputs.Right.performed -= RightAction;
        parent.Inputs.Activate.performed -= ActivateAction;
        parent.Inputs.Cancel.performed -= CancelAction;
    }

    internal void Activate(Vector2 position)
    {
        gameObject.SetActive(true);
        transform.position = position;
        CurrentSelection = MenuItems.First();
        StartCoroutine(SubscribeEventsNextFrame());
    }

    IEnumerator SubscribeEventsNextFrame()
    {
        yield return null;

        SubscribeEvents();
    }

    public void Select(string target)
    {
        if (String.IsNullOrEmpty(target))
            return;

        tilemap.SetTile(Vector3Int.RoundToInt(CurrentSelection.TilemapPosition), parent.TileEmpty);
        CurrentSelection = MenuItems.Find(x => x.Id == target);
        tilemap.SetTile(Vector3Int.RoundToInt(CurrentSelection.TilemapPosition), parent.TileSelectionMarker);
        blinkToggleTime = Time.time + parent.BlinkOnTime;
        isBlinkOnPeriod = false;
    }

    private bool isBlinkOnPeriod;
    private float blinkToggleTime;
    // Update is called once per frame
    void Update()
    {
        if (blinkToggleTime < Time.time)
        {
            blinkToggleTime += isBlinkOnPeriod ? parent.BlinkOnTime : parent.BlinkOffTime;
            tilemap.SetTile(Vector3Int.RoundToInt(CurrentSelection.TilemapPosition), isBlinkOnPeriod ? parent.TileSelectionMarker : parent.TileEmpty);
            isBlinkOnPeriod = !isBlinkOnPeriod;
        }
    }
}

[System.Serializable]
public class MenuItem
{
    public string Id;
    public Vector2 TilemapPosition;
    public string TargetUp;
    public string TargetDown;
    public string TargetLeft;
    public string TargetRight;

    public SubMenu SubMenu;
}
