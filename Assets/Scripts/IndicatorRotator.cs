using UnityEngine;

public class IndicatorRotator : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 100f;


    void Update()
    {
        transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
    }
}
