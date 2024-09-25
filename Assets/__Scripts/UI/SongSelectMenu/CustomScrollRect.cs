using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Introduces smooth mouse wheel scrolling and disables viewport drag scrolling
public class CustomScrollRect : ScrollRect
{
    //You won't see those in inspector, since ScrollRect draws its own inspector, but you can access those in Debug Mode
    public bool SmoothScrolling = true; 
    public float SmoothScrollTime = 0.05f;
    public float ScrollSensitivity = 100;

    private bool damping;
    private Vector2 ghostNormalizedPos = new Vector2(0, 1);     //To which target Update smoothly moves the scroll
    private Vector2 vel;    //Needed by SmoothDamp

    void Update()
    {
        if (!SmoothScrolling) return;
        if (!damping) return;

        //Separately damping coordinates so the second coordinate doesn't get stuck damping for whatever reason
        float dampX = !vertical ? Mathf.SmoothDamp(normalizedPosition.x, ghostNormalizedPos.x, ref vel.x, SmoothScrollTime) : normalizedPosition.x;
        float dampY = vertical ? Mathf.SmoothDamp(normalizedPosition.y, ghostNormalizedPos.y, ref vel.y, SmoothScrollTime) : normalizedPosition.y;

        normalizedPosition = new Vector2(dampX, dampY);
        if (vel.magnitude < 0.008) damping = false;
        //Without disabling damping, in periods of time when we don't scroll with mouse, scroll bars would be broken and scroll back to 'ghostNormalizedPos' every time
    }

    //TIP There was some BS where it would not stop damping at close-to-0 vel.y, since for some reason vel.x would have value that doesn't come to 0

    public override void OnScroll(PointerEventData data)
    {
        if (!IsActive())
            return;
        
        if (SmoothScrolling)
        {
            //======= OLD IMPLEMENTATION ========
            ////This basically executes normal base.OnScroll, but doesn't modify any actual values, only stores them to then reproduce those in smooth fashion
            //Vector2 positionBefore = normalizedPosition;
            //base.OnScroll(data);
            //Vector2 positionAfter = normalizedPosition;
            //normalizedPosition = positionBefore;
            //ghostNormalizedPos = positionAfter;
            //damping = true;

            //Separately calculating coordinates so the second coordinate doesn't get stuck damping for whatever reason
            //Since 'normalizedPosition is always [0,1] regardless of scroll height, we need to calculate relative addition to ghostPos from the height of scroll rect
            //ContentBounds is actual content size, viewRect is one we see. Need to subtract, since scroll area is actually the one that is not visible

            //For deltaX we talk both X and Y scroll, since X scroll is tilting the wheel (rare mouse feature) and Y scroll is normal up-down scroll, 
            // which we also take negative, since IMO scrolling the mouse down should scroll the rect to the right ("further" like up->down and left->right)
            float deltaX = !vertical ? (data.scrollDelta.x - data.scrollDelta.y) / (m_ContentBounds.size.x - viewRect.rect.size.x) * ScrollSensitivity : 0;
            float deltaY = vertical ? data.scrollDelta.y / (m_ContentBounds.size.y - viewRect.rect.size.y) * ScrollSensitivity : 0;

            Vector2 effectiveDelta = new Vector2(deltaX, deltaY);
            ghostNormalizedPos += effectiveDelta;

            float xClamp = Mathf.Clamp(ghostNormalizedPos.x, 0, 1);
            float yClamp = Mathf.Clamp(ghostNormalizedPos.y, 0, 1);
            ghostNormalizedPos = new Vector2(xClamp, yClamp);
            damping = true;
        }
        else
        {
            base.OnScroll(data);
        }
    }

    /// <summary> Sets the scroll rect to top position (only for vertical scrollRect) </summary>
    public void ResetToTopVertical()
    {
        ghostNormalizedPos = normalizedPosition = new Vector2(normalizedPosition.x, 1);
    }

    /// <summary> Sets the scroll rect to bot position (only for vertical scrollRect) </summary>
    public void ResetToBotVertical()
    {
        ghostNormalizedPos = normalizedPosition = new Vector2(normalizedPosition.x, 0);
    }

    /// <summary> Sets the scroll rect to left position (only for horizontal scrollRect) </summary>
    public void ResetToLeftHorizontal()
    {
        ghostNormalizedPos = normalizedPosition = new Vector2(0, normalizedPosition.y);
    }

    /// <summary> Resets the ghost position, in case when it remains in some other position than current, and without calling this, scrolling would damp it to this position</summary>
    public void ResetGhostToCurrent()
    {
        ghostNormalizedPos = normalizedPosition;
    }

    /// <summary> Needed when you used ScrollToVertical/Horizontal and you don't need to animate the scroll, but instead immediately set the scroll state </summary>
    public void ResetCurrentToGhost()
    {
        normalizedPosition = ghostNormalizedPos;
    }

    /// <summary> Smoothly scrolls to specified normalizedPos (only for vertical scrollRect). 1 is top, 0 is bottom. </summary>
    public void ScrollToVertical(float normalizedPos)
    {
        damping = true;
        normalizedPos = Mathf.Clamp(normalizedPos, 0, 1);
        ghostNormalizedPos = new Vector2(ghostNormalizedPos.x, normalizedPos);
    }

    /// <summary> Smoothly scrolls to specified normalizedPos (only for horizontal scrollRect). 0 is left, 1 is right. </summary>
    public void ScrollToHorizontal(float normalizedPos)
    {
        damping = true;
        normalizedPos = Mathf.Clamp(normalizedPos, 0, 1);
        ghostNormalizedPos = new Vector2(normalizedPos, ghostNormalizedPos.y);
    }

    public override void OnDrag(PointerEventData data)
    {
        //This being empty disables viewport drag
    }

    //Further stuff makes so scrolling the actual scrollbar with mouse drag sets ghost too, because otherwise ghost would remain in original position
    protected override void OnEnable()
    {
        base.OnEnable();
        if (vertical) verticalScrollbar.onValueChanged.AddListener(SetGhostOnScrollbarDrag);
        else horizontalScrollbar.onValueChanged.AddListener(SetGhostOnScrollbarDrag);
    }

    protected override void OnDisable()
    {
        base.OnDestroy();
        if (vertical) verticalScrollbar.onValueChanged.RemoveListener(SetGhostOnScrollbarDrag);
        else horizontalScrollbar.onValueChanged.RemoveListener(SetGhostOnScrollbarDrag);
    }

    private void SetGhostOnScrollbarDrag(float pos)
    {
        if (!damping) ResetGhostToCurrent();    //onValueChanged procs also when we are actually wheel-scrolling AND during damping, so only resetting it when not damping
    }
}
