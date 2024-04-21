using System;
using UnityEngine;

public class Pillar : MonoBehaviour
{
    [NonSerialized] public bool locked;
    [SerializeField] private float fac = 1;
    [SerializeField] private Collider col;

    private void Update()
    {
        float tickFac = fac * Time.deltaTime * GameState.pillarFactor;
        
        transform.RotateAroundLocal(Vector3.up, tickFac);

        if (locked) return;

        // fall
        if (Player.CharacterMovement.groundCollider == col)
            transform.Translate(Vector3.down * 6 * Time.deltaTime * GameState.pillarFactor, Space.World);
        // rise
        else
            transform.Translate(Vector3.up * (tickFac * .1f), Space.World);
    }
}
