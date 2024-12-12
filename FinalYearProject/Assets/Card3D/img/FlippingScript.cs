using UnityEngine;
using System.Collections;

public class Card3DFlip : MonoBehaviour {
    private bool isFlipping = false;

    public void FlipCard() {
        if (!isFlipping) StartCoroutine(Flip());
    }

    private IEnumerator Flip() {
        isFlipping = true;
        float flipTime = 0.5f; // Adjust as needed
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 180, 0);
        float elapsedTime = 0;

        while (elapsedTime < flipTime) {
            elapsedTime += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsedTime / flipTime);
            yield return null;
        }

        isFlipping = false;
    }

    private void OnMouseDown() {
        FlipCard(); // Trigger the flip when the card is clicked
    }
}
