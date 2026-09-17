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
    public void RandomizeShips(int shipCount = 5)
    {
        System.Random rng = new System.Random();
        int placed = 0;
        while (placed < shipCount)
        {
            int x = rng.Next(0, SIZE);
            int y = rng.Next(0, SIZE);
            if (cells[x, y] == CellState.Empty)
            {
                cells[x, y] = CellState.Ship;
                placed++;
            }
        }
    }
}
