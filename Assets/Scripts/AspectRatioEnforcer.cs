using UnityEngine;

/// <summary>
/// Forces the camera to render at a fixed aspect ratio (default 16:9).
/// On wider screens, adds pillarbox (black bars on the sides).
/// On taller screens, adds letterbox (black bars on top/bottom).
/// Attach to the Main Camera.
/// </summary>
[RequireComponent(typeof(Camera))]
public class AspectRatioEnforcer : MonoBehaviour
{
    [SerializeField] private float targetAspectWidth  = 16f;
    [SerializeField] private float targetAspectHeight = 9f;

    private float _lastScreenWidth;
    private float _lastScreenHeight;
    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    private void Start()
    {
        ApplyAspect();
    }

    private void Update()
    {
        if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
            ApplyAspect();
    }

    private void ApplyAspect()
    {
        _lastScreenWidth  = Screen.width;
        _lastScreenHeight = Screen.height;

        float targetAspect  = targetAspectWidth / targetAspectHeight;
        float windowAspect  = (float)Screen.width / (float)Screen.height;
        float scaleHeight   = windowAspect / targetAspect;

        Rect rect = new Rect();

        if (scaleHeight < 1f)
        {
            // Tela mais alta que o alvo — letterbox (barras em cima e embaixo)
            rect.width  = 1f;
            rect.height = scaleHeight;
            rect.x      = 0f;
            rect.y      = (1f - scaleHeight) / 2f;
        }
        else
        {
            // Tela mais larga que o alvo — pillarbox (barras nas laterais, ultra-wide)
            float scaleWidth = 1f / scaleHeight;
            rect.width  = scaleWidth;
            rect.height = 1f;
            rect.x      = (1f - scaleWidth) / 2f;
            rect.y      = 0f;
        }

        _cam.rect = rect;
    }
}
