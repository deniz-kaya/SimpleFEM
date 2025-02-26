namespace SimpleFEM.Interfaces;

public interface IPopupWindow
{
    public bool isOpen { get;  }
    public void Show();
    public void Close();
}