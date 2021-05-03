using UnityEngine;

public class PaintApp : MonoBehaviour
{
    public UiService UiService; 
        
    public static PaintApp Instance;

    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
        UiService.OpenView("BookView");
    }
}