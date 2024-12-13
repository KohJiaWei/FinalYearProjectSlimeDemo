using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public bool isAlreadyMatchedWithAnotherCard;
    public Image frontImage;
    public CardGameMain.cardTypes cardType;
    public Animator anim;
    public void PlayAnim(string AnimName, System.Action funcCall)
    {
        anim.speed = 0.25f;
        StartCoroutine(PlayAnimationAfterCurrent("FlipClose",funcCall));
        anim.Play(AnimName, 0, 0);

    }

    public void onCardPress()
    {
        if (isAlreadyMatchedWithAnotherCard)
        {
            return;

        }
        CardGameMain.main.onCardSelection(this);

    }
    IEnumerator PlayAnimationAfterCurrent(string Animation, System.Action funcCall)
    {
        // Play the first animation
        anim.Play(Animation);

        // Wait for the first animation to end
        yield return new WaitForSeconds(4/6f);
        funcCall();

    }



}

