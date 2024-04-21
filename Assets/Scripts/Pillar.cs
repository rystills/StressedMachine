using System;
using UnityEngine;

public class Pillar : MonoBehaviour
{
    [NonSerialized] public bool locked;
    [SerializeField] private float fac = 1;
    [SerializeField] private Collider col;
    [SerializeField] private AudioSource rotateSnd;
    [SerializeField] private AudioSource fallSnd;

    private void OnEnable() => rotateSnd.Play();

    private void Update()
    {
        float tickFac = fac * Time.deltaTime * GameState.pillarFactor;
        
        transform.RotateAroundLocal(Vector3.up, tickFac);

        rotateSnd.pitch = GameState.powerDownFactor * fac * .2f;
        
        if (locked) return;

        // fall
        if (Player.CharacterMovement.groundCollider == col)
        {
            transform.Translate(Vector3.down * 6 * Time.deltaTime * GameState.pillarFactor, Space.World);
            if (!fallSnd.isPlaying) fallSnd.Play();
        }
        // rise
        else
        {
            transform.Translate(Vector3.up * (tickFac * .1f), Space.World);
            fallSnd.Stop();
        }
    }
}
