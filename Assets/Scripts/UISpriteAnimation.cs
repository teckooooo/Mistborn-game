using UnityEngine;
using UnityEngine.UI;

public class UISpriteAnimation : MonoBehaviour
{
    public Sprite[] frames;
    public float fps = 12f;

    private Image  image;
    private float  timer;
    private int    currentFrame;

    void Start()
    {
        image = GetComponent<Image>();
        if (frames.Length > 0) image.sprite = frames[0];
    }

    void Update()
    {
        if (frames.Length == 0) return;

        timer += Time.unscaledDeltaTime; // funciona aunque el juego esté pausado
        if (timer >= 1f / fps)
        {
            timer = 0f;
            currentFrame = (currentFrame + 1) % frames.Length;
            image.sprite = frames[currentFrame];
        }
    }
}
