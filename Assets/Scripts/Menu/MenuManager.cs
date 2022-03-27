using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    public bool paused = false;

    private Lighting lighting;

    public GameObject playerPrefab;

    public MenuCamera menuCamera;
    public GameObject loadingCamera;
    GameObject playerCamera;

    [Header("UI")]
    public GameObject backButton;

    public GameObject menuUi;
    public GameObject loadingUi;

    public GameObject menuButtonsUi; // (main menu)
    public GameObject openSeaUi;
    public GameObject sandboxUi;

    public InputField inputFieldSeed;

    public Text textGamemode;
    public Text textDescription;

    private void Awake()
    {
        instance = this;

        lighting = FindObjectOfType<Lighting>();
        lighting.fog = true;
        lighting.UpdateShader();

        menuUi.SetActive(true);
        menuButtonsUi.SetActive(true);

        backButton.SetActive(false);

        openSeaUi.SetActive(false);
        sandboxUi.SetActive(false);

        loadingUi.SetActive(false);

        textGamemode.gameObject.SetActive(true);
        textDescription.gameObject.SetActive(true);

        textGamemode.text = "Choose a gamemode";
        textDescription.text = "";

        MouseMode.Pause();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TogglePause();
        }
    }

    public void FinishedLoading()
    {
        lighting.fog = true;
        lighting.UpdateShader();

        loadingCamera.SetActive(false);
        loadingUi.SetActive(false);

        Instantiate(playerPrefab, null);
    }

    public void TogglePause()
    {
        if (!FindObjectOfType<PlayerManager>()) return;

        paused = !paused;

        if (paused)
        {
            menuUi.SetActive(true);
            menuButtonsUi.SetActive(true);

            backButton.SetActive(false);

            openSeaUi.SetActive(false);
            sandboxUi.SetActive(false);

            playerCamera = FindObjectOfType<PlayerCamera>().gameObject;

            playerCamera.SetActive(false);
            menuCamera.gameObject.SetActive(true);
            menuCamera.GetComponent<MenuCamera>().SetupCameraPosition(World.instance);

            textGamemode.gameObject.SetActive(true);
            textDescription.gameObject.SetActive(true);

            textGamemode.text = "Choose a gamemode";
            textDescription.text = "Press Q to resume";

            MouseMode.Pause();
        }
        else
        {
            menuUi.SetActive(false);
            menuButtonsUi.SetActive(false);

            backButton.SetActive(false);

            openSeaUi.SetActive(false);
            sandboxUi.SetActive(false);

            if (playerCamera != null)
                playerCamera.SetActive(true);
            menuCamera.gameObject.SetActive(false);

            MouseMode.Play();
        }
    }

    public void Button_Play()
    {
        menuCamera.gameObject.SetActive(false);

        lighting.fog = false;
        lighting.UpdateShader();

        loadingCamera.SetActive(true);
        loadingUi.SetActive(true);

        if (inputFieldSeed.text.Length == 0)
        {
            Button_RandomiseSeed();
        }

        PlayerManager player = FindObjectOfType<PlayerManager>();

        if (player) Destroy(player.gameObject);
        if (playerCamera) Destroy(playerCamera.gameObject);

        LoadingStatus.SetStatus($"Calculating terrain data (seed: {World.instance.seed})...");

        World.instance.GenerateChunks();

        menuUi.SetActive(false);
        paused = false;
    }

    public void InputField_SetSeed()
    {
        if (int.TryParse(inputFieldSeed.text, out int seed))
        {
            World.instance.seed = seed;

            if (seed > 9999999)
            {
                World.instance.seed = 9999999;
                inputFieldSeed.text = "9999999";
            }
            else if (seed < -9999999)
            {
                World.instance.seed = -9999999;
                inputFieldSeed.text = "-9999999";
            }
        }
        else if (inputFieldSeed.text.Length > 0)
        {
            if (inputFieldSeed.text.Length == 1)
            {
                if (inputFieldSeed.text == "-")
                {
                    World.instance.seed = 0;
                }
                else
                {
                    string s = inputFieldSeed.text;
                    s = s.Remove(s.Length - 1);
                    inputFieldSeed.text = s;
                }
            }
            else
            {
                string s = inputFieldSeed.text;
                s = s.Remove(s.Length - 1);
                inputFieldSeed.text = s;
            }
        }
    }

    public void Button_Back()
    {
        menuButtonsUi.SetActive(true);
        textGamemode.gameObject.SetActive(true);
        textDescription.gameObject.SetActive(true);

        openSeaUi.SetActive(false);
        sandboxUi.SetActive(false);
        backButton.SetActive(false);

        textGamemode.text = "Choose a gamemode";
        textDescription.text = "";
    }

    public void Button_OpenSea()
    {
        menuButtonsUi.SetActive(false);
        openSeaUi.SetActive(true);
        backButton.SetActive(true);
        textGamemode.gameObject.SetActive(false);
        textDescription.gameObject.SetActive(false);
    }

    public void Button_Sandbox()
    {
        menuButtonsUi.SetActive(false);
        sandboxUi.SetActive(true);
        backButton.SetActive(true);
        textGamemode.gameObject.SetActive(false);
        textDescription.gameObject.SetActive(false);
    }

    public void Hover_OpenSea()
    {
        textGamemode.text = "OpenSea";
        textDescription.text = "Fetch an NFT terrain from the blockchain";
    }

    public void Hover_Sandbox()
    {
        textGamemode.text = "Sandbox";
        textDescription.text = "Generate a new terrain using a seed";
    }

    public void HoverExit()
    {
        textGamemode.text = "";
        textDescription.text = "";
    }

    public void Button_RandomiseSeed()
    {
        int randomSeed = Random.Range(-9999999, 9999999);
        World.instance.seed = randomSeed;
        inputFieldSeed.text = randomSeed.ToString();
    }
}
