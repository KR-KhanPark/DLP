using UnityEngine;
using UnityEngine.InputSystem;

public class LineAimVisualizer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LineRenderer radiusRenderer;
    [SerializeField] private Transform startPointVisual;
    [SerializeField] private Transform aimOrigin;

    [Header("Radius")]
    [SerializeField] private float aimRadius = 4f;
    [SerializeField] private int circleSegments = 64;

    [Header("Plane")]
    [SerializeField] private float worldZ = 0f;

    [Header("Preview")]
    [SerializeField] private LineRenderer previewLine;

    [Header("Slow Motion")]
    [SerializeField, Range(0.01f, 1f)] private float slowMotionScale = 0.2f;

    [Header("Line Creation")]
    [SerializeField] private GameObject lineSegmentPrefab;

    [Header("Generated Line Layers")]
    [SerializeField] private string groundLayerName = "Ground";
    [SerializeField] private string wallLayerName = "Wall";
    
    [Header("Preview Colors")]
    [SerializeField] private Color validPreviewColor = Color.white;
    [SerializeField] private Color invalidPreviewColor = Color.red;

    public Vector3 CurrentStartPointWorld { get; private set; }

    private bool isDragging;
    private bool isCurrentDragValid = true;
    private Vector3 dragStartPoint;

    private GameObject currentLine;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        DrawRadiusCircle();

        if (previewLine != null)
        {
            previewLine.positionCount = 2;
            previewLine.useWorldSpace = true;
            previewLine.enabled = false;
            SetPreviewLineColor(validPreviewColor);
        }
    }

    private void OnValidate()
    {
        if (circleSegments < 8)
            circleSegments = 8;

        if (radiusRenderer != null)
            DrawRadiusCircle();
    }

    private void Update()
    {
        UpdateRadiusVisualPosition();
        UpdateStartPointVisual();
        HandleInput();
    }

    private void DrawRadiusCircle()
    {
        if (radiusRenderer == null)
            return;

        radiusRenderer.loop = true;
        radiusRenderer.useWorldSpace = false;
        radiusRenderer.positionCount = circleSegments;

        float angleStep = 360f / circleSegments;

        for (int i = 0; i < circleSegments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * aimRadius;
            float y = Mathf.Sin(angle) * aimRadius;

            radiusRenderer.SetPosition(i, new Vector3(x, y, 0f));
        }
    }

    private void UpdateRadiusVisualPosition()
    {
        if (radiusRenderer == null || aimOrigin == null)
            return;

        // 반경 원은 항상 현재 플레이어(Dot)를 따라감
        radiusRenderer.transform.position = aimOrigin.position;
    }

    private void UpdateStartPointVisual()
    {
        if (mainCamera == null || startPointVisual == null || aimOrigin == null)
            return;

        if (Mouse.current == null)
            return;

        Vector2 mouseScreen2D = Mouse.current.position.ReadValue();
        Vector3 mouseScreen = new Vector3(mouseScreen2D.x, mouseScreen2D.y, 0f);

        float distanceToPlane = worldZ - mainCamera.transform.position.z;
        mouseScreen.z = distanceToPlane;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = worldZ;

        Vector3 center = aimOrigin.position;
        Vector3 offset = mouseWorld - center;
        offset.z = 0f;

        if (offset.magnitude > aimRadius)
            offset = offset.normalized * aimRadius;

        Vector3 clampedPoint = center + offset;
        clampedPoint.z = worldZ;

        CurrentStartPointWorld = clampedPoint;

        if (isDragging)
        {
            // 드래그 중에는 시작점 시각표시를 월드 기준으로 고정
            startPointVisual.position = dragStartPoint;
        }
        else
        {
            // 평소에는 마우스를 따라다님
            startPointVisual.position = clampedPoint;
        }
    }

    private void HandleInput()
    {
        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartDrag();
        }

        if (isDragging && Mouse.current.leftButton.isPressed)
        {
            UpdateDrag();
        }

        if (isDragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            EndDrag();
        }
    }

    private void StartDrag()
    {
        isDragging = true;
        isCurrentDragValid = true;
        dragStartPoint = CurrentStartPointWorld;

        if (startPointVisual != null)
        {
            startPointVisual.position = dragStartPoint;
        }

        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        if (previewLine != null)
        {
            previewLine.positionCount = 2;
            previewLine.enabled = true;
            previewLine.SetPosition(0, dragStartPoint);
            previewLine.SetPosition(1, dragStartPoint);
        }

        SetPreviewLineColor(validPreviewColor);
    }

    private void UpdateDrag()
    {
        if (previewLine == null || aimOrigin == null)
            return;

        isCurrentDragValid = IsDragStillValid();

        Vector3 endPoint = CurrentStartPointWorld;

        previewLine.SetPosition(0, dragStartPoint);
        previewLine.SetPosition(1, endPoint);

        if (isCurrentDragValid)
        {
            SetPreviewLineColor(validPreviewColor);
        }
        else
        {
            SetPreviewLineColor(invalidPreviewColor);
        }
    }

    private void EndDrag()
    {
        // 다음 단계에서 실제 생성 로직 추가 예정
        // 지금은 유효할 때만 "생성 가능 상태"였다고 간주
        if (isCurrentDragValid)
        {
            CreateLineSegment(dragStartPoint, CurrentStartPointWorld);
        }
        else
        {
            Debug.Log("Line preview invalid: start point moved out of current radius, no line created.");
        }

        isDragging = false;
        isCurrentDragValid = true;

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (previewLine != null)
        {
            previewLine.enabled = false;
        }

        if (startPointVisual != null)
        {
            startPointVisual.position = CurrentStartPointWorld;
        }
    }

    private bool IsDragStillValid()
    {
        if (aimOrigin == null)
            return false;

        Vector3 currentCenter = aimOrigin.position;
        Vector3 toStart = dragStartPoint - currentCenter;
        toStart.z = 0f;

        return toStart.magnitude <= aimRadius;
    }

    private void OnDisable()
    {
        isDragging = false;
        isCurrentDragValid = true;

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (previewLine != null)
        {
            previewLine.enabled = false;
        }
    }
    private void CreateLineSegment(Vector3 start, Vector3 end)
    {
        if (lineSegmentPrefab == null)
            return;

        // 기존 선 제거
        if (currentLine != null)
        {
            Destroy(currentLine);
        }

        Vector3 direction = end - start;
        float length = direction.magnitude;

        if (length <= 0.01f)
            return;

        Vector3 normalizedDirection = direction.normalized;
        Vector3 midPoint = (start + end) * 0.5f;

        currentLine = Instantiate(lineSegmentPrefab, midPoint, Quaternion.identity);
        GameObject line = currentLine;

        // X축이 선 방향을 바라보게
        line.transform.right = normalizedDirection;

        // 길이는 X축으로 늘림, 두께/깊이는 유지
        Vector3 scale = line.transform.localScale;
        scale.x = length;
        line.transform.localScale = scale;

        ApplyGeneratedLineLayer(line, normalizedDirection);
    }
    private void ApplyGeneratedLineLayer(GameObject line, Vector3 normalizedDirection)
    {
        bool isGroundLike = Mathf.Abs(normalizedDirection.x) >= Mathf.Abs(normalizedDirection.y);

        string targetLayerName = isGroundLike ? groundLayerName : wallLayerName;
        int targetLayer = LayerMask.NameToLayer(targetLayerName);

        if (targetLayer == -1)
        {
            return;
        }

        SetLayerRecursively(line, targetLayer);
    }
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
    private void SetPreviewLineColor(Color color)
    {
        if (previewLine == null)
            return;

        previewLine.startColor = color;
        previewLine.endColor = color;
    }
}