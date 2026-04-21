using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TorchDesync : MonoBehaviour
{
    private Animator animator;

    [Tooltip("This MUST match the exact name of the state in your Animator window.")]
    public string stateName = "Torch_Flicker";

    void Start()
    {
        animator = GetComponent<Animator>();

        float randomStartOffset = Random.Range(0f, 1f);

        animator.Play(stateName, 0, randomStartOffset);

        animator.speed = Random.Range(0.9f, 1.1f);
    }
}