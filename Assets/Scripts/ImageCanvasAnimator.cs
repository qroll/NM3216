using UnityEngine;
using UnityEngine.UI;

public class ImageCanvasAnimator : MonoBehaviour
{

    // set the controller you want to use in the inspector
    public RuntimeAnimatorController controller;

    // the UI/Image component
    Image imageCanvas;
    // the fake SpriteRenderer
    SpriteRenderer fakeRenderer;
    // the Animator
    Animator animator;


    void Start()
    {
        CDebug.SetDebugLoggingLevel((int)CDebug.EDebugLevel.DEBUG);
        imageCanvas = GetComponent<Image>();
        fakeRenderer = gameObject.AddComponent<SpriteRenderer>();
        // avoid the SpriteRenderer to be rendered
        fakeRenderer.enabled = false;
        animator = gameObject.AddComponent<Animator>();

        // set the controller
        animator.runtimeAnimatorController = controller;
    }

    void Update()
    {
        // if a controller is running, set the sprite
        if (animator.runtimeAnimatorController)
        {
            imageCanvas.sprite = fakeRenderer.sprite;
        }
    }

}