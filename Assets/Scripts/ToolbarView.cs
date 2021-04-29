using Tools;
using UnityEngine;

public class ToolbarView : MonoBehaviour
{
    public PaintView PaintView;
    
    public ToolButton PenButton;
    public ToolButton EraserButton;

    public Tool PenTool;
    public Tool EraserTool;
    private void Start()
    {
        UpdateSelection();
        PenButton.onClick.AddListener(() => SetTool(PenTool));
        EraserButton.onClick.AddListener(() => SetTool(EraserTool));
        EraserButton.OnDoubleClick.AddListener(() => PaintView.Clear());
    }

    private void SetTool(Tool tool)
    {
        PaintView.CurrentTool = tool;
        UpdateSelection();
    }

    private void UpdateSelection()
    {
        PenButton.SetChecked(PaintView.CurrentTool == PenTool);
        EraserButton.SetChecked(PaintView.CurrentTool == EraserTool);
    }
}
