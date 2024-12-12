using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public bool matched;

    public Image frontImage;
    public CardGameMain.cardTypes cardType;
    public Animator anim;
    public void PlayAnim(string AnimName){
        anim.Play(AnimName);

    }
    public void onCardPress(){
        if (matched){
            return;
            
        }
        CardGameMain.main.onCardSelection(this);
        
    }

    
}
