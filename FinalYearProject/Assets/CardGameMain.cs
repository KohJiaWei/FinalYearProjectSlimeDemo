using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CardGameMain : MonoBehaviour
{
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
    bool didPlayerWinGame = false;
    int max_x = 4;
    int max_y = 4;
    bool isCurrentlyChecking = false;
    List<GameObject> listOfCardsObject = new List<GameObject>();
    int totalPairs;

    void Start()
    {
        main = this;
        StartGame();
    }

    void StartGame()
    {
        // Resetting game state and adjusting difficulty
        didPlayerWinGame = false;
        totalPairs = (max_x * max_y) / 2; //4*4/2 = 8
        DestroyAllCards();

        var x_pos = -350; //-700 : 700
        var y_pos = 400; //-400 : 400

        List<cardTypes> cardsTypeList = new List<cardTypes>
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

        ShuffleCards(cardsTypeList);

        int i = 0;
        for (int x = 0; x < max_x; x++)
        {
            for (int y = 0; y < max_y; y++)
            {

                CardUI clone = Instantiate(prefabCardUI, canvas);
                clone.transform.localPosition = new Vector3(x_pos + 200 * x, y_pos - 250 * y);
                clone.cardType = cardsTypeList[i];

                AssignCardSprite(clone);
                listOfCardsObject.Add(clone.gameObject);
                i++;
            }
        }
    }

    void ShuffleCards(List<cardTypes> cards)
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = cards[i];
            cards[i] = cards[j];
            cards[j] = temp;
        }

    }

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
                    if (count == totalPairs && !didPlayerWinGame)
                    {
                        didPlayerWinGame = true; //mutex lock
                        StartCoroutine(HandleWinCondition());
                        
                    }
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

        //return;
    }

    void AssignCardSprite(CardUI card)
    {
        switch (card.cardType)
        {
            case cardTypes.diamond:
                card.frontImage.sprite = diamondSprite;
                break;
            case cardTypes.spade:
                card.frontImage.sprite = spadeSprite;
                break;
            case cardTypes.club:
                card.frontImage.sprite = clubSprite;
                break;
            case cardTypes.heart:
                card.frontImage.sprite = heartSprite;
                break;
            case cardTypes.joker:
                card.frontImage.sprite = jokerSprite;
                break;
            case cardTypes.orange_mushroom:
                card.frontImage.sprite = orangeMushroomSprite;
                break;
            case cardTypes.slime:
                card.frontImage.sprite = slimeSprite;
                break;
            case cardTypes.yeti:
                card.frontImage.sprite = yetiSprite;
                break;
        }
    }

    IEnumerator HandleWinCondition()
    {
        yield return new WaitForSeconds(2f);
        DestroyAllCards();
        StartGame();
    }
    
    void DestroyAllCards()
    {
        foreach (var card in listOfCardsObject)
        {
            Destroy(card);
        }
        listOfCardsObject.Clear();
        
    }



    // Update is called once per frame
    void Update()
    {


    }
}
