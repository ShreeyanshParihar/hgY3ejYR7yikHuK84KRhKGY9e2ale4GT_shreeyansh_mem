using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class _CardGameManager : MonoBehaviour
{

    public static _CardGameManager Instance;
    public static int gameSizeX = 2;
    public static int gameSizeY = 2;
    // gameobject instance
    [SerializeField]
    private GameObject prefab;
    // parent object of cards
    [SerializeField]
    private GameObject cardList;
    // sprite for card back
    [SerializeField]
    private Sprite cardBack;
    // all possible sprite for card front
    [SerializeField]
    private Sprite[] sprites;
    // list of card
    private _Card[] cards;
    [SerializeField]
    private GameObject menu;
    [SerializeField]
    private GameObject panel;
    [SerializeField]
    private GameObject info;
    //we place card on this panel
    [SerializeField]
    private GameObject cardFloor;
    // other UI
    [SerializeField]
    private Text sizeLabel;
    [SerializeField]
    private Slider sizeSliderX;
    [SerializeField]
    private Slider sizeSliderY;
    [SerializeField]
    private Text timeLabel;
    [SerializeField]
    private Text timeLabel_infoPanel;
    private float time;

    private int spriteSelectedId;
    private int cardSelectedIndex;
    private int cardLeft;
    private bool gameStart;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        gameStart = false;
        panel.SetActive(false);
        info.SetActive(false);
        menu.SetActive(true);
    }

    // Start a game
    public void StartCardGame()
    {
        if (gameStart) return; // return if game already running
        gameStart = true;
        // toggle UI
        menu.SetActive(false);
        panel.SetActive(true);
        info.SetActive(false);
        // set cards, size, position
        SetGamePanel();
        // renew gameplay variables
        cardSelectedIndex = spriteSelectedId = -1;
        cardLeft = cards.Length;
        // allocate sprite to card
        SpriteCardAllocation();
        StartCoroutine(HideFace());
        time = 0;
    }

    // Initialize cards, size, and position based on size of game
    private void SetGamePanel()
    {
        // if game is odd, we should have 1 card less
        int isOdd = (gameSizeX * gameSizeY) % 2;

        cards = new _Card[gameSizeX * gameSizeY - isOdd];
        // remove all gameobject from parent
        foreach (Transform child in cardList.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        cardList.transform.localPosition = cardFloor.transform.localPosition;

        // calculate position between each card & start position of each card based on the Panel
        RectTransform panelsize = cardFloor.transform.GetComponent(typeof(RectTransform)) as RectTransform;
        float row_size = panelsize.rect.width;
        float col_size = panelsize.rect.height;
        float xInc = row_size / gameSizeX;
        float yInc = col_size / gameSizeY;
        float curX = -xInc * (float)(gameSizeX / 2);
        float curY = -yInc * (float)(gameSizeY / 2);
        Vector2 cardSize = new Vector2(xInc * 0.9f, yInc * 0.9f);

        if (isOdd == 0)
        {
            if (gameSizeX % 2 == 0) curX += xInc / 2;
            if (gameSizeY % 2 == 0) curY += yInc / 2;
        }
        float initialX = curX;
        // for each in y-axis
        for (int i = 0; i < gameSizeY; i++)
        {
            curX = initialX;
            // for each in x-axis
            for (int j = 0; j < gameSizeX; j++)
            {
                GameObject c;
                // if is the last card and game is odd, we instead move the middle card on the panel to last spot
                if (isOdd == 1 && i == (gameSizeY - 1) && j == (gameSizeX - 1))
                {
                    int index = gameSizeY / 2 * gameSizeX + gameSizeX / 2;
                    c = cards[index].gameObject;
                }
                else
                {
                    // create card prefab
                    c = Instantiate(prefab);
                    // assign parent
                    c.transform.SetParent(cardList.transform, worldPositionStays: false);

                    int index = i * gameSizeX + j;
                    cards[index] = c.GetComponent<_Card>();
                    cards[index].ID = index;
                    // modify its size
                    c.GetComponent<RectTransform>().sizeDelta = cardSize;
                }
                // assign location
                c.transform.localPosition = new Vector3(curX, curY, 0);
                curX += xInc;

            }
            curY += yInc;
        }

    }
    // reset face-down rotation of all cards
    void ResetFace()
    {
        for (int i = 0; i < cards.Length; i++)
            cards[i].ResetRotation();
    }
    // Flip all cards after a short period
    IEnumerator HideFace()
    {
        //display for a short moment before flipping
        yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < cards.Length; i++)
            cards[i].Flip();
        yield return new WaitForSeconds(0.5f);
    }
    // Allocate pairs of sprite to card instances
    private void SpriteCardAllocation()
    {
        int i, j;
        int[] selectedID = new int[cards.Length / 2];
        // sprite selection
        for (i = 0; i < cards.Length / 2; i++)
        {
            // get a random sprite
            int value = Random.Range(0, sprites.Length - 1);
            // check previous number has not been selection
            // if the number of cards is larger than number of sprites, it will reuse some sprites
            for (j = i; j > 0; j--)
            {
                if (selectedID[j - 1] == value)
                    value = (value + 1) % sprites.Length;
            }
            selectedID[i] = value;
        }

        // card sprite deallocation
        for (i = 0; i < cards.Length; i++)
        {
            cards[i].Active();
            cards[i].SpriteID = -1;
            cards[i].ResetRotation();
        }
        // card sprite pairing allocation
        for (i = 0; i < cards.Length / 2; i++)
            for (j = 0; j < 2; j++)
            {
                int value = Random.Range(0, cards.Length - 1);
                while (cards[value].SpriteID != -1)
                    value = (value + 1) % cards.Length;

                cards[value].SpriteID = selectedID[i];
            }

    }
    // Slider update gameSize
    public void SetGameSizeX()
    {
        gameSizeX = (int)sizeSliderX.value;
        updateGameSizeLabel();
    }

    public void SetGameSizeY()
    {
        gameSizeY = (int)sizeSliderY.value;
        updateGameSizeLabel();
    }

    void updateGameSizeLabel()
    {
        sizeLabel.text = gameSizeX + " X " + gameSizeY;
    }
    // return Sprite based on its id
    public Sprite GetSprite(int spriteId)
    {
        return sprites[spriteId];
    }
    // return card back Sprite
    public Sprite CardBack()
    {
        return cardBack;
    }
    // check if clickable
    public bool canClick()
    {
        if (!gameStart)
            return false;
        return true;
    }
    // card onclick event
    public void cardClicked(int spriteId, int cardIndex)
    {
        // first card selected
        if (spriteSelectedId == -1)
        {
            spriteSelectedId = spriteId;
            cardSelectedIndex = cardIndex;
        }
        else
        { // second card selected
            if (spriteSelectedId == spriteId)
            {
                //correctly matched
                cards[cardSelectedIndex].Inactive();
                cards[cardIndex].Inactive();
                cardLeft -= 2;
                if (!CheckGameWin()) AudioPlayer.Instance.PlayAudio(2);
            }
            else
            {
                // incorrectly matched
                cards[cardSelectedIndex].Flip();
                cards[cardIndex].Flip();
                AudioPlayer.Instance.PlayAudio(3);
            }
            cardSelectedIndex = spriteSelectedId = -1;
        }
    }
    // check if game is completed
    private bool CheckGameWin()
    {
        // win game
        if (cardLeft == 0)
        {
            DisplayInfo();
            AudioPlayer.Instance.PlayAudio(1);
            return true;
        }
        return false;
    }
    // stop game
    private void EndGame()
    {
        CancelInvoke(nameof(EndGame));
        gameStart = false;
        panel.SetActive(false);
        info.SetActive(false);
        menu.SetActive(true);
    }
    public void GiveUp()
    {
        EndGame();
    }
    public void SaveAndExit()
    {
        gameStart = false;
        _GameFrame saveGameFrame = new _GameFrame();
        saveGameFrame.time = time;
        saveGameFrame.gameSizeX = gameSizeX;
        saveGameFrame.gameSizeY = gameSizeY;
        saveGameFrame.cards = new _CardFrame[cards.Length];
        for (int i = 0; i < cards.Length; i++)
        {
            _CardFrame cardFrame = new _CardFrame();
            cardFrame.id = cards[i].ID;
            cardFrame.spriteID = cards[i].SpriteID;
            cardFrame.flipped = cards[i].Flipped;
            cardFrame.isInactive = cards[i].IsInactive;
            saveGameFrame.cards[i] = cardFrame;
        }
        //TODO : save data
        EndGame();
    }

    public void LoadLastGame()
    {
        //TODO : Load last game
    }

    public void DisplayInfo()
    {
        gameStart = false;
        info.SetActive(true);
        timeLabel_infoPanel.text = time + "s";
        Invoke(nameof(EndGame), 3f);
    }
    // track elasped time
    private void Update()
    {
        if (gameStart)
        {
            time += Time.deltaTime;
            timeLabel.text = "Time: " + time + "s";
        }
    }
}
