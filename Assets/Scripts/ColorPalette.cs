using UnityEngine;
using UnityEngine.UI;

public class ColorPalette : DropdownPalette<Color>
{
    protected override void SetupItem(Button item, int index)
    {
        item.GetComponent<Image>().color = Options[index];
    }

    protected override void RefreshShownValue()
    {
        CaptionImage.color = SelectedValue;
    }
}
