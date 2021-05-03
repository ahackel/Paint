using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Views;

public class UiService : MonoBehaviour
{
    public List<UiView> Views;

    public UiView FindView(string name)
    {
        return Views.FirstOrDefault(x => x.name == name);
    }

    public void OpenView(string name)
    {
        foreach (var view in Views)
        {
            view.Close();
        }

        FindView(name).Open();
    }
}