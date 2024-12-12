using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



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
        yeti = 8
        
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
    

    public void onCardSelection(CardUI cardUIelement){
        

        if (card1 == null){
            card1 = cardUIelement;
            card1.PlayAnim("FlipOpen");
            return;
        }
        
        if (card2 == null){
            card2 = cardUIelement;
        }
        card2.PlayAnim("FlipOpen");


        if (card1.cardType == card2.cardType){
            card1.matched = true;
            card2.matched = true;
            count += 1;
            
        }
        else{

            card1.PlayAnim("FlipClose");
            card2.PlayAnim("FlipClose");
        }
        card1 = null;
        card2 = null;
        return;
    }

    void Start()
    {
        main = this;
        var x_pos = -350;//-700 : 700
        var y_pos = 400; //-400 : 400
        cardTypes[] cardsType = new cardTypes[]{ cardTypes.club, cardTypes.spade, cardTypes.heart, cardTypes.diamond,
            cardTypes.joker, cardTypes.orange_mushroom, cardTypes.slime, cardTypes.yeti,
            cardTypes.club, cardTypes.spade, cardTypes.heart, cardTypes.diamond,
            cardTypes.joker, cardTypes.orange_mushroom, cardTypes.slime, cardTypes.yeti};

        int i = 0;

        for (int x = 0; x < 4; x++){
            
            for (int y = 0; y < 4; y++){
                CardUI clone = Instantiate(prefabCardUI,canvas);
                clone.transform.localPosition = new Vector3(x_pos + 200* x, y_pos - 250 * y);
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
                i++;

            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
