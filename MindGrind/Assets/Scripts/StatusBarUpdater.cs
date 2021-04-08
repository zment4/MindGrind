using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StatusBarUpdater : MonoBehaviour
{
    public Tile MagicBase;
    public Tile MagicActive;
    public Tile HealthBase;
    public Tile HealthActive;
    public Tile[] Digits;
    public Tile Empty;

    private Tilemap _tilemap = null;
    private Tilemap tilemap => _tilemap == null ? (_tilemap = GetComponent<Tilemap>()) : _tilemap;

    private int statusBarR1 = 13;
    private int statusBarR2 = 12;
    private int experienceDigitStart = 14;

    private int maxHealth;

    public int MaxHealth {
        set {
            maxHealth = value;
            FillHealthSlots(value, HealthBase);
        }
    }

    public int CurrentHealth
    {
        set
        {
            MaxHealth = maxHealth;
            FillHealthSlots(value, HealthActive);
        }
    }
    private int maxMagic;
    public int MaxMagic { 
        set 
        {
            maxMagic = value;
            FillMagicSlots(value, MagicBase);
        } 
    }
    public int CurrentMagic
    {
        set
        {
            MaxMagic = maxMagic;
            FillMagicSlots(value, MagicActive);
        }
    }
    public int Experience {
        set
        {
            var exp = value;

            var curX = experienceDigitStart;

            do
            {
                var digit = exp % 10;
                tilemap.SetTile(new Vector3Int(curX, statusBarR2, 0), Digits[digit]);
                exp -= digit;
                exp /= 10;
                curX -= 1;
            } while (exp > 0);
        }
    }

    private void FillHealthSlots(int number, Tile tile)
    {
        var curX = -16;
        var curY = statusBarR1;

        for (int i = 0; i < number; i++, curX++)
        {
            tilemap.SetTile(new Vector3Int(curX, curY, 0), tile);
            if (curX == -7)
            {
                curX = -17;
                curY -= 1;
            }
        }
    }

    private void FillMagicSlots(int number, Tile tile)
    {
        var curX = 15;

        for (int i = 0; i < number; i++, curX--)
        {
            tilemap.SetTile(new Vector3Int(curX, statusBarR1, 0), tile);
        }
    }
}
