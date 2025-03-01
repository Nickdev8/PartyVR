using System;
using System.Collections;
using UnityEngine;
using SaintsField;

public class ImageRenderer : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public CustomImage errorImage;
    public CustomImage[] images;
    public GameObject background;

    // this was for checking if it is the same as the current displaing image
    // private int _displayingIndex;
    // private bool _displayingBlinking;
    // private int _displayingBlinkFrom;
    // private int _displayingBlinkTo;

    private void Start()
    {
        ClearImage();
    }

    public void ClearImage()
    {
        StopAllCoroutines();
        background.SetActive(false);
        spriteRenderer.sprite = null;
    }
    public void BlinkImage(int from, int to, float time = Mathf.Infinity,  float interval = 0.3f)
    {
        // this was for checking if it is the same as the current displaing image
        // Update the blink range.
        // if (_displayingBlinking)
        // {
        //     if (_displayingBlinkFrom == from && _displayingBlinkTo == to)
        //     {
        //         RenderImage(-1, true);
        //         return;
        //     }
        // }
        // _displayingBlinking = true;
        
        StopAllCoroutines();
        StartCoroutine(Blink(from, to, time, interval));
    }

    public void ShowImage(int index)
    {
        // this was for checking if it is the same as the current displaing image
        // if (index == _displayingIndex && _displayingBlinking)
        // {
        //     RenderImage(-1, true);
        //     return;
        // }
        
        StopAllCoroutines();
        if (images[index] == null)
        {
            if (index < 0)
                RenderImage(index, true);
            else
                ClearImage();
        }
        else
        {
            RenderImage(index);
        }
    }

    private IEnumerator Blink(int from, int to, float time, float interval)
    {
        int currentImageIndex = from;
        
        if (images[from] == null || images[to] == null)
        {
            RenderImage(0, true);
            yield break;
        }
        
        while (true)
        {
            RenderImage(currentImageIndex);
            
            currentImageIndex++;
            if (currentImageIndex > to)
                currentImageIndex = from;
            
            yield return new WaitForSeconds(interval);
            time -= interval;
            
            if (time <= 0)
            {
                ClearImage();
                yield break;
            }
        }
    }

    private void RenderImage(int index, bool error = false)
    {
        background.SetActive(true);
        
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

            Transform imageTransform = spriteRenderer.gameObject.transform;
            spriteRenderer.gameObject.transform.localScale = new Vector3(images[index].size.x * 0.08f, images[index].size.y * 0.08f, 0.08f);
        }
        else
        {
            spriteRenderer.sprite = errorImage.sprite;
            spriteRenderer.color = errorImage.color;

            spriteRenderer.gameObject.transform.localScale = new Vector3(errorImage.size.x * 0.08f, errorImage.size.y * 0.08f, 0.08f);
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
