using System.Windows.Forms;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace Automate;

public class AutomateSettings : ISettings
{
    public ToggleNode Enable { get; set; } = new ToggleNode(false);
    public ToggleNode Stashie { get; set; } = new(false);
    public ToggleNode MyLittleCrafter { get; set; } = new(false);
    public RangeNode<int> LoadingTimeout { get; set; } = new(10, 1, 60);
    public RangeNode<int> OtherHideoutTimeout { get; set; } = new(10, 1, 60);
    public HotkeyNodeV2 HideoutKey { get; set; } = new(Keys.F5);
    public HotkeyNodeV2 LogoutKey { get; set; } = new(Keys.BrowserBack);
    public ToggleNode Debug { get; set; } = new();
}