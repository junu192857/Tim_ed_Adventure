using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterimageBehaviour : MonoBehaviour
{
    SpriteRenderer sr;
    Color c;
    void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        c = sr.color;
        StartCoroutine(CharacterFadeout());
    }

    private IEnumerator CharacterFadeout() {
        float time = 0f;
        while (time < 0.5f) {
            c.a = Mathf.Max(0, 0.5f - time);
            sr.color = c;
            time += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
        
    }
}
