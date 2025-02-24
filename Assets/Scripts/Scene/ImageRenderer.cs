using System.Collections;
using UnityEngine;
using SaintsField;

public class ImageRenderer : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public CustomImage errorImage;
    public CustomImage[] images;

    private int _displayingIndex;
    private bool _displayingBlinking;
    private int _displayingBlinkFrom;
    private int _displayingBlinkTo;

    public void ClearImage()
    {
        StopAllCoroutines();
        spriteRenderer.sprite = null;
    }
    public void BlinkImage(int from, int to, float interval = 0.3f)
    {
        // Update the blink range.
        if (_displayingBlinking)
        {
            if (_displayingBlinkFrom == from && _displayingBlinkTo == to)
            {
                RenderImage(-1, true);
                return;
            }
        }
        _displayingBlinking = true;
        
        StopAllCoroutines();
        _displayingBlinkFrom = from;
        _displayingBlinkTo = to;
        StartCoroutine(Blink(from, to, interval));
    }

    public void ShowImage(int index)
    {
        if (index == _displayingIndex && _displayingBlinking)
        {
            RenderImage(-1, true);
            return;
        }
        
        StopAllCoroutines();
        RenderImage(index);
        _displayingBlinking = false;
    }

    private IEnumerator Blink(int from, int to, float interval)
    {
        int currentImageIndex = from;
        
        while (true)
        {
            if (currentImageIndex != _displayingIndex)
                RenderImage(currentImageIndex);
            
            currentImageIndex++;
            if (currentImageIndex > to)
                currentImageIndex = from;
            
            yield return new WaitForSeconds(interval);
        }
    }

    private void RenderImage(int index, bool error = false)
    {
        if (!error)
        {
            // Ensure the index is valid.
            if (index < 0 || index >= images.Length)
            {
                Debug.LogWarning("Index out of range!");
                return;
            }

            spriteRenderer.sprite = images[index].sprite;
            spriteRenderer.color = images[index].color;

            // Use transform.localScale if you don't have a RectTransform.
            // If you really need RectTransform (for UI), ensure your GameObject has one.
            Transform imageTransform = spriteRenderer.gameObject.transform;
            spriteRenderer.gameObject.transform.localScale = new Vector3(images[index].size.x * 0.08f, images[index].size.y * 0.08f, 0.08f);

            _displayingIndex = index;
        }
        else
        {
            spriteRenderer.sprite = errorImage.sprite;
            spriteRenderer.color = errorImage.color;

            // Use transform.localScale if you don't have a RectTransform.
            // If you really need RectTransform (for UI), ensure your GameObject has one.
            spriteRenderer.gameObject.transform.localScale = new Vector3(errorImage.size.x * 0.08f, errorImage.size.y * 0.08f, 0.08f);

            _displayingIndex = -1;
        }
    }
}

[System.Serializable]
public class CustomImage
{
    public Sprite sprite = null;
    public Vector2 size = new Vector2(1, 1);
    public Color color = Color.white;
}
