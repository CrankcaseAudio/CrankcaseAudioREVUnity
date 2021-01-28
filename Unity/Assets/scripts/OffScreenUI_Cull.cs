using UnityEngine;
using UnityEngine.UI;
using System.Linq;

//Igor Aherne 10/May/2017
//https://www.facebook.com/igor.aherne
//
//as soon as our rect no longer overlaps the reference rect
//we will disable our graphic (if supplied)  and  an optional GameObject (could be our child)
// https://forum.unity.com/threads/cull-ui-if-recttransform-goes-outside-viewport-screen-canvas.470226/
//Works even if recttransforms belong to different parents

public class OffScreenUI_Cull : MonoBehaviour
{
    [SerializeField] RectTransform _viewportRectangle;
    [SerializeField, Space(15)] RectTransform _ownRectTransform;


    protected virtual void Start()
    {
        if (_viewportRectangle == null)
        {
            _viewportRectangle = (GetComponentInParent(typeof(Canvas)) as Canvas).transform as RectTransform;
        }
        if (_ownRectTransform == null)
        {
            _ownRectTransform = transform as RectTransform;
        }
    }

    protected virtual void Update()
    {
        Cull();
    }

    protected void Cull()
    {
        if (_viewportRectangle == null || _ownRectTransform == null)
            return;

        bool overlaps = rectTransfOverlaps_inScreenSpace(_ownRectTransform, _viewportRectangle);

        OnCull(overlaps);
    }

    protected virtual void OnCull(bool visible) { }

    public static bool rectTransfOverlaps_inScreenSpace(RectTransform rectTrans1, RectTransform rectTrans2)
    {
        Rect rect1 = getScreenSpaceRect(rectTrans1);
        Rect rect2 = getScreenSpaceRect(rectTrans2);

        return rect1.Overlaps(rect2);
    }

    //rect transform into coordinates expressed as seen on the screen (in pixels)
    //takes into account RectTrasform pivots
    // based on answer by Tobias-Pott
    // http://answers.unity3d.com/questions/1013011/convert-recttransform-rect-to-screen-space.html
    public static Rect getScreenSpaceRect(RectTransform transform)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        Rect rect = new Rect(transform.position.x, Screen.height - transform.position.y, size.x, size.y);
        rect.x -= (transform.pivot.x * size.x);
        rect.y -= ((1.0f - transform.pivot.y) * size.y);
        return rect;
    }
}
