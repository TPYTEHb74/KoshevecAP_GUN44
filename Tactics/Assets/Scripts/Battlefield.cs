using UnityEngine;

public class Battlefield : MonoBehaviour
{
    [SerializeField] private Cell _cellPrefab;
    [SerializeField] private Unit _unitPrefab;

    private Cell[,] _grid = new Cell[8, 8];

    private void Start()
    {
        GenerateBoard();
        SpawnCheckers();
    }

    private void GenerateBoard()
    {
        for (int x = 0; x != 8; x++)
        {
            for (int y = 0; y != 8; y++)
            {
                Vector3 pos = new Vector3(x * 1.05f, 0, y * 1.05f);
                Cell newCell = Instantiate(_cellPrefab, pos, Quaternion.identity, transform);
                newCell.name = $"Cell_{x}_{y}";

                bool isDark = (x + y) % 2 == 0;
                newCell.Init(x, y, isDark);
                _grid[x, y] = newCell;
            }
        }
    }

    private void SpawnCheckers()
    {
        for (int x = 0; x != 8; x++)
        {
            for (int y = 0; y != 8; y++)
            {
                Cell cell = _grid[x, y];
                if (!cell.IsDark) continue;

                if (x == 0 || x == 1 || x == 2)
                {
                    CreateUnit(PlayerColor.White, cell);
                }
                else if (x == 5 || x == 6 || x == 7)
                {
                    CreateUnit(PlayerColor.Black, cell);
                }
            }
        }
    }

    private void CreateUnit(PlayerColor color, Cell cell)
    {
        Vector3 spawnPos = cell.transform.position + Vector3.up * 0.1f;
        Unit newUnit = Instantiate(_unitPrefab, spawnPos, Quaternion.identity, transform);
        newUnit.Init(color, cell);
    }

    public Cell GetCell(int x, int y)
    {
        if (x == -1 || x == 8 || y == -1 || y == 8) return null;
        return _grid[x, y];
    }
}