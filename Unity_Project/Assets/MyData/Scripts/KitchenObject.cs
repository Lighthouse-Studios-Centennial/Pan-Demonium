using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using DG.Tweening;

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    private FollowTransform followTransform;

    private IKitchenObjectParent kitchenObjectParent;

    public KitchenObjectSO GetKitchenObjectSO() { return kitchenObjectSO; }

    public IKitchenObjectParent GetKitchenObjectParent() { return kitchenObjectParent; }

    protected virtual void Awake()
    {
        followTransform = GetComponent<FollowTransform>();
    }

    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
        if (kitchenObjectParent == null)
        {
            // Debug.LogError("KitchenObjectParent is null");
            SetKitchenObjectParentServerRpc();
            return;
        }

        SetKitchenObjectParentServerRpc(kitchenObjectParent.GetNetworkObject());
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void ClearKitchenObjectOnParent()
    {
        kitchenObjectParent.ClearKitchenObject();
    }

    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
    {
        if (this is PlateKitchenObject)
        {
            plateKitchenObject = this as PlateKitchenObject;
            return true;
        }
        else
        {
            plateKitchenObject = null;
            return false;
        }
    }

    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        KitchenGameMultiplayer.Instance.SpawnKitchenObject(kitchenObjectSO, kitchenObjectParent);
    }

    public static void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        KitchenGameMultiplayer.Instance.DestroyKitchenObject(kitchenObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectParentServerRpc()
    {
        SetKitchenObjectParentClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectParentServerRpc(NetworkObjectReference kitchenObjectParentNetworkObjectRef)
    {
        SetKitchenObjectParentClientRpc(kitchenObjectParentNetworkObjectRef);
    }

    [ClientRpc]
    private void SetKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectParentNetworkObjectRef)
    {
        kitchenObjectParentNetworkObjectRef.TryGet(out var kitchenObjectNetworkObject);
        var kitchenObjectParent = kitchenObjectNetworkObject.GetComponent<IKitchenObjectParent>();

        this.kitchenObjectParent?.ClearKitchenObject();

        this.kitchenObjectParent = kitchenObjectParent;
        if (kitchenObjectParent.HasKitchenObject())
        {
            Debug.LogError("Counter already has kitchen object");
        }

        kitchenObjectParent.SetKitchenObject(this);

        followTransform.SetTargetTransform(kitchenObjectParent.GetKitchenObjectFollowTransform());
    }

    [ClientRpc]
    private void SetKitchenObjectParentClientRpc()
    {
        var parentPosition = kitchenObjectParent.GetKitchenObjectFollowTransform().position;
        var parentRotation = kitchenObjectParent.GetKitchenObjectFollowTransform().rotation;
        var direction = kitchenObjectParent.GetKitchenObjectFollowTransform().forward;
        var throwDistance = 4f;
        this.kitchenObjectParent?.ClearKitchenObject();

        followTransform.SetTargetTransform(null);

        var sequence = DOTween.Sequence();

        sequence.Append(transform.DOJump(parentPosition + direction * throwDistance - new Vector3(0, 1.5f, 0), 0.8f, 1, 0.3f).SetEase(Ease.OutFlash));
        sequence.Append(transform.DOMove(new Vector3(direction.x, 0, direction.y) * 2, 0.2f).SetRelative(true).SetEase(Ease.OutFlash));
    }

}
