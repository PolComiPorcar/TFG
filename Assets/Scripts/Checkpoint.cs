using UnityEngine;
using UnityEngine.Events;

public class Checkpoint : MonoBehaviour
{
    public UnityEvent<Checkpoint, Collider2D> OnCheckpointEntered;

    void OnTriggerEnter2D(Collider2D other)
    {
        OnCheckpointEntered.Invoke(this, other);
    }
}
