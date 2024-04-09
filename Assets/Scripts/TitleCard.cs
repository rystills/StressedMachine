using UnityEngine;

public class TitleCard : MonoBehaviour
{
    private void FixedUpdate()
    {
        if (GameState.state > -1) gameObject.SetActive(false);
    }
}
