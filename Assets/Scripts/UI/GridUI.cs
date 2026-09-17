using UnityEngine;
using UnityEngine.UI;

public class GridUI : MonoBehaviour
{
    public GameObject cellPrefab;
    public bool isEnemyGrid;

    private Image[,] images = new Image[GridData.SIZE, GridData.SIZE];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int y = 0; y < GridData.SIZE; y++)
        {
            for (int x = 0; x < GridData.SIZE; x++)
            {
                GameObject cellObj = Instantiate(cellPrefab, transform);
                Button btn = cellObj.GetComponent<Button>();
                images[x, y] = cellObj.GetComponent<Image>();

                int cx = x, cy = y;
                if (isEnemyGrid)
                    btn.onClick.AddListener(() => GameManager.Instance.FireAt(cx, cy));
                else
                    btn.interactable = false;
            }
        }
    }

    public void Redraw(GridData grid)
    {
        for (int y = 0; y < GridData.SIZE; y++)
        {
            for (int x = 0; x < GridData.SIZE; x++)
            {
                CellState state = grid.cells[x, y];
                Color c = Color.cyan; // water

                if (state == CellState.Ship && !isEnemyGrid) c = Color.gray;
                else if (state == CellState.Hit) c = Color.red;
                else if (state == CellState.Miss) c = Color.white;

                images[x, y].color = c;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
