using System.Collections;
using UnityEngine;

public class CameraControl : MonoBehaviour {
    private Camera cam;
    private float defaultSize;
    private Vector3 defaultPos;
    private bool isZooming = false;

    float zoomSpeed = 5f;
    float minSize = 2f;
    float maxSize = 10f;

    Vector3 lastMousePos;

    void Start() {
        cam = GetComponent<Camera>();
        defaultSize = cam.orthographicSize;
        defaultPos = transform.position;
    }

    void Update() {
        if (isZooming) return;

        HandleDrag();
        HandleScroll();
    }

    void HandleDrag() {
        if (Input.GetMouseButtonDown(0)) {
            lastMousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0)) {
            Vector3 cur = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 delta = lastMousePos - cur;
            transform.position += new Vector3(delta.x, delta.y, 0f);
        }
    }

    void HandleScroll() {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0f) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);

        float newSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed, minSize, maxSize);
        float ratio = newSize / cam.orthographicSize;

        transform.position = mouseWorld + (transform.position - mouseWorld) * ratio;
        cam.orthographicSize = newSize;
    }

    public void ZoomInTarget(Vector3 targetPos, float duration) {
        if (!isZooming) {
            StartCoroutine(ZoomRoutine(targetPos, duration));
        }
    }

    private IEnumerator ZoomRoutine(Vector3 targetPos, float duration) {
        isZooming = true;

        float elapsed = 0f;
        float zoomSize = 2.0f;

        Vector3 focusPos = new Vector3(targetPos.x, targetPos.y, -10f);

        while (elapsed < 0.5f) {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / 0.5f);

            cam.orthographicSize = Mathf.Lerp(defaultSize, zoomSize, t);
            transform.position = Vector3.Lerp(defaultPos, focusPos, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        elapsed = 0f;
        while (elapsed < 0.5f) {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / 0.5f);

            cam.orthographicSize = Mathf.Lerp(zoomSize, defaultSize, t);
            transform.position = Vector3.Lerp(focusPos, defaultPos, t);
            yield return null;
        }

        isZooming = false;
    }
}