using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public GameObject[] tilePrefabs; // Farkl� renklerde tile prefab'lar�
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
                GameObject prefab = GetSafeTile(x, y); // ?? e�le�mesiz tile prefab'� se�
                GameObject tileObj = Instantiate(prefab, new Vector2(x, y), Quaternion.identity);
                tileObj.transform.parent = this.transform;
                tileObj.name = $"Tile {x},{y}";

                Tile tile = tileObj.GetComponent<Tile>();
                tile.Init(x, y, this);
                // tileType art�k prefab'�n i�inden geliyor, d��ar�dan atanmaz

                grid[x, y] = tile;
            }
        }
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
            // G�rsel efekt, highlight vs. ekleyebilirsin burada
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

    GameObject GetSafeTile(int x, int y)
    {
        List<GameObject> possiblePrefabs = new List<GameObject>(tilePrefabs);

        // Soldaki 2 tile ayn�ysa
        if (x >= 2)
        {
            Tile left1 = grid[x - 1, y];
            Tile left2 = grid[x - 2, y];
            if (left1 != null && left2 != null && left1.tileType == left2.tileType)
            {
                // Bu type'a sahip prefab'� listeden ��kar
                possiblePrefabs.RemoveAll(p => p.GetComponent<Tile>().tileType == left1.tileType);
            }
        }

        // Alttaki 2 tile ayn�ysa
        if (y >= 2)
        {
            Tile down1 = grid[x, y - 1];
            Tile down2 = grid[x, y - 2];
            if (down1 != null && down2 != null && down1.tileType == down2.tileType)
            {
                possiblePrefabs.RemoveAll(p => p.GetComponent<Tile>().tileType == down1.tileType);
            }
        }

        // Rastgele g�venli prefab se�
        int randomIndex = Random.Range(0, possiblePrefabs.Count);
        return possiblePrefabs[randomIndex];
    }
        
    void ClearMatches(List<Tile> matches)
    {
        foreach (Tile tile in matches)
        {
            // Grid'den kald�r
            grid[tile.x, tile.y] = null;

            // Sahneden sil
            Destroy(tile.gameObject);
        }
    }

    IEnumerator SwapTiles(Tile a, Tile b)
    {
        // Konumlar�n� animasyonlu olarak de�i�tir (�imdilik an�nda)
        Vector3 posA = a.transform.position;
        Vector3 posB = b.transform.position;

        // ?? �kisini ayn� anda �al��t�r ve birlikte bekle
        Coroutine moveA = StartCoroutine(a.AnimateSwap(posB));
        Coroutine moveB = StartCoroutine(b.AnimateSwap(posA));

        // Hepsi bitsin diye bekle
        yield return moveA;
        yield return moveB;

        // Grid dizisindeki yerlerini de�i�tir
        grid[a.x, a.y] = b;
        grid[b.x, b.y] = a;

        // x, y de�erlerini de de�i�tir
        int tempX = a.x;
        int tempY = a.y;
        a.x = b.x;
        a.y = b.y;
        b.x = tempX;
        b.y = tempY;

        yield return new WaitForSeconds(0.1f);

        // ?? ��te burada �a�r�l�yor:
        List<Tile> matchesA = GetMatchesAt(a);
        List<Tile> matchesB = GetMatchesAt(b);

        List<Tile> allMatches = matchesA.Union(matchesB).Distinct().ToList();

        Debug.Log($"Toplam e�le�en tile: {allMatches.Count}");

        if (allMatches.Count >= 3)
        {
            Debug.Log("Match found!");
            ClearMatches(allMatches);

            yield return new WaitForSeconds(0.2f); // biraz bo�luk

            CollapseBoard(); // ?? d���r

            yield return StartCoroutine(WaitForGravity());

            RefillBoard(); // ?? yeni tile�lar gel

            yield return new WaitForSeconds(0.2f);
            
            StartCoroutine(HandleMatches()); // ?? combo zincirini ba�lat
        }
        else
        {
            // ? E�le�me yoksa swap geri al�n�r

            // Swap geri al
            //a.transform.position = posA;
            //b.transform.position = posB;

            StartCoroutine(a.AnimateSwap(posA));
            yield return StartCoroutine(b.AnimateSwap(posB));


            // Grid�i eski haline getir
            grid[a.x, a.y] = b;
            grid[b.x, b.y] = a;

            // Koordinatlar� da geri al
            tempX = a.x;
            tempY = a.y;
            a.x = b.x;
            a.y = b.y;
            b.x = tempX;
            b.y = tempY;
        }
    }

    void RefillBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                {
                    GameObject prefab = GetSafeTile(x, y);
                    GameObject tileObj = Instantiate(prefab, new Vector2(x, y + 1), Quaternion.identity); // 1 birim yukar�dan gelsin
                    tileObj.transform.parent = this.transform;

                    Tile tile = tileObj.GetComponent<Tile>();
                    tile.Init(x, y, this);
                    grid[x, y] = tile;

                    tile.AnimateFall(new Vector2(x, y));
                }
            }
        }
    }

    IEnumerator WaitForGravity()
    {
        bool anyMoving;
        do
        {
            anyMoving = false;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y] != null && grid[x, y].isMoving)
                    {
                        anyMoving = true;
                        break;
                    }
                }
            }
            yield return null;
        } while (anyMoving);
    }

    List<Tile> CheckForMatches()
    {
        List<Tile> allMatches = new List<Tile>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    List<Tile> matches = GetMatchesAt(grid[x, y]);
                    if (matches.Count >= 3)
                        allMatches.AddRange(matches);
                }
            }
        }

        return allMatches.Distinct().ToList();
    }

    IEnumerator HandleMatches()
    {
        bool comboActive = true;

        while (comboActive)
        {
            yield return new WaitForSeconds(0.2f);

            List<Tile> matches = CheckForMatches();

            if (matches.Count >= 3)
            {
                ClearMatches(matches);
                yield return new WaitForSeconds(0.2f);
                CollapseBoard();
                yield return StartCoroutine(WaitForGravity());
                RefillBoard();
            }
            else
            {
                comboActive = false; // e�le�me kalmad�, ��k
            }
        }
    }

    List<Tile> GetMatchesAt(Tile currentTile)
    {
        List<Tile> horizontal = new List<Tile>();
        List<Tile> vertical = new List<Tile>();

        int type = currentTile.tileType;

        // YATAY kontrol (sol-sa�)
        horizontal.Add(currentTile);

        int x = currentTile.x - 1;
        while (x >= 0 && grid[x, currentTile.y] != null && grid[x, currentTile.y].tileType == type)
        {
            horizontal.Add(grid[x, currentTile.y]);
            x--;
        }

        x = currentTile.x + 1;
        while (x < width && grid[x, currentTile.y] != null && grid[x, currentTile.y].tileType == type)
        {
            horizontal.Add(grid[x, currentTile.y]);
            x++;
        }

        // D�KEY kontrol (yukar�-a�a��)
        vertical.Add(currentTile);

        int y = currentTile.y - 1;
        while (y >= 0 && grid[currentTile.x, y] != null && grid[currentTile.x, y].tileType == type)
        {
            vertical.Add(grid[currentTile.x, y]);
            y--;
        }

        y = currentTile.y + 1;
        while (y < height && grid[currentTile.x, y] != null && grid[currentTile.x, y].tileType == type)
        {
            vertical.Add(grid[currentTile.x, y]);
            y++;
        }

        // E�le�meleri birle�tir
        List<Tile> matches = new List<Tile>();
        if (horizontal.Count >= 3)
            matches.AddRange(horizontal);

        if (vertical.Count >= 3)
            matches.AddRange(vertical);

        return matches.Distinct().ToList();
    }

    void CollapseColumn(int x)
    {
        for (int y = 1; y < height; y++)
        {
            if (grid[x, y] != null)
            {
                int fallTo = y;
                while (fallTo > 0 && grid[x, fallTo - 1] == null)
                {
                    fallTo--;
                }

                if (fallTo != y)
                {
                    // Tile'� al
                    Tile tile = grid[x, y];

                    // Grid g�ncelle
                    grid[x, fallTo] = tile;
                    grid[x, y] = null;

                    // Tile koordinat�n� g�ncelle
                    tile.y = fallTo;

                    // ?? AN� ZIPLATMA YER�NE: animasyonlu ge�i�
                    tile.AnimateFall(new Vector2(x, fallTo), 5f); // h�z iste�e ba�l�
                }
            }
        }
    }

    void CollapseBoard()
    {
        for (int x = 0; x < width; x++)
        {
            CollapseColumn(x);
        }
    }

    void CenterCamera()
    {
        float camX = (width - 1) / 2f;
        float camY = (height - 1) / 2f;
        Camera.main.transform.position = new Vector3(camX, camY, -10f);
    }

}
