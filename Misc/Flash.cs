using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Flash : Singleton<Flash>
{
    //闪屏全亮效果持续时间
    public float duration = 0.1f;
    //渐隐消失的时间
    public float fadeDuration = 0.5f;
    //UI 层
    protected Image image;
    
    protected virtual void Start()
    {
        image = GetComponent<Image>();
    }
    public void Trigger() => Trigger(duration, fadeDuration);
    
    public void Trigger(float duration, float fadeDuration)
    {
        StopAllCoroutines();
        StartCoroutine(Routine(duration, fadeDuration));
    }
    protected IEnumerator Routine(float duration, float fadeDuration)
    {
        var elapsedTime = 0f;
        var color = image.color;

        color.a = 1;
        image.color = color;

        yield return new WaitForSeconds(duration);

        while (elapsedTime < fadeDuration)
        {
            color.a = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            image.color = color;

            yield return null;
        }

        color.a = 0;
        image.color = color;
    }
}