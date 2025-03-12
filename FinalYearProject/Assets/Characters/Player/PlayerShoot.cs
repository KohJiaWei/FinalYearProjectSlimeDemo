using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using JetBrains.Annotations;
using System.Runtime.CompilerServices;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Timers;

public class PlayerShoot : MonoBehaviour
{
    [Header("References")]
    public Camera fpsCam;
    public GameObject projectilePrefab;
    public Animator anim;
    public LightningSpell LightningSpell;

    [Header("Shooting Settings")]
    //public float shootForce = 1000f;

    private Thread serverThread;
    private bool running = true;

    // The port to listen on
    private int port = 5005;

    // Lightning charge tracking
    public int lightningCharges = 0;
    public const int maxCharges = 3;

    public TMP_Text lightningChargesText;
    public ConcentrationBar concentrationBar;
    
    public float ShootCooldown = 0.3f;

    private float ShootCooldownTimer;


    void Start()
    {
        // Start the TCP listener thread
        serverThread = new Thread(TCPServerLoop);
        serverThread.IsBackground = true;
        serverThread.Start();

        UpdateLightningUI();
    }

    void Update()
    {
        // On left click, decide which attack to perform
        
        ShootCooldownTimer += Time.deltaTime;
        if (Input.GetMouseButtonDown(0))
        {
            if (ShootCooldownTimer > ShootCooldown)
            {
                ShootCooldownTimer = 0f;
                if (lightningCharges > 0)
                {
                    UseCharge();
                    LightningSpell.Shoot();

                }
                else
                {
                    Shoot();
                }
            }

        }
    }

    void Shoot()
    {
        if (projectilePrefab == null) return;
        anim.Play("Attack01", 0, 0);
        GameObject projectile = Instantiate(projectilePrefab, fpsCam.transform.position, fpsCam.transform.rotation);
        //if (projectile.TryGetComponent(out Rigidbody rb))
        //{
        //    rb.AddForce(fpsCam.transform.forward * shootForce);
        //}
    }

    /// <summary>
    /// Continuously listens for incoming connections on the specified port.
    /// When a client connects, it spins off a new thread to handle messages from that client.
    /// </summary>
    /// 

    void TCPServerLoop()
    {
        TcpListener listener = null;

        try

        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Debug.Log("TCP Server started. Listening on port " + port);

            while (running)
            {
                // Check if there is an incoming connection; if not, sleep briefly.
                if (!listener.Pending())
                {
                    Thread.Sleep(100);
                    continue;
                }

                // Accept the incoming client
                TcpClient client = listener.AcceptTcpClient();
                Debug.Log("Client connected.");

                // Handle this client in a separate thread
                Thread clientThread = new Thread(() => ClientHandler(client));
                clientThread.IsBackground = true;
                clientThread.Start();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("TCPServerLoop Error: " + e.Message);
        }
        finally
        {
            if (listener != null)
            {
                listener.Stop();
            }
        }
    }

    /// <summary>
    /// Reads data from a connected client until the connection closes.
    /// Adjust the useLightningSpell flag based on incoming messages.
    /// </summary>
    void ClientHandler(TcpClient client)
    {
        try
        {
            using (NetworkStream stream = client.GetStream())
            {
                byte[] buffer = new byte[1024];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                    Debug.Log("Message from client: " + message);

                    if (message == "AddLightningCharge")
                    {
                        // Enqueue UI and charge updates on the main thread
                        MainThreadDispatcher.Actions.Enqueue(() =>
                        {
                            if (lightningCharges < maxCharges)
                            {
                                GainCharge();
                            }
                            else
                            {
                                Debug.Log("Lightning charge already at max!");
                            }
                        });
                    }
                    else if (float.TryParse(message, out float betaAlphaRatio))
                    {
                        Debug.Log($"Parsed concentration value: {betaAlphaRatio}");
                        MainThreadDispatcher.Actions.Enqueue(() =>
                        {
                            if (concentrationBar != null)
                            {
                                concentrationBar.UpdateConcentration(betaAlphaRatio);
                            }
                            else
                            {
                                Debug.LogError("ConcentrationBar is not assigned in PlayerShoot!");
                            }
                        });
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("ClientHandler Error: " + e.Message);
        }
        finally
        {
            client.Close();
            Debug.Log("Client disconnected.");
        }
    }



    void OnApplicationQuit()
    {
        running = false;
        if (serverThread != null && serverThread.IsAlive)
        {
            serverThread.Abort();
        }
    }
    private void UpdateLightningUI()
    {
        if (lightningChargesText != null)
        {
            lightningChargesText.text = $"Lightning Charges: {lightningCharges}";
        }
    }

    public void UseCharge()
    {
        lightningCharges--;
        lightningChargesText.text = $"Lightning Charges: {lightningCharges}";
    }

    public void GainCharge()
    {
        lightningCharges++;
        lightningChargesText.text = $"Lightning Charges: {lightningCharges}";
        MainThreadDispatcher.Actions.Enqueue(() =>
        {
            UpdateLightningUI();
        });
    }

}


