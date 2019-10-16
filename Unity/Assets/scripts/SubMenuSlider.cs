using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubMenuSlider : MonoBehaviour
{
    public float animationDuration = 0.5f;

    bool isShown = false;
    float size { get { return (this.transform as RectTransform).sizeDelta.y; } }

    private void Awake()
    {
        SetPosY(-size);
    }

    public void ToggleSlide()
    {
        if(Application.isMobilePlatform)
        {
            GameObject.FindObjectOfType<MainUGUI>().ShowPopUp(true);
            return;
        }

        StopAllCoroutines();
        StartCoroutine(SlideIE(!isShown));
    }

    private IEnumerator SlideIE(bool on)
    {
        float timeStarted = Time.time;
        while (true)
        {
            float lerp = (Time.time - timeStarted) / animationDuration;

            float newY = 0;
            if (on)
                newY = Mathf.Lerp(-size, 0, lerp);
            else
                newY = Mathf.Lerp(0, -size, lerp);

            SetPosY(-newY);

            if (lerp >= 1f)
                break;
            yield return null;
        }
        isShown = on;
    }

    private void SetPosX(float x)
    {
        this.transform.localPosition = new Vector3(x, this.transform.localPosition.y, this.transform.localPosition.z);
    }

    private void SetPosY(float y)
    {
        this.transform.localPosition = new Vector3(this.transform.localPosition.x, y, this.transform.localPosition.z);
    }
}
