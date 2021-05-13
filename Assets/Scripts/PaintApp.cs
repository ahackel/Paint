using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Views;

public class PaintApp : MonoBehaviour
{
    public UiService UiService;
    public List<UiView> Views;
    public UIDocument UIDocument;
    
        
    public static PaintApp Instance;

    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
        UiService.Initialize(UIDocument.rootVisualElement, Views);
    }


    private void Start()
    {
        UiService.OpenView("BookView");
    }
}