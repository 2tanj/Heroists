using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float _minZoom = -1f, _maxZoom = -50f, _zoomIncrement = .5f;

    private        Camera _camera;

    private        Vector3 _dragOrigin;

    private static Unit    _unitToFollow = null;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (_unitToFollow)
            Camera.main.transform.position =
                            new Vector3(_unitToFollow.transform.position.x,
                                        _unitToFollow.transform.position.y,
                                        _camera.transform.position.z);

        CameraZoom();
        CameraPan();
    }

    public static Vector3 GetWorldPosition(float z)
    {
        // Camera.main for static method
        var mousePos = Camera.main.ScreenPointToRay(Input.mousePosition);
        var ground = new Plane(Vector3.forward, new Vector3(0, 0, z));

        float distance;
        ground.Raycast(mousePos, out distance);

        return mousePos.GetPoint(distance);
    }

    private void CameraPan()
    {
        // saving the starting point of the first click
        if (Input.GetMouseButtonDown(2))
            _dragOrigin = GetWorldPosition(0);

        // moving the camera using the difference in current and starting position
        if (Input.GetMouseButton(2))
            _camera.transform.position += _dragOrigin - GetWorldPosition(0);
    }
    private void CameraZoom()
    {
        if (Input.mouseScrollDelta != Vector2.zero)
        {
            if (Input.mouseScrollDelta.y < 0 && _camera.transform.position.z <= _maxZoom
                || Input.mouseScrollDelta.y > 0 && _camera.transform.position.z >= _minZoom)
                return;

            var pos = _camera.transform.position;
            pos.z += Input.mouseScrollDelta.y * _zoomIncrement;
            _camera.gameObject.transform.position = pos;
        }
    }

    public static void FollowUnit(Unit u) => _unitToFollow = u;
}
