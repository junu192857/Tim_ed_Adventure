using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeObjectBehaviour : MonoBehaviour
{
    SpriteRenderer sr;
    Vector3 initialPos;
    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        initialPos = transform.position;
        StartCoroutine(FadeoutObject());
    }

    private IEnumerator FadeoutObject()
    {
        Color c = sr.color;
        float time = 0f;
        while (time < 1f) {
            c.a = Mathf.Min(1, 2 - 2 * time);
            sr.color = c;
            time += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);

    }
}
