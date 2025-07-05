using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x, y;
    private GameManager board;

    public int tileType; // 0, 1, 2, 3 gibi prefab index'iyle e�le�en ID

    public void Init(int _x, int _y, GameManager _board)
    {
        x = _x;
        y = _y;
        board = _board;
    }

    private void OnMouseDown()
    {
        Debug.Log($"Clicked on tile {x},{y}");
        // Sonra: Se�me ve takas i�lemi buraya eklenecek
        board.SelectTile(this);
    }
}
