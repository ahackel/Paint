using UnityEngine;
using UnityEngine.UI;

public class BrushSizePalette : DropdownPalette<float>
{
    protected override void SetupItem(Button item, int index)
    {
        SetPreviewSize(item.gameObject, index);
    }

    private void SetPreviewSize(GameObject item, int index)
    {
        var preview = item.GetComponentInChildren<BrushSizePreview>();
        if (preview == null)
        {
            return;
        }

        preview.Size = Options[index];
    }

    protected override void RefreshShownValue()
    {
        SetPreviewSize(CaptionImage.gameObject, SelectedIndex);
    }
}
