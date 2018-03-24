using UnityEngine;

public class EndZone : MonoBehaviour
{
    public LevelManager LevelManager { get; set; }
    public Collider2D CarCollider { get; set; }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log(other.gameObject.name);
        if (other == CarCollider)
        {
            LevelManager.OnEnterEndZone();
        }
    }
}