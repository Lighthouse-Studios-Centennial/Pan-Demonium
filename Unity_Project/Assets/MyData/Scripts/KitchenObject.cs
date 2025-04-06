using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using DG.Tweening;

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    [SerializeField] private LayerMask collisionLayerMask;
    private FollowTransform followTransform;

    private IKitchenObjectParent kitchenObjectParent;

    public KitchenObjectSO GetKitchenObjectSO() { return kitchenObjectSO; }

    public IKitchenObjectParent GetKitchenObjectParent() { return kitchenObjectParent; }

    private bool isThrowing = false;
    private float gravity = 9.81f;
    Sequence throwTween;

    protected virtual void Awake()
    {
        followTransform = GetComponent<FollowTransform>();
    }
    private void Update()
    {
        if (isThrowing)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, 0.7f, collisionLayerMask);
            foreach (var hit in hits)
            {
                if (hit.gameObject != this.gameObject)
                {
                    Debug.Log("Fake collision with " + hit.name);
                    throwTween.Pause();

                    hit.TryGetComponent(out IKitchenObjectParent kitchenObjectParent);
                    if (kitchenObjectParent != null && kitchenObjectParent is not ContainerCounter && !kitchenObjectParent.HasKitchenObject())
                    {
                        this.SetKitchenObjectParent(kitchenObjectParent);
                    }
                    else
                    {
                        //fall speed related to gravaty
                        var fallSpeed = transform.position.y / gravity;
                        transform.DOMoveY(0, fallSpeed).SetEase(Ease.Linear).OnComplete(() =>
                        {
                            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
                        });
                    }

                    isThrowing = false;
                    break;
                }
            }
        }
    }
    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
        SetKitchenObjectParentServerRpc(kitchenObjectParent.GetNetworkObject());
    }

    public void ThrowKitchenObject(NetworkObjectReference objectRef, Vector3 throwPos, Vector3 throwDir)
    {
        ThrowKitchenObjectServerRpc(objectRef, throwPos, throwDir);
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
    private void ThrowKitchenObjectServerRpc(NetworkObjectReference objectRef, Vector3 throwPos, Vector3 throwDir)
    {
        ThrowKitchenObjectClientRpc(objectRef, throwPos, throwDir);
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
    private void ThrowKitchenObjectClientRpc(NetworkObjectReference objectRef, Vector3 throwPos, Vector3 throwDir)
    {
        if (objectRef.TryGet(out var kitchenObjectNetworkObject))
        {
            var throwDistance = 5f;

            this.kitchenObjectParent?.ClearKitchenObject();
            followTransform.SetTargetTransform(null);

            Vector3 target = throwPos + throwDir * throwDistance - new Vector3(0, 1.5f, 0);

            throwTween = DOTween.Sequence();
            throwTween.Append(transform.DOJump(target, 1f, 1, 1f).SetEase(Ease.OutExpo));
            throwTween.OnComplete(() =>
            {
                isThrowing = false;
            });

            isThrowing = true;
        }
    }

}
