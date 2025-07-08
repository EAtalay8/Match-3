using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour
{
    public int x, y;
    public int tileType; // 0, 1, 2, 3 gibi prefab index'iyle eþleþen ID
    public bool isMoving = false;


    private GameManager board;

    public void Init(int _x, int _y, GameManager _board)
    {
        x = _x;
        y = _y;
        board = _board;
    }

    private void OnMouseDown()
    {
        // Debug.Log($"Clicked on tile {x},{y}");
        // Sonra: Seçme ve takas iþlemi buraya eklenecek
        board.SelectTile(this);
    }

    public void AnimateFall(Vector2 targetPos, float fallSpeed = 5f)
    {
        StartCoroutine(SmoothMove(targetPos, fallSpeed));
    }

    IEnumerator SmoothMove(Vector2 targetPos, float speed)
    {
        isMoving = true;

        while ((Vector2)transform.position != targetPos)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
    }

    public IEnumerator AnimateSwap(Vector2 targetPos, float speed = 8f)
    {
        isMoving = true;

        while ((Vector2)transform.position != targetPos)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
    }
}
