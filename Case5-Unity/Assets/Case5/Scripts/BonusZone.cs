using UnityEngine;

// Зона бонуса. Когда игрок входит в триггер, включаем анимацию бонуса через Animator,
// когда выходит, анимация возвращается в обычное состояние.
// В Animator Controller есть параметр bool "PlayerInside" и два состояния: Idle и Active
[RequireComponent(typeof(Collider))]
public class BonusZone : MonoBehaviour
{
    public Animator bonusAnimator;
    public string parameterName = "PlayerInside";
    public Light bonusLight; // подсветка, необязательно

    private int paramHash;
    private int playersInside;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        paramHash = Animator.StringToHash(parameterName);
        if (bonusAnimator == null) bonusAnimator = GetComponentInChildren<Animator>();
        SetLight(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playersInside++;
        SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playersInside = Mathf.Max(0, playersInside - 1);
        if (playersInside == 0) SetActive(false);
    }

    private void SetActive(bool value)
    {
        if (bonusAnimator != null) bonusAnimator.SetBool(paramHash, value);
        SetLight(value);
    }

    private void SetLight(bool value)
    {
        if (bonusLight != null) bonusLight.enabled = value;
    }

    // рисуем зону в редакторе, чтобы было видно где она
    void OnDrawGizmos()
    {
        var col = GetComponent<SphereCollider>();
        if (col == null) return;
        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.35f);
        Gizmos.DrawWireSphere(transform.TransformPoint(col.center), col.radius * transform.lossyScale.x);
    }
}
