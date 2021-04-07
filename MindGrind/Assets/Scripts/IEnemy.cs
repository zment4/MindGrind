using UnityEngine;

public interface IEnemy 
{
    public int MaxHitPoints { get; }
    public int CurrentHitPoints { get; }
    public int Damage { get; }
    public int Level { get; set; }
    public GameObject GameObject { get; }
}
