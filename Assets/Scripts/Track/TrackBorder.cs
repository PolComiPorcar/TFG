using UnityEngine;
using UnityEngine.Events;

public class TrackBorder : MonoBehaviour
{
    private RaceController parentController;

    void Start()
    {
        parentController = GetComponentInParent<RaceController>();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (parentController != null) parentController.OnBorderTriggerExit(collision);
    }
}
