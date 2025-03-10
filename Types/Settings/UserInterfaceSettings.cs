using ImGuiNET;

namespace SimpleFEM.Types.Settings;

public struct UserInterfaceSettings
{
    public float FooterHeight;
    public ImGuiKey AddNodeToolKey;
    public ImGuiKey AddElementToolKey;
    public ImGuiKey SelectNodesToolKey;
    public ImGuiKey SelectElementsToolKey;
    public ImGuiKey MouseSelectToolKey;

    public static UserInterfaceSettings Default =>
        new UserInterfaceSettings()
        {
            FooterHeight = 30f,
            AddNodeToolKey = ImGuiKey.Q,
            AddElementToolKey = ImGuiKey.W,
            SelectNodesToolKey = ImGuiKey.E,
            SelectElementsToolKey = ImGuiKey.R,
            MouseSelectToolKey = ImGuiKey.T
        };
}