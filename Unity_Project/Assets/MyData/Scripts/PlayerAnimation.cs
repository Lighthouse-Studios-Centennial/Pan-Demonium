using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimation : NetworkBehaviour
{
    [SerializeField] private PlayerController playerController;

    private readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int PickupIngredientHash = Animator.StringToHash("PickupIngredient");
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        playerController.OnPickedupSomething += PlayerController_OnPickedupSomething;
        playerController.OnDroppedSomething += PlayerController_OnDroppedSomething;
    }

    private void OnDisable()
    {
        playerController.OnPickedupSomething -= PlayerController_OnPickedupSomething;
        playerController.OnDroppedSomething -= PlayerController_OnDroppedSomething;
    }

    private void PlayerController_OnDroppedSomething(object sender, System.EventArgs e)
    {
        animator.SetBool(PickupIngredientHash, false);
    }

    private void PlayerController_OnPickedupSomething(object sender, System.EventArgs e)
    {
        animator.SetBool(PickupIngredientHash, true);
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

    }

    private void LateUpdate()
    {
        //animator.SetBool(IsWalkingHash, playerController.IsWalking.Value);
        animator.SetBool(IsWalkingHash, playerController.IsWalking);
    }

}
