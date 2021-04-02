using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class SegmentController : MonoBehaviour
{
    public List<GameObject> PossibleVariations => AllVariations.Where(x => x != null).ToList();
    public GameObject ExitUp;
    public GameObject ExitLeft;
    public GameObject ExitUpLeft;
    public GameObject ExitUpRight;
    public GameObject ExitDown;
    public GameObject ExitRight;
    public GameObject ExitDownRight;
    public GameObject ExitDownLeft;
    public GameObject NoExits;
    public GameObject Altar;
    private List<GameObject> AllVariations => new List<GameObject> {
        ExitUp, ExitLeft, ExitUpLeft, ExitUpRight, ExitDown, ExitRight, ExitDownRight, ExitDownLeft, NoExits, Altar
    };
    public int VariationIndex => AllVariations.FindIndex(x => x && x.activeSelf);

    public void Select(int variationIndex)
    {
        Select(AllVariations[variationIndex]);
    }

    public void Select(GameObject go)
    {
        DisableAllVariations();
        go?.SetActive(true);
    }

    private void DisableAllVariations() => AllVariations.ToList().ForEach(x => { if (x) x.SetActive(false); });

    public void SelectExitUp() => Select(ExitUp);
    public void SelectExitDown() => Select(ExitDown);
    public void SelectExitDownLeft() => Select(ExitDownLeft);
    public void SelectExitUpLeft() => Select(ExitUpLeft);
    public void SelectExitLeft() => Select(ExitLeft);
    public void SelectExitRight() => Select(ExitRight);
    public void SelectExitDownRight() => Select(ExitDownRight);
    public void SelectExitUpRight() => Select(ExitUpRight);
    public void SelectNoExits() => Select(NoExits);
    public void SelectAltar() => Select(Altar);

}
