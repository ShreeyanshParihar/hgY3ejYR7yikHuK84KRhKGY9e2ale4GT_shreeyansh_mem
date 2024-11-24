﻿using System.Collections;
using System.Collections.Generic;
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
    //we place card on this panel
    [SerializeField]
    private GameObject panel;
    [SerializeField]
    private GameObject info;
    // for preloading
    [SerializeField]
    private _Card spritePreload;
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

    private int spriteSelected;
    private int cardSelected;
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
    // Purpose is to allow preloading of panel, so that it does not lag when it loads
    // Call this in the start method to preload all sprites at start of the script
    private void PreloadCardImage()
    {
        for (int i = 0; i < sprites.Length; i++)
            spritePreload.SpriteID = i;
        spritePreload.gameObject.SetActive(false);
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
        cardSelected = spriteSelected = -1;
        cardLeft = cards.Length;
        // allocate sprite to card
        SpriteCardAllocation();
        StartCoroutine(HideFace());
        time = 0;
    }

    // Initialize cards, size, and position based on size of game
    private void SetGamePanel(){
        // if game is odd, we should have 1 card less
        int isOdd = (gameSizeX * gameSizeY) % 2 ;

        cards = new _Card[gameSizeX * gameSizeY - isOdd];
        // remove all gameobject from parent
        foreach (Transform child in cardList.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        // calculate position between each card & start position of each card based on the Panel
        RectTransform panelsize = panel.transform.GetComponent(typeof(RectTransform)) as RectTransform;
        float row_size = panelsize.sizeDelta.x;
        float col_size = panelsize.sizeDelta.y;
        float scale = 1.0f/gameSizeX;
        float xInc = row_size/gameSizeX;
        float yInc = col_size/gameSizeY;
        float curX = -xInc * (float)(gameSizeX / 2);
        float curY = -yInc * (float)(gameSizeY / 2);

        if(isOdd == 0) {
            curX += xInc / 2;
            curY += yInc / 2;
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
                    c.transform.SetParent(cardList.transform,worldPositionStays:false);

                    int index = i * gameSizeX + j;
                    cards[index] = c.GetComponent<_Card>();
                    cards[index].ID = index;
                    // modify its size
                    c.transform.localScale = new Vector3(scale, scale);
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
        for (i = 0; i < cards.Length/2; i++)
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
    public void SetGameSizeX() {
        gameSizeX = (int)sizeSliderX.value;
        updateGameSizeLabel();
    }

    public void SetGameSizeY() {
        gameSizeY = (int)sizeSliderY.value;
        updateGameSizeLabel();
    }

    void updateGameSizeLabel(){
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
    public void cardClicked(int spriteId, int cardId)
    {
        // first card selected
        if (spriteSelected == -1)
        {
            spriteSelected = spriteId;
            cardSelected = cardId;
        }
        else
        { // second card selected
            if (spriteSelected == spriteId)
            {
                //correctly matched
                cards[cardSelected].Inactive();
                cards[cardId].Inactive();
                cardLeft -= 2;
                if(!CheckGameWin()) AudioPlayer.Instance.PlayAudio(2);
            }
            else
            {
                // incorrectly matched
                cards[cardSelected].Flip();
                cards[cardId].Flip();
                AudioPlayer.Instance.PlayAudio(3);
            }
            cardSelected = spriteSelected = -1;
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
    public void DisplayInfo()
    {
        gameStart = false;
        info.SetActive(true);
        timeLabel_infoPanel.text = time + "s";
        Invoke(nameof(EndGame),3f);
    }
    // track elasped time
    private void Update(){
        if (gameStart) {
            time += Time.deltaTime;
            timeLabel.text = "Time: " + time + "s";
        }
    }
}
