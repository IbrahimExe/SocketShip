using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject waitingPanel;

    public GridUI myGridUI;
    public GridUI enemyGridUI;

    public GridData myGrid = new GridData();       // my ships
    public GridData enemyGridView = new GridData(); // what I know of enemy's grid (hits/misses only)

    public bool isMyTurn;
    public TMPro.TMP_Text turnStatusText;
    private bool gameStarted = false;

    void Awake()
    {
        Instance = this;

        // set initial panel state here, not in Start(), so it can never
        // run AFTER OnEnable() has already turned it off via StartGame()
        if (waitingPanel != null)
        {
            waitingPanel.SetActive(true);
        }
    }

    void OnEnable()
    {
        NetworkManager.Instance.OnMessageReceived += HandleMessage;
        NetworkManager.Instance.OnClientConnected += StartGame;   // host side
        NetworkManager.Instance.OnConnectedToHost += StartGame;   // client side

        // to fix race condition 
        if (NetworkManager.Instance.IsConnected && !gameStarted)
        {
            StartCoroutine(DelayedStartGame());
        }
    }

    System.Collections.IEnumerator DelayedStartGame()
    {
        yield return null; // wait a frame for all Awake/ Start calls

        if (!gameStarted)
        {
            StartGame();
        }
    }

    void SetTurn(bool myTurn)
    {
        isMyTurn = myTurn;
        enemyGridUI.SetInteractable(myTurn);
        if (turnStatusText != null)
            turnStatusText.text = myTurn ? "Your Turn: Fire!" : "Opponent's Turn: Wait...";
    }

    void StartGame()
    {
        if (waitingPanel != null)
        {
            waitingPanel.SetActive(false);
        }

        myGrid.RandomizeShips();
        gameStarted = true;

        myGridUI.Redraw(myGrid);
        enemyGridUI.Redraw(enemyGridView);

        // Host decides who goes first
        if (NetworkManager.Instance.IsHost)
        {
            SetTurn(true);
            NetworkManager.Instance.SendMessageToPeer(NetworkMessage.Turn(false)); // tell client it's OPPONENT's turn (i.e. host's)
        }
        else
        {
            SetTurn(false); // wait for TURN message from host
        }

        Debug.Log("Game started. My turn: " + isMyTurn);
    }

    // Update is called once per frame
//    void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.Escape))
//        {
//            Application.Quit();
//#if UNITY_EDITOR
//            UnityEditor.EditorApplication.isPlaying = false;
//#endif
//        }
//    }

    // Call this when the local player clicks a cell on the ENEMY grid to fire
    public void FireAt(int x, int y)
    {
        if (!gameStarted || !isMyTurn) return;
        if (enemyGridView.cells[x, y] != CellState.Empty) return; // already fired here
        NetworkManager.Instance.SendMessageToPeer(NetworkMessage.Fire(x, y));
        SetTurn(false); // wait for result + turn swap
    }

    void HandleMessage(string message)
    {
        var (type, args) = NetworkMessage.Parse(message);

        switch (type)
        {
            case "FIRE":
                var c = args[0].Split(',');
                int fx = int.Parse(c[0]), fy = int.Parse(c[1]);
                myGrid.ReceiveFire(fx, fy, out bool hit);
                myGridUI.Redraw(myGrid);
                NetworkManager.Instance.SendMessageToPeer(NetworkMessage.Result(hit, fx, fy));

                if (myGrid.AllShipsSunk())
                {
                    NetworkManager.Instance.SendMessageToPeer(NetworkMessage.GameOver(false)); // attacker wins
                    gameStarted = false;
                    enemyGridUI.SetInteractable(false);
                }
                else
                {
                    NetworkManager.Instance.SendMessageToPeer(NetworkMessage.Turn(false)); // tell attacker it's now opponent's turn
                    SetTurn(true); // it's now my turn to fire back
                }
                break;

            case "RESULT":
                bool wasHit = args[0] == "HIT";
                var rc = args[1].Split(',');
                int rx = int.Parse(rc[0]), ry = int.Parse(rc[1]);
                enemyGridView.cells[rx, ry] = wasHit ? CellState.Hit : CellState.Miss;
                enemyGridUI.Redraw(enemyGridView);
                break;

            case "TURN":
                SetTurn(args[0] == "YOU");
                break;

            case "GAMEOVER":
                gameStarted = false;
                enemyGridUI.SetInteractable(false); // lock the board, game's over
                Debug.Log(args[0] == "YOU_WIN" ? "I win!" : "I lost!");
                break;

        }
    }
}