// AnimatorTransitionSniffer.cs  (INSTANCE EVENT SÜRÜMÜ)
using UnityEngine;
using System;
using UnityEngine.AI;

public class AnimatorTransitionSniffer : MonoBehaviour
{
    [Header("Sadece okunur")]
    [SerializeField] private Animator animator;
    [SerializeField] private int animLayer = 0;
    [SerializeField] private NavMeshAgent agent; // opsiyonel, sadece referans için

    [Tooltip("Transition blend’lerinde düşük ağırlıklı event’leri elemek için")]
    [Range(0f, 1f)] public float minWeightForEvent = 0.25f;

    // Geçiş tamamlandığında: (fromHash, toHash)
    public event Action<int, int> TransitionCompleted;

    public bool wasInTransition = false;
    private int fromHash = 0;
    private int lastStableHash = 0;

    // ⚠️ ARTIK STATIC DEĞİL — NPC başına izole olur
    public event Action<string, GameObject, Animator> OnAnimEvent;

    private void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (!animator) { enabled = false; return; }
        if (animLayer < 0 || animLayer >= animator.layerCount) { enabled = false; return; }

        lastStableHash = animator.GetCurrentAnimatorStateInfo(animLayer).fullPathHash;
    }

    private void Update()
    {
        bool inTr = animator.IsInTransition(animLayer);

        if (!wasInTransition && inTr)
        {
            fromHash = animator.GetCurrentAnimatorStateInfo(animLayer).fullPathHash;
            wasInTransition = true;
            return;
        }

        if (wasInTransition && !inTr)
        {
            int toHash = animator.GetCurrentAnimatorStateInfo(animLayer).fullPathHash;

            if (toHash != fromHash)
            {
                //if (logOnComplete)
                    //Debug.Log($"[Animator] Transition complete: {fromHash} -> {toHash}");

                TransitionCompleted?.Invoke(fromHash, toHash);
                lastStableHash = toHash;
            }

            wasInTransition = false;
            return;
        }

        int cur = animator.GetCurrentAnimatorStateInfo(animLayer).fullPathHash;
        if (cur != lastStableHash)
            lastStableHash = cur;

        if (agent != null)
        {
            float speed = agent.velocity.magnitude;
            animator.SetBool("Walk", speed > 0.1f);
        }
    }

    // Animation Clip Event → Function: Fire
    public void Fire(AnimationEvent evt)
    {
        try
        {
            if (evt.animatorClipInfo.weight < minWeightForEvent) return;
        }
        catch { /* bazı Unity sürümlerinde evt.animatorClipInfo erişimi yoksa yoksay */ }

        var name = !string.IsNullOrEmpty(evt.stringParameter) ? evt.stringParameter : "UnnamedEvent";

        // ⚠️ INSTANCE event tetikleniyor
        OnAnimEvent?.Invoke(name, gameObject, animator);

        //Debug.Log($"[AnimEventHub] {name} from {animator?.gameObject.name}");
    }
}
