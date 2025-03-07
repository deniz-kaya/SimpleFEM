using ImGuiNET;

namespace SimpleFEM.Types.Settings;

public struct HotkeySettings
{
    public float FooterHeight;
    public ImGuiKey MoveToolKey;
    public ImGuiKey AddNodeToolKey;
    public ImGuiKey AddElementToolKey;
    public ImGuiKey SelectNodesToolKey;
    public ImGuiKey SelectElementsToolKey;
    public ImGuiKey MouseSelectToolKey;
    

    public static HotkeySettings Default
    {
        get
        {
            return new HotkeySettings()
            {
                FooterHeight = 30f,
                MoveToolKey = ImGuiKey.M,
                AddNodeToolKey = ImGuiKey.Q,
                AddElementToolKey = ImGuiKey.W,
                SelectNodesToolKey = ImGuiKey.E,
                SelectElementsToolKey = ImGuiKey.R,
                MouseSelectToolKey = ImGuiKey.T
            };
        }
    }
}