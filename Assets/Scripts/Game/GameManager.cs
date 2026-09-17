using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GridData myGrid = new GridData();       // my ships
    public GridData enemyGridView = new GridData(); // what I know of enemy's grid (hits/misses only)

    public bool isMyTurn;
    private bool gameStarted = false;

    void OnEnable()
    {
        NetworkManager.Instance.OnMessageReceived += HandleMessage;
        NetworkManager.Instance.OnClientConnected += StartGame;   // host side
        NetworkManager.Instance.OnConnectedToHost += StartGame;   // client side
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    void StartGame()
    {
        myGrid.RandomizeShips();
        gameStarted = true;

        // Host decides who goes first
        if (NetworkManager.Instance.IsHost)
        {
            isMyTurn = true;
            NetworkManager.Instance.SendMessageToPeer(NetworkMessage.Turn(false)); // tell client it's OPPONENT's turn (i.e. host's)
        }
        else
        {
            isMyTurn = false; // wait for TURN message from host
        }

        Debug.Log("Game started. My turn: " + isMyTurn);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Call this when the local player clicks a cell on the ENEMY grid to fire
    public void FireAt(int x, int y)
    {
        if (!gameStarted || !isMyTurn) return;
        NetworkManager.Instance.SendMessageToPeer(NetworkMessage.Fire(x, y));
        isMyTurn = false; // wait for result + turn swap
    }

    void HandleMessage(string message)
    {
        var (type, args) = NetworkMessage.Parse(message);

        switch (type)
        {
            case "FIRE":
                var coords = args[0].Split(',');
                int fx = int.Parse(coords[0]);
                int fy = int.Parse(coords[1]);

                myGrid.ReceiveFire(fx, fy, out bool hit);
                NetworkManager.Instance.SendMessageToPeer(NetworkMessage.Result(hit, fx, fy));

                if (myGrid.AllShipsSunk())
                {
                    NetworkManager.Instance.SendMessageToPeer(NetworkMessage.GameOver(false)); // I lose, so opponent wins
                    Debug.Log("I lost!");
                }
                else
                {
                    NetworkManager.Instance.SendMessageToPeer(NetworkMessage.Turn(true)); // give opponent the turn back... 
                }
                break;

            case "RESULT":
                bool wasHit = args[0] == "HIT";
                var rcoords = args[1].Split(',');
                int rx = int.Parse(rcoords[0]);
                int ry = int.Parse(rcoords[1]);
                enemyGridView.cells[rx, ry] = wasHit ? CellState.Hit : CellState.Miss;
                Debug.Log($"Fired at {rx},{ry}: {(wasHit ? "HIT" : "MISS")}");
                break;

            case "TURN":
                isMyTurn = args[0] == "YOU";
                Debug.Log("My turn: " + isMyTurn);
                break;

            case "GAMEOVER":
                Debug.Log(args[0] == "YOU_WIN" ? "I win!" : "I lost!");
                gameStarted = false;
                break;

        }
    }
}
