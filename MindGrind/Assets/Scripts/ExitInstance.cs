using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ExitInstance : MonoBehaviour
{
    private ExitHandler ExitHandler;

    private void Start()
    {
        ExitHandler = transform.parent.GetComponent<ExitHandler>();
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.name == "Player")
            ExitHandler?.TriggerExit(int.Parse($"{name.Last()}"));
    }
}
