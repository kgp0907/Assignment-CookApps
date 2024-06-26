using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptimizeCanvas : MonoBehaviour
{
    public CanvasScaler canvasScaler = null;
    private Rect safeArea = new Rect();
    private Vector2 minAnchor = new Vector2();
    private Vector2 maxAnchor = new Vector2();
    public RectTransform AnchoredObject;//캔버스 하위 전체사이즈 패널
    private bool isResolutioned = false;

    private void Awake()
    {
        if (canvasScaler == null)
        {
            canvasScaler = gameObject.GetComponent<CanvasScaler>();

        }
        canvasScaler.matchWidthOrHeight = 1f;
        isResolutioned = false;
        SetResolution();
        //Scene_Manager.Instance.RefreshSetLow();
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }

    int lastScreenWidth = 0;
    int lastScreenHeight = 0;

    private void LateUpdate()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {

            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            isResolutioned = false;
            SetResolution();
        }
    }

    private void SetResolution()
    {
        if (isResolutioned)
        {
            return;
        }

        if (canvasScaler == null)
        {
            canvasScaler = gameObject.GetComponent<CanvasScaler>();
        }

        safeArea = Screen.safeArea;
        minAnchor = safeArea.position;
        maxAnchor = safeArea.position + safeArea.size;

        minAnchor.x /= Screen.width;
        minAnchor.y /= Screen.height;
        maxAnchor.x /= Screen.width;
        maxAnchor.y /= Screen.height;

        AnchoredObject.anchorMin = minAnchor;
        AnchoredObject.anchorMax = maxAnchor;

        float ratio = Screen.height / (float)Screen.width;
        canvasScaler.matchWidthOrHeight = (ratio >= 1.77f) ? 0.0f : 1f;

#if UNITY_EDITOR
        isResolutioned = false;
#else
    isResolutioned = true;
#endif

    }
}
