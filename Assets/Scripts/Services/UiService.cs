using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Views;

[CreateAssetMenu(fileName = "UiService", menuName = "UiService")]
public class UiService : ScriptableObject
{
    public List<UiView> Views;
    
    private VisualElement _rootVisualElement;
    
    public VisualElement UiRoot => _rootVisualElement;

    private UiView _openView;

    public void Initialize(VisualElement rootVisualElement, List<UiView> views)
    {
        Views = views;
        _rootVisualElement = rootVisualElement;
        foreach (var view in Views)
        {
            view.Initialize();
        }
    }
    
    public UiView FindView(string name)
    {
        return Views.FirstOrDefault(x => x.name == name);
    }

    public void OpenView(string name, object data = null)
    {
        var view = FindView(name);
        if (view == null)
        {
            throw new Exception($"Could not find view '{name}'.");
        }
        OpenView(view, data);
    }

    public void OpenView(UiView openView, object data = null)
    {
        foreach (var view in Views)
        {
            view.Close();
        }

        _openView = openView;
        if (_openView == null)
        {
            return;
        }
        _openView.Open(data);
    }

    public void Refresh()
    {
        OpenView(_openView);
    }
}