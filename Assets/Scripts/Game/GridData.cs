using UnityEngine;

public enum  CellState
{
    Empty,
    Ship,
    Hit,
    Miss
}

public class GridData
{
    public const int SIZE = 10;
    public CellState[,] cells = new CellState[SIZE, SIZE];

    public GridData()
    {
        for (int x = 0; x < SIZE; x++)
            for (int y = 0; y < SIZE; y++)
                cells[x, y] = CellState.Empty;
    }

    public bool PlaceShip(int x, int y)
    {
        if (x < 0 || x >= SIZE || y < 0 || y >= SIZE) return false;
        cells[x, y] = CellState.Ship;
        return true;
    }

    public bool ReceiveFire(int x, int y, out bool hit)
    {
        hit = cells[x, y] == CellState.Ship;
        cells[x, y] = hit ? CellState.Hit : CellState.Miss;
        return true;
    }

    public bool AllShipsSunk()
    {
        foreach (var c in cells)
            if (c == CellState.Ship) return false;
        return true;
    }

    // Randomly place ships on grid ill change this later when i 
    // have time lol (Thanks for the recommendation Darren!)
    public void RandomizeShips()
    {
        int[] shipSizes = { 5, 4, 3, 3, 2 };

        System.Random rng = new System.Random();

        foreach (int size in shipSizes)
        {
            bool placed = false;
            while (!placed)
            {
                bool horizontal = rng.Next(2) == 0;
                int x = rng.Next(0, SIZE);
                int y = rng.Next(0, SIZE);

                if (CanPlace(x, y, size, horizontal))
                {
                    for (int i = 0; i < size; i++)
                    {
                        int px = horizontal ? x + i : x;
                        int py = horizontal ? y : y + i;
                        cells[px, py] = CellState.Ship;
                    }
                    placed = true;
                }
            }
        }
    }

    private bool CanPlace(int x, int y, int size, bool horizontal)
    {
        for (int i = 0; i < size; i++)
        {
            int px = horizontal ? x + i : x;
            int py = horizontal ? y : y + i;

            if (px < 0 || px >= SIZE || py < 0 || py >= SIZE) return false;
            if (cells[px, py] != CellState.Empty) return false;
        }
        return true;
    }
}
