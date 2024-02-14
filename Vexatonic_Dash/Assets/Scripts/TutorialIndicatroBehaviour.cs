using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//아 오타..    
public class TutorialIndicatroBehaviour : MonoBehaviour
{
    private List<SpriteRenderer> srs;
    private Vector3 initPos;
    private Vector3 destPos;
    private Color c = Color.white;
    private void Start()
    {
        srs = GetComponentsInChildren<SpriteRenderer>().ToList();
        initPos = transform.position;
        destPos = initPos + new Vector3(0.4f, 0.8f);
    }

    public IEnumerator Fadeout() {
        float time = 0f;
        while (time < 0.5f) {
            transform.position = 2 * (time * destPos + (0.5f - time) * initPos);
            c.a = 1 - 2 * time;
            foreach (var sr in srs) {
                sr.color = c;
            }
            time += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

}
