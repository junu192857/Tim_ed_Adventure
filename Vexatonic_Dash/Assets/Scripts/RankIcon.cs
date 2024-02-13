using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RankIcon : MonoBehaviour
{
    private static readonly int AnimShowHash = Animator.StringToHash("Show");
    
    private static readonly Color[] RankColors =
    {
        new(0.9f, 0.7f, 0.1f),  // V Rank
        new(0.8f, 0.7f, 0.1f),  // S Rank
        new(0.4f, 0.7f, 0.4f),  // A Rank
        new(0.2f, 0.6f, 0.7f),  // B Rank
        new(0.4f, 0.4f, 0.9f),  // C Rank
        new(0.6f, 0.4f, 0.7f)   // D Rank
    };
    
    [SerializeField] private Image image;
    [SerializeField] private Text text;
    [SerializeField] private Animator animator;

    public void SetRank(RankType rank)
    {
        image.color = RankColors[(int)rank];
        text.text = rank.ToString();
    }

    public IEnumerator ShowAnimationCoroutine()
    {
        yield return new WaitForEndOfFrame();
        animator.SetTrigger(AnimShowHash);
    }

    // Temporary method for testing
    public void SetRank(int rank)
    {
        SetRank((RankType)rank);
    }
}
