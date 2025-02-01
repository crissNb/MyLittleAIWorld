using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera _mainCamera; 
    [SerializeField] private NPCController _npcController;
    
    [Header("Click Indicator Settings")]
    [SerializeField] private float _indicatorYOffset = 0.1f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private GameObject _currentIndicator;

    private void Update()
    {
        HandleMovementInput();
    }

    private void HandleMovementInput()
    {
        if (Input.GetMouseButton(1))
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _groundLayer))
            {
                _npcController.Move(hit.point);
                ShowClickIndicator(hit.point);
            }
        }
    }

    private void ShowClickIndicator(Vector3 position)
    {
        _currentIndicator.SetActive(true);
        _currentIndicator.transform.position = position + Vector3.up * _indicatorYOffset;
    }
}