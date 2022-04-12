using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
    public GameObject settingsButton;

    public GameObject menuUi;
    public GameObject loadingUi;

    public GameObject menuButtonsUi; // (main menu)
    public GameObject openSeaUi;
    public GameObject sandboxUi;
    public GameObject settingsUi;

    public InputField inputFieldSeed;

    public Text textGamemode;
    public Text textDescription;

    public Dropdown dropdownMapSize;

    public Text textMessage;

    private void Awake()
    {
        instance = this;

        lighting = FindObjectOfType<Lighting>();
        lighting.fog = true;
        lighting.UpdateShader();

        menuUi.SetActive(true);
        menuButtonsUi.SetActive(true);

        backButton.SetActive(false);
        settingsButton.SetActive(true);

        openSeaUi.SetActive(false);
        sandboxUi.SetActive(false);
        nftPlayUi.SetActive(false);
        settingsUi.SetActive(false);

        textMessage.gameObject.SetActive(true);

        loadingUi.SetActive(false);

        textGamemode.gameObject.SetActive(true);
        textDescription.gameObject.SetActive(true);

        SetMessageText("", 0, new Color(1, 1, 1, .8f));

        textGamemode.text = "Choose a gamemode";
        textDescription.text = "";

        MouseMode.Pause();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    #region UI

    private void SetMessageText(string message, float duration, Color color)
    {
        textMessage.color = color;
        textMessage.text = message;
    }

    public void TogglePause()
    {
        if (!FindObjectOfType<PlayerManager>()) return;

        paused = !paused;

        AudioManager.instance.playMusic = paused;

        if (paused)
        {
            menuUi.SetActive(true);
            menuButtonsUi.SetActive(true);

            backButton.SetActive(false);
            settingsButton.SetActive(true);

            openSeaUi.SetActive(false);
            sandboxUi.SetActive(false);
            nftPlayUi.SetActive(false);

            playerCamera = FindObjectOfType<PlayerCamera>().gameObject;

            playerCamera.SetActive(false);
            menuCamera.gameObject.SetActive(true);
            menuCamera.GetComponent<MenuCamera>().SetupCameraPosition(World.instance);

            textGamemode.gameObject.SetActive(true);
            textDescription.gameObject.SetActive(true);

            textGamemode.text = "Choose a gamemode";
            textDescription.text = "Press Escape to resume";

            SetMessageText("", 10, new Color(1, 1, 1, .8f));

            MouseMode.Pause();
        }
        else
        {
            menuUi.SetActive(false);
            menuButtonsUi.SetActive(false);

            backButton.SetActive(false);

            openSeaUi.SetActive(false);
            sandboxUi.SetActive(false);
            settingsUi.SetActive(false);

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


        if (inputFieldSeed.text.Length == 0 && sandboxUi.activeInHierarchy)
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
        settingsButton.SetActive(true);

        openSeaUi.SetActive(false);
        sandboxUi.SetActive(false);
        nftPlayUi.SetActive(false);
        settingsUi.SetActive(false);
        backButton.SetActive(false);

        textGamemode.text = "Choose a gamemode";
        textDescription.text = "";

        SetMessageText("", 10, new Color(1, 1, 1, .8f));
    }

    public void Button_RandomiseSeed()
    {
        int randomSeed = Random.Range(-9999999, 9999999);
        World.instance.seed = randomSeed;
        inputFieldSeed.text = randomSeed.ToString();
    }

    public void Button_DropdownMapSize()
    {
        if (dropdownMapSize.options.Count == 4)
        {
            dropdownMapSize.options.RemoveAt(3);
            dropdownMapSize.value = 1;
        }
    }

    #region Main Menu

    public void Button_OpenSea()
    {
        menuButtonsUi.SetActive(false);
        settingsButton.SetActive(false);
        openSeaUi.SetActive(true);
        backButton.SetActive(true);
        textGamemode.gameObject.SetActive(false);
        textDescription.gameObject.SetActive(false);
    }

    public void Button_Sandbox()
    {
        menuButtonsUi.SetActive(false);
        settingsButton.SetActive(false);
        sandboxUi.SetActive(true);
        backButton.SetActive(true);
        textGamemode.gameObject.SetActive(false);
        textDescription.gameObject.SetActive(false);
        dropdownMapSize.interactable = true;
    }

    public void Button_Settings()
    {
        backButton.SetActive(true);

        settingsButton.SetActive(false);
        settingsUi.SetActive(true);
        menuButtonsUi.SetActive(false);
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

        if (paused)
            textDescription.text = "Press Escape to resume";
        else
            textDescription.text = "";
    }

    public void Button_OpenWebsite()
    {
        Application.OpenURL("https://testnets.opensea.io/collection/test-terrain");
    }

    #endregion

    #region OpenSea API

    [Header("OpenSea API")]
    public string collectionAddress = "0x";
    public string apiUrl = "https://testnets-api.opensea.io/api/v1/asset/";
    public InputField inputFieldTokenId;
    public Button buttonFindNft;
    public GameObject nftPlayUi;
    public Text textNftSeed;
    public Text textNftSize;

    public void Button_FindNFT()
    {
        int token;
        if (int.TryParse(inputFieldTokenId.text, out token))
        {
            // see if its an int, else cant find nft etc
        }// also check if input lengeth is > 0

        buttonFindNft.interactable = false;

        //Debug.Log($"{apiUrl}{collectionAddress}/{inputFieldTokenId.text}");

        HttpWebRequest webRequest;
        webRequest = (HttpWebRequest)WebRequest.Create($"{apiUrl}{collectionAddress}/{inputFieldTokenId.text}");
        webRequest.ContentType = "application/json; charset=utf-8";
        webRequest.Method = "GET";

        try
        {
            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
            Stream stream = webResponse.GetResponseStream();
            StreamReader streamReader = new StreamReader(stream);
            string result = streamReader.ReadToEnd();

            NFT_Data nft = new NFT_Data();
            nft = JsonUtility.FromJson<NFT_Data>(result);

            foreach (Trait trait in nft.traits)
            {
                if (trait.trait_type == "Seed")
                {
                    World.instance.seed = int.Parse(trait.value);
                    textNftSeed.text = $"Seed: {trait.value}";
                }

                if (trait.trait_type == "Size")
                {
                    textNftSize.text = $"Size: {trait.value}";

                    switch (trait.value)
                    {
                        case "Small":
                            World.instance.mapSize = new Vector2Int(10, 10);
                            break;

                        case "Normal":
                            World.instance.mapSize = new Vector2Int(20, 20);
                            break;

                        case "Massive":
                            World.instance.mapSize = new Vector2Int(30, 30);
                            break;
                    }
                }
            }

            nftPlayUi.SetActive(true);
            openSeaUi.SetActive(false);

            SetMessageText("Successfully recieved NFT", 10, new Color(0, 1, 0, .8f));
        }
        catch (System.Exception ex)
        {
            //Debug.Log(ex.Message);
            SetMessageText(ex.Message, 10, new Color(1, 0, 0, .8f));
        }

        buttonFindNft.interactable = true;
    }

    [System.Serializable]
    public class NFT_Data
    {
        public int id;
        public List<Trait> traits = new List<Trait>();
    }

    [System.Serializable]
    public class Trait
    {
        public string trait_type;
        public string value;
    }

    #endregion

    #endregion

    public void FinishedLoading()
    {
        if (!loadingUi.activeInHierarchy) return;

        lighting.fog = true;
        lighting.UpdateShader();

        loadingCamera.SetActive(false);
        loadingUi.SetActive(false);

        AudioManager.instance.playMusic = false;

        Instantiate(playerPrefab, null);
    }


    public void ChangeMapSize()
    {
        World world = World.instance;

        switch (dropdownMapSize.value)
        {
            case 0: // TINY
                world.mapSize = new Vector2Int(10, 10);
                break;

            case 1: // NORMAL
                world.mapSize = new Vector2Int(20, 20);
                break;

            case 2: // MASSIVE
                world.mapSize = new Vector2Int(30, 30);
                break;
        }

        loadingCamera.GetComponent<LoadingCamera>().SetupCameraPosition(world);
    }


    public void SetDefaultMapSize()
    {
        World.instance.mapSize = new Vector2Int(20, 20);
    }
}
