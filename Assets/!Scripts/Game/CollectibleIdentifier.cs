using UnityEngine;

// This class just act as an identifier. No callbacks therefore, better performance
public class CollectibleIdentifier : MonoBehaviour
{
    public void SetAsCollected()
    {   
        this.gameObject.SetActive(false);
        EventManager.OnCubeCollected?.Invoke();
    }

    public void ResetState()
    {
        this.gameObject.SetActive(true);
    }
}
