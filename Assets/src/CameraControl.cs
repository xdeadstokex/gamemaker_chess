using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private Camera cam;
    private float defaultSize;
    private Vector3 defaultPos;
    private bool isZooming = false; // Chống việc zoom chồng lên nhau

    void Start()
    {
        cam = GetComponent<Camera>();
        defaultSize = cam.orthographicSize;
        defaultPos = transform.position; // Đây là vị trí bàn cờ chuẩn
    }

    public void ZoomInTarget(Vector3 targetPos, float duration)
    {
        if (!isZooming)
        {
            StartCoroutine(ZoomRoutine(targetPos, duration));
        }
    }

    private IEnumerator ZoomRoutine(Vector3 targetPos, float duration)
    {
        isZooming = true;
        float elapsed = 0f;
        float zoomSize = 2.0f; 

        // Khóa mục tiêu zoom (giữ nguyên Z = -10 để cùng phương)
        Vector3 focusPos = new Vector3(targetPos.x, targetPos.y, -10f);

        // 1. Giai đoạn Zoom In (defaultPos -> focusPos)
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / 0.5f); // Dùng SmoothStep cho mượt
            
            cam.orthographicSize = Mathf.Lerp(defaultSize, zoomSize, t);
            transform.position = Vector3.Lerp(defaultPos, focusPos, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // 3. Giai đoạn Zoom Out (focusPos -> defaultPos)
        // CỰC KỲ QUAN TRỌNG: Dùng focusPos làm điểm bắt đầu cố định để cùng phương
        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / 0.5f);
            
            cam.orthographicSize = Mathf.Lerp(zoomSize, defaultSize, t);
            // Bay thẳng từ vị trí quân cờ về vị trí bàn cờ mặc định
            transform.position = Vector3.Lerp(focusPos, defaultPos, t); 
            yield return null;
        }

        isZooming = false;
    }
}
