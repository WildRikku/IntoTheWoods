using System.Collections;
using UnityEngine;

public class Collectible : MonoBehaviour {
    public void PickUp() {
        StartCoroutine(DestroySelf());
    }

    private IEnumerator DestroySelf() {
        yield return new WaitForSeconds(0);
        Destroy(gameObject);
    }
}
