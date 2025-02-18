using ImGuiNET;

namespace SimpleFEM.Types;

public struct UserSettings
{
    public float FooterHeight;
    public ImGuiKey MoveToolKey;
    public ImGuiKey AddNodeToolKey;
    public ImGuiKey AddElementToolKey;
    public ImGuiKey SelectNodesToolKey;
    public ImGuiKey SelectElementsToolKey;

    public static UserSettings Default
    {
        get
        {
            return new UserSettings()
            {
                FooterHeight = 30f,
                MoveToolKey = ImGuiKey.M,
                AddNodeToolKey = ImGuiKey.Q,
                AddElementToolKey = ImGuiKey.W,
                SelectNodesToolKey = ImGuiKey.E,
                SelectElementsToolKey = ImGuiKey.R,
            };
        }
    }
}