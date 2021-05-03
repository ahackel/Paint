using UnityEngine;

namespace Views
{
    public abstract class UiView : MonoBehaviour
    {
        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void OpenView(string name) => PaintApp.Instance.UiService.OpenView(name);
    }
}