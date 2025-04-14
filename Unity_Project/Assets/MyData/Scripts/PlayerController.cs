using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour, IKitchenObjectParent
{
    public static PlayerController LocalInstance { get; private set; }

    public static event EventHandler OnAnyPlayerSpawned;
    public static event EventHandler OnAnyPlayerPickedupSomething;

    public event EventHandler OnPickedupSomething;
    public event EventHandler OnDroppedSomething;

    public class OnSelectedCounterChangedEventArgs : EventArgs { public BaseCounter selectedCounter; }
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private LayerMask counterLayerMask;
    [SerializeField] private LayerMask collisionLayerMask;
    [SerializeField] private LayerMask kitchenObjectLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;
    [SerializeField] private PlayerVisual playerVisual;
    //public NetworkVariable<bool> IsWalking { get; private set; } = new NetworkVariable<bool>(false);
    public bool IsWalking { get; private set; }
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject selectedKitchenObject;
    private KitchenObject holdingKitchenObject;

    private bool isDashing = false;
    private float dashInitialSpeedMultiplier = 6f;
    private float dashEndSpeedMultiplier = 1.5f;
    private float dashDuration = 0.3f;
    private float dashCooldown = 0.7f;
    private float lastDashTime = -Mathf.Infinity;


    public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
        OnAnyPlayerPickedupSomething = null;
    }

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            LocalInstance = this;
        }

        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && HasKitchenObject())
        {
            KitchenObject.DestroyKitchenObject(GetKitchenObject());
        }
    }

    private void OnEnable()
    {
        InputHandler.Instance.OnInteractAction += OnInteractAction;
        InputHandler.Instance.OnInteractAltAction += OnInteractAltAction;
        InputHandler.Instance.OnThrowAction += OnThrowAction;
    }

    private void OnDisable()
    {
        InputHandler.Instance.OnInteractAction -= OnInteractAction;
        InputHandler.Instance.OnInteractAltAction -= OnInteractAltAction;
        InputHandler.Instance.OnThrowAction -= OnThrowAction;
    }

    private void OnInteractAltAction(object sender, EventArgs e)
    {
        if(!GameManager.Instance.IsGamePlaying()) { return; }

        if (selectedCounter != null)
        {
            selectedCounter.InteractAlt(this);
        }
        //else if (selectedKitchenObject != null)
        //{
        //    selectedKitchenObject.SetKitchenObjectParent(this);
        //}
    }

    private void OnInteractAction(object sender, EventArgs e)
    {
        if(!GameManager.Instance.IsGamePlaying()) { return; }

        if(selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
        else if(selectedKitchenObject != null && GetKitchenObject() == null)
        {
            selectedKitchenObject.SetKitchenObjectParent(this);
        }
    }

    private void OnThrowAction(object sender, EventArgs e)
    {
        Debug.Log("Try to throw");
        if (!GameManager.Instance.IsGamePlaying() || !IsOwner) { return; }

        if(GetKitchenObject() != null)
        {
            var throwPos = GetKitchenObject().transform.position;
            var throwDir = transform.forward;
            GetKitchenObject().ThrowKitchenObject(NetworkObject, throwPos, throwDir);
        }
    }

    private void Start()
    {
        var playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        playerVisual.SetPlayerColor(KitchenGameMultiplayer.Instance.GetPlayerColor(playerData.colorId));
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        HandleMovement();
        HandleDash();
        HandleInteraction();
    }

    private void HandleInteraction()
    {
        var moveInput = InputHandler.Instance.GetInputVector();

        var moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        if(moveDir != Vector3.zero)
            lastInteractDir = moveDir;

        const float maxInteractionDistance = 2f;

        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit hit, 1.5f, kitchenObjectLayerMask))
        {
            if (hit.transform.TryGetComponent<KitchenObject>(out var ko))
            {
                SetSelectedKitchenObject(ko);
            }
            else
            {
                SetSelectedKitchenObject(null);
            }
        }
        else
        {
            SetSelectedKitchenObject(null);
        }

        if(selectedKitchenObject == null)
        {
            if (Physics.Raycast(transform.position, lastInteractDir, out hit, maxInteractionDistance, counterLayerMask))
            {
                if (hit.transform.TryGetComponent<BaseCounter>(out var baseCounter))
                {
                    SetSelectedCounter(baseCounter);
                }
                else
                {
                    SetSelectedCounter(null);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        

    }

    private void HandleMovement()
    {
        var moveInput = InputHandler.Instance.GetInputVector();
        var moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        var oriMoveDir = moveDir;

        float playerRadius = .5f;
        float speedMultiplier = isDashing ? Mathf.Lerp(dashInitialSpeedMultiplier, dashEndSpeedMultiplier, (Time.time - lastDashTime) / dashDuration) : 1f;
        float moveDistance = moveSpeed * speedMultiplier * Time.deltaTime;

        bool canMove = !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDir, Quaternion.identity, moveDistance, collisionLayerMask);

        if (!canMove)
        {
            Debug.Log("Move Dir: " + moveDir);
            var absX = Mathf.Abs(moveDir.x);
            var absZ = Mathf.Abs(moveDir.z);

            if (absZ > 0.5f && absX > 0.2f || absX > 0.5f)
            {
                // Attempt Only X Movement
                var moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
                canMove = moveDir.x != 0 && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirX, Quaternion.identity, moveDistance, collisionLayerMask);

                if (canMove)
                {
                    moveDistance *= absX;
                    moveDir = moveDirX;
                }
                else
                {
                    // Attempt Only Z Movement
                    var moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                    canMove = moveDir.z != 0 && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirZ, Quaternion.identity, moveDistance, collisionLayerMask);

                    if (canMove)
                    {
                        moveDistance *= absZ;
                        moveDir = moveDirZ;
                    }
                }
            }
            
        }

        if (canMove)
        {
            transform.position += moveDistance * moveDir;
        }

        IsWalking = moveDir != Vector3.zero;

        float rotationSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, oriMoveDir, Time.deltaTime * rotationSpeed);

        HandleDash();
    }
    //private void HandleMovement()
    //{
    //    var moveInput = InputHandler.Instance.GetInputVector();
    //    var moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
    //    var oriMoveDir = moveDir;

    //    float playerRadius = .5f;
    //    float speedMultiplier = isDashing ? Mathf.Lerp(dashInitialSpeedMultiplier, dashEndSpeedMultiplier, (Time.time - lastDashTime) / dashDuration) : 1f;
    //    float moveDistance = moveSpeed * speedMultiplier * Time.deltaTime;

    //    bool canMove = !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDir, Quaternion.identity, moveDistance, collisionLayerMask);

    //    if (!canMove)
    //    {
    //        Debug.Log("Move Dir: " + moveDir);
    //        var absX = Mathf.Abs(moveDir.x);
    //        var absZ = Mathf.Abs(moveDir.z);

    //        if (absZ > 0.5f && absX > 0.2f || absX > 0.5f)
    //        {
    //            // Attempt Only X Movement
    //            var moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
    //            canMove = moveDir.x != 0 && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirX, Quaternion.identity, moveDistance, collisionLayerMask);

    //            if (canMove)
    //            {
    //                moveDistance *= absX;
    //                moveDir = moveDirX;
    //            }
    //            else
    //            {
    //                // Attempt Only Z Movement
    //                var moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
    //                canMove = moveDir.z != 0 && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirZ, Quaternion.identity, moveDistance, collisionLayerMask);

    //                if (canMove)
    //                {
    //                    moveDistance *= absZ;
    //                    moveDir = moveDirZ;
    //                }
    //            }
    //        }

    //    }

    //    if (canMove)
    //    {
    //        transform.position += moveDistance * moveDir;
    //    }

    //    IsWalking.Value = moveDir != Vector3.zero;

    //    float rotationSpeed = 10f;
    //    transform.forward = Vector3.Slerp(transform.forward, oriMoveDir, Time.deltaTime * rotationSpeed);

    //    HandleDash();
    //}

    private void HandleDash()
    {
        if (InputHandler.Instance.IsDashTriggered() && Time.time >= lastDashTime + dashCooldown)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        lastDashTime = Time.time;
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
    }

    private void SetSelectedCounter(BaseCounter counter)
    {
        selectedCounter = counter;
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs { selectedCounter = selectedCounter });
    }
    private void SetSelectedKitchenObject(KitchenObject ko)
    {
        selectedKitchenObject = ko;
    }

    public Transform GetKitchenObjectFollowTransform() { return kitchenObjectHoldPoint; }

    public KitchenObject GetKitchenObject() { return holdingKitchenObject; }

    public void SetKitchenObject(KitchenObject kitchenObject) 
    {
        this.holdingKitchenObject = kitchenObject; 

        if(kitchenObject !=  null)
        {
            OnPickedupSomething?.Invoke(this, EventArgs.Empty);
            OnAnyPlayerPickedupSomething?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ClearKitchenObject() 
    {
        holdingKitchenObject = null;
        OnDroppedSomething?.Invoke(this, EventArgs.Empty);
    }

    public bool HasKitchenObject() { return holdingKitchenObject != null; }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
