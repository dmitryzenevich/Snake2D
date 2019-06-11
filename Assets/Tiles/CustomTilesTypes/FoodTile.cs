using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/Food")]
public class FoodTile : Tile
{
    [SerializeField]
    private int _foodValue;

    public int FoodValue
    {
        get { return _foodValue; }
        private set { _foodValue = value; }
    }
}
