using System.Collections.Generic;
using UnityEngine;

public class PillarMachine : MonoBehaviour
{
    public static LEDMachine instance;

    [SerializeField] private List<GameObject> pillars;
    [SerializeField] private float rotSpeed;

    private void LateUpdate()
    {
        pillars.ForEach(p => p.transform.RotateAroundLocal(Vector3.up, rotSpeed * Time.deltaTime));
    }
}
