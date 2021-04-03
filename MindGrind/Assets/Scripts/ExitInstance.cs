using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ExitInstance : MonoBehaviour
{
    [System.Serializable]
    public enum OrientationEnum
    {
        Horizontal,
        Vertical
    }

    public OrientationEnum Orientation;

    private ExitHandler ExitHandler;

    private void Start()
    {
        ExitHandler = transform.parent.GetComponent<ExitHandler>();
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.name == "Player")
        {
            var deltaX = Orientation == OrientationEnum.Horizontal ?
                collider.transform.position.x - transform.position.x : 0f;
            var deltaY = Orientation == OrientationEnum.Vertical ?
                collider.transform.position.y - transform.position.y : 0f;
            var deltaVector = new Vector3(deltaX, deltaY, 0);

            ExitHandler?.TriggerExit(int.Parse($"{name.Last()}"), deltaVector);
        }
    }
}
