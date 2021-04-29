using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToolButton : Button
{
    public Image ContentImage;
    public Color ContentNormalColor;
    public Color ContentCheckedColor;
    public Color CheckedColor;

    public bool IsChecked;
    public ButtonClickedEvent OnDoubleClick { get; } = new ButtonClickedEvent();

    public void SetChecked(bool isChecked)
    {
        IsChecked = isChecked;
        DoStateTransition(SelectionState.Normal, false);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left &&
            eventData.clickCount > 1)
        {
            OnDoubleClick.Invoke();
            return;
        }

        base.OnPointerClick(eventData);
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        if (IsChecked)
        {
            if (image != null)
            {
                image.CrossFadeColor(CheckedColor, 0, true, true);
            }
            if (ContentImage)
            {
                ContentImage.color = ContentCheckedColor;
            }
            return;
        }

        if (ContentImage)
        {
            ContentImage.color = ContentNormalColor;
        }

        base.DoStateTransition(state, instant);
    }
}