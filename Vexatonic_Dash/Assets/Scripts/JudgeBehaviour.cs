using System;
using UnityEngine;
using UnityEngine.UI;

public class JudgeBehaviour : MonoBehaviour
{
    public static float JudgeDisplayTime = 0.5f;

    [SerializeField] private Text text;
    private float _startTime;

    private void Update()
    {
        if (Time.time - _startTime >= JudgeDisplayTime) Destroy(gameObject);
    }

    public void Initialize(JudgementType type)
    {
        _startTime = Time.time;

        text.text = type switch
        {
            JudgementType.PurePerfect => "Perfect",
            JudgementType.Perfect => "Perfect",
            JudgementType.Great => "Great",
            JudgementType.Good => "Good",
            JudgementType.Miss => "Miss",
            JudgementType.Invalid => "Invalid",
            _ => throw new ArgumentException()
        };
    }
}