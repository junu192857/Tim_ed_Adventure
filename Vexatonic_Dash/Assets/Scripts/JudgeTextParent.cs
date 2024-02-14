using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeTextParent : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(StartMoveCoroutine());
    }

    private IEnumerator StartMoveCoroutine() {
        float time = 0f;
        Vector3 initPos = transform.position;
        Vector3 destPos = initPos + Quaternion.AngleAxis(Camera.main.transform.rotation.z, Vector3.forward) * Vector3.up;
        while (time < 1f) {
            transform.position = initPos * (1 - time) + destPos * time;
            time += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
