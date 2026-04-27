using UnityEngine;

// CollectibleIdentifier
// Simple marker component used to identify collectible objects in the scene.
// Designed to be lightweight: it only exposes methods to mark the collectible
// as collected (disables the GameObject and emits an event) and to reset it.
public class CollectibleIdentifier : MonoBehaviour
{
    // Mark this collectible as collected: disable the GameObject and notify listeners.
    public void SetAsCollected()
    {   
        this.gameObject.SetActive(false);
        EventManager.OnCubeCollected?.Invoke();
    }

    // Reset the collectible to its default (active) state.
    public void ResetState()
    {
        this.gameObject.SetActive(true);
    }
}
