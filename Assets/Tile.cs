using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x, y;
    public int tileType; // 0, 1, 2, 3 gibi prefab index'iyle eþleþen ID

    private GameManager board;

    public void Init(int _x, int _y, GameManager _board)
    {
        x = _x;
        y = _y;
        board = _board;
    }

    private void OnMouseDown()
    {
        //Debug.Log($"Clicked on tile {x},{y}");
        // Sonra: Seçme ve takas iþlemi buraya eklenecek
        board.SelectTile(this);
    }
}
