using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CardGameMain;

public class CardGameMain : MonoBehaviour
{
    // Start is called before the first frame update
    public enum cardTypes
    {
        diamond = 1,
        spade = 2,
        club = 3,
        heart = 4,
        joker = 5,
        orange_mushroom = 6,
        slime = 7,
        yeti = 8,
    }

    public static CardGameMain main;
    public CardUI prefabCardUI;
    public RectTransform canvas;
    public Sprite diamondSprite;
    public Sprite spadeSprite;
    public Sprite clubSprite;
    public Sprite heartSprite;
    public Sprite jokerSprite;
    public Sprite orangeMushroomSprite;
    public Sprite slimeSprite;
    public Sprite yetiSprite;

    CardUI card1;
    CardUI card2;

    int count = 0;
    int max_x = 4;
    int max_y = 4;
    bool isCurrentlyChecking = false;
    List<GameObject> listOfCardsObject = new List<GameObject>();

    public void onCardSelection(CardUI cardUIelement)
    {
        if (isCurrentlyChecking)
        {
            return;
        }
        if (card1 == null)
        {
            card1 = cardUIelement;
            card1.PlayAnim("FlipOpen", () => { });
            return;
        }
        if (card1 == cardUIelement)
        {
            return;
        }
        card2 = cardUIelement; //right hand doesnt matter whether have card or not
        isCurrentlyChecking = true;
        card2.PlayAnim(
            "FlipOpen",
            () =>
            {
                if (card1.cardType == card2.cardType)
                {
                    card1.isAlreadyMatchedWithAnotherCard = true;
                    card2.isAlreadyMatchedWithAnotherCard = true;
                    count += 1;
                }
                else
                {
                    card1.PlayAnim("FlipClose", () => { });
                    card2.PlayAnim("FlipClose", () => { });
                }
                card1 = null;
                card2 = null;
                isCurrentlyChecking = false;
            }
        );

        return;
    }

    //

    void StartGame()
    {
        var x_pos = -350; //-700 : 700
        var y_pos = 400; //-400 : 400
        
        cardTypes[] cardsType = new cardTypes[]
        {
            cardTypes.club,
            cardTypes.spade,
            cardTypes.heart,
            cardTypes.diamond,
            cardTypes.joker,
            cardTypes.orange_mushroom,
            cardTypes.slime,
            cardTypes.yeti,
            cardTypes.club,
            cardTypes.spade,
            cardTypes.heart,
            cardTypes.diamond,
            cardTypes.joker,
            cardTypes.orange_mushroom,
            cardTypes.slime,
            cardTypes.yeti,
        };

        int i = 0;

        for (int x = 0; x < max_x; x++)
        {
            for (int y = 0; y < max_y; y++)
            {
                
                CardUI clone = Instantiate(prefabCardUI, canvas);
                clone.transform.localPosition = new Vector3(x_pos + 200 * x, y_pos - 250 * y);
                clone.cardType = cardsType[i];
                switch (clone.cardType)
                {
                    case cardTypes.diamond:
                        clone.frontImage.sprite = diamondSprite;
                        break;
                    case cardTypes.spade:
                        clone.frontImage.sprite = spadeSprite;
                        break;
                    case cardTypes.club:
                        clone.frontImage.sprite = clubSprite;
                        break;
                    case cardTypes.heart:
                        clone.frontImage.sprite = heartSprite;
                        break;
                    case cardTypes.joker:
                        clone.frontImage.sprite = jokerSprite;
                        break;
                    case cardTypes.orange_mushroom:
                        clone.frontImage.sprite = orangeMushroomSprite;
                        break;
                    case cardTypes.slime:
                        clone.frontImage.sprite = slimeSprite;
                        break;
                    case cardTypes.yeti:
                        clone.frontImage.sprite = yetiSprite;
                        break;
                }
                listOfCardsObject.Add(clone.gameObject);
                i++;
            }
        }
    }

    void DestroyAllCards()
    {
        foreach (var card in listOfCardsObject)
        {
            Destroy(card);
        }
    }

    void Start()
    {
        main = this;
        Debug.Log("am i called?");
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (count == ((max_x * max_y) / 2))
        {
            DestroyAllCards();
            StartGame();
        }
    }
}
