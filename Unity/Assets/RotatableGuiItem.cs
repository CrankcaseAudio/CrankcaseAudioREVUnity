using UnityEngine;

[ExecuteInEditMode()]
public class RotatableGuiItem : MonoBehaviour
{
    public RotatableGuiItem()
    {

    }
    public Texture2D texture = null;
    public float angle = 0;
    public Vector2 size = new Vector2 (128, 128);

//this will overwrite the items position
    public AlignmentScreenpoint ScreenpointToAlign = AlignmentScreenpoint.TopLeft;
    public Vector2 relativePosition = new Vector2 (0, 0);
    public Vector2 centreOffset = new Vector2 (0, 0);

    protected Vector2 pos = new Vector2 (0, 0);
    protected Rect rect;
    protected Vector2 pivot;


    void Start ()
    {



        UpdateSettings ();
    }

    void UpdateSettings ()
    {
        Vector2 cornerPos = new Vector2 (0, 0);

        //overwrite the items position
        switch (ScreenpointToAlign)
        {
        case AlignmentScreenpoint.TopLeft:
            cornerPos = new Vector2 (0, 0);
            break;
        case AlignmentScreenpoint.TopMiddle:
            cornerPos = new Vector2 (Screen.width / 2, 0);
            break;
        case AlignmentScreenpoint.TopRight:
            cornerPos = new Vector2 (Screen.width, 0);
            break;
        case AlignmentScreenpoint.LeftMiddle:
            cornerPos = new Vector2 (0, Screen.height / 2);
            break;
        case AlignmentScreenpoint.RightMiddle:
            cornerPos = new Vector2 (Screen.width, Screen.height / 2);
            break;
        case AlignmentScreenpoint.BottomLeft:
            cornerPos = new Vector2 (0, Screen.height);
            break;
        case AlignmentScreenpoint.BottomMiddle:
            cornerPos = new Vector2 (Screen.width / 2, Screen.height);
            break;
        case AlignmentScreenpoint.BottomRight:
            cornerPos = new Vector2 (Screen.width, Screen.height);
            break;
        default:
            break;
        }

        float screenScaleX = (float)(Screen.width / 320.0);
        float screenScaleY = (float)(Screen.height / 480.0);

        float screenScale = Mathf.Min(screenScaleX,screenScaleY);
        Vector2 scaleVector = new Vector2 (screenScale, screenScale);

        Vector2 centreOffset2 = centreOffset;
        Vector2 relativePosition2 = relativePosition;
        Vector2 size2 = size;

        relativePosition2.Scale(scaleVector);
        size2.Scale(scaleVector);
        centreOffset2.Scale(scaleVector);

        pos = cornerPos + relativePosition2;//- centreOffset;
        rect = new Rect (pos.x - size2.x * 0.5f, pos.y - size2.y * 0.5f, size2.x, size2.y);
        //pivot = new Vector2 (rect.xMin + rect.width * 0.5f, rect.yMin + rect.height * 0.5f);
        pivot = new Vector2 (rect.xMin + centreOffset2.x, rect.yMin + centreOffset2.y);
    }

    void OnGUI ()
    {
        UpdateSettings ();

        Matrix4x4 matrixBackup = GUI.matrix;

        GUIUtility.RotateAroundPivot (angle, pivot);
        GUI.DrawTexture (rect, texture);
        GUI.matrix = matrixBackup;
    }

    public enum AlignmentScreenpoint
    {
        TopLeft,
        TopMiddle,
        TopRight,
        LeftMiddle,
        RightMiddle,
        BottomLeft,
        BottomMiddle,
        BottomRight
    }


}