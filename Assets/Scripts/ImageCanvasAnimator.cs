using UnityEngine;
using UnityEngine.UI;

/*
 * Reuse sprite animation for animated image in UI canvas
 * Credit: Boia Games
 * http://boiagames.blogspot.sg/2015/08/reusing-your-unity-in-game-animations.html
 */

public class ImageCanvasAnimator : MonoBehaviour
{

    [Tooltip("The animator controller to be used")]
    public RuntimeAnimatorController controller;

    Image imageCanvas;
    SpriteRenderer fakeRenderer;
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
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
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
