using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    public bool IsHost { get; private set; }
    public bool IsConnected { get; private set; }

    public event Action<string> OnMessageReceived;
    public event Action OnClientConnected;   // host: someone joined
    public event Action OnConnectedToHost;   // client: successfully connected
    public event Action OnDisconnected;
    public event Action<string> OnConnectionFailed;

    private TcpListener listener;
    private TcpClient client;
    private NetworkStream stream;
    private Thread listenThread;
    private Thread receiveThread;

    private readonly ConcurrentQueue<string> incomingMessages = new ConcurrentQueue<string>();
    private readonly ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

    private const int PORT = 7777;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Drain messages/actions queued from background threads
        while (incomingMessages.TryDequeue(out string msg))
        {
            OnMessageReceived?.Invoke(msg);
        }
        while (mainThreadActions.TryDequeue(out Action action))
        {
            action.Invoke();
        }
    }


    // connect to host
    public void StartHost()
    {
        IsHost = true;
        listener = new TcpListener(IPAddress.Any, PORT);
        listener.Start();

        listenThread = new Thread(AcceptClientLoop) { IsBackground = true };
        listenThread.Start();

        Debug.Log($"Hosting on port {PORT}. Waiting for a player...");
    }

    private void AcceptClientLoop()
    {
        try
        {
            client = listener.AcceptTcpClient(); // blocks until someone connects
            stream = client.GetStream();
            IsConnected = true;

            mainThreadActions.Enqueue(() => OnClientConnected?.Invoke());

            receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
            receiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogWarning("Host accept failed: " + e.Message);
        }
    }

    // client
    public void StartClient(string ip)
    {
        IsHost = false;
        Thread connectThread = new Thread(() => ConnectToHost(ip)) { IsBackground = true };
        connectThread.Start();
    }
    private void ConnectToHost(string ip)
    {
        try
        {
            client = new TcpClient();
            client.Connect(ip, PORT); // runs on background thread
            stream = client.GetStream();
            IsConnected = true;

            mainThreadActions.Enqueue(() => OnConnectedToHost?.Invoke());

            receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
            receiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogWarning("Client connect failed: " + e.Message);
            mainThreadActions.Enqueue(() => OnConnectionFailed?.Invoke(e.Message));
        }
    }

    // shared betweeb both
    private void ReceiveLoop()
    {
        byte[] buffer = new byte[4096];
        StringBuilder sb = new StringBuilder();

        try
        {
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break; // disconnected

                sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                // Messages are newline-delimited
                string data = sb.ToString();
                int newlineIndex;
                while ((newlineIndex = data.IndexOf('\n')) >= 0)
                {
                    string message = data.Substring(0, newlineIndex).Trim();
                    if (message.Length > 0)
                        incomingMessages.Enqueue(message);

                    data = data.Substring(newlineIndex + 1);
                }
                sb.Clear();
                sb.Append(data);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Receive loop ended: " + e.Message);
        }

        IsConnected = false;
        mainThreadActions.Enqueue(() => OnDisconnected?.Invoke());
    }

    public void SendMessageToPeer(string message)
    {
        if (!IsConnected || stream == null) return;
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message + "\n");
            stream.Write(data, 0, data.Length);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Send failed: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        Shutdown();
    }

    public void Shutdown()
    {
        try { receiveThread?.Abort(); } catch { }
        try { stream?.Close(); } catch { }
        try { client?.Close(); } catch { }
        try { listener?.Stop(); } catch { }
        IsConnected = false;
    }

    public string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        var ip = host.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
        return ip != null ? ip.ToString() : "Unable to find IP!";
    }
}
