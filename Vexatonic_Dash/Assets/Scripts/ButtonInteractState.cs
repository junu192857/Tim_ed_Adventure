using UnityEngine;
using UnityEngine.UI;

public class ButtonInteractState : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.GetComponent<Button>() != null)
            animator.GetComponent<Button>().interactable = false;
    }
    
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.GetComponent<Button> () != null)
            animator.GetComponent<Button>().interactable = true;
    }
}
