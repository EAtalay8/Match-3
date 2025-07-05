using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public GameObject[] tilePrefabs; // Farklý renklerde tile prefab'larý
    public Tile[,] grid;

    private Tile selectedTile = null;

    void Start()
    {
        grid = new Tile[width, height];
        SpawnBoard();
        CenterCamera();
    }

    void SpawnBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = new Vector2(x, y);
                GameObject tileObj = Instantiate(GetRandomTile(), pos, Quaternion.identity);
                tileObj.transform.parent = this.transform;
                tileObj.name = $"Tile {x},{y}";

                Tile tile = tileObj.GetComponent<Tile>();
                tile.Init(x, y, this);
                grid[x, y] = tile;
            }
        }
    }

    GameObject GetRandomTile()
    {
        int index = Random.Range(0, tilePrefabs.Length);
        return tilePrefabs[index];
    }

    bool AreAdjacent(Tile a, Tile b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }

    public void SelectTile(Tile tile)
    {
        if (selectedTile == null)
        {
            selectedTile = tile;
            // Görsel efekt, highlight vs. ekleyebilirsin burada
        }
        else
        {
            if (AreAdjacent(selectedTile, tile))
            {
                StartCoroutine(SwapTiles(selectedTile, tile));
            }
            selectedTile = null;
        }
    }

    IEnumerator SwapTiles(Tile a, Tile b)
    {
        // Konumlarýný animasyonlu olarak deðiþtir (þimdilik anýnda)
        Vector3 posA = a.transform.position;
        Vector3 posB = b.transform.position;

        a.transform.position = posB;
        b.transform.position = posA;

        // Grid dizisindeki yerlerini deðiþtir
        grid[a.x, a.y] = b;
        grid[b.x, b.y] = a;

        // x, y deðerlerini de deðiþtir
        int tempX = a.x;
        int tempY = a.y;
        a.x = b.x;
        a.y = b.y;
        b.x = tempX;
        b.y = tempY;

        // Buraya match kontrolü eklenecek
        yield return null;
    }

    void CenterCamera()
    {
        float camX = (width - 1) / 2f;
        float camY = (height - 1) / 2f;
        Camera.main.transform.position = new Vector3(camX, camY, -10f);
    }

}
