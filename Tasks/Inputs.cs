using System;
using System.Windows.Forms;
using ExileCore.Shared;
using System.Threading.Tasks;
using ExileCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Numerics;

namespace Automate.Tasks;

public class Inputs()
{
    private readonly Automate Instance = Automate.Instance;
    private readonly Random _random = new();
    private readonly IEnumerable<Keys> _keysToRelease =
    [
        Keys.LButton,
        Keys.RButton,
        Keys.LControlKey,
        Keys.LShiftKey,
        Keys.Control,
        Keys.ShiftKey,
        Keys.Shift,
        Keys.Left,
        Keys.Right,
    ];

    private int DownUpDelay => _random.Next(25, 65);

    // This task is NOT cancellable by design. It does not accept a token.
    public async SyncTask<bool> Logout()
    {
        ReleaseHeldKeys(); // Calls the synchronous version
        await SendKey(Instance.Settings.LogoutKey.Value.Key);
        return true;
    }

    // This task is also NOT cancellable by design.
    public async SyncTask<bool> TeleportToHideout()
    {
        ReleaseHeldKeys(); // Calls the synchronous version
        await SendKey(Instance.Settings.HideoutKey.Value.Key);
        return true;
    }

    // This task REMAINS cancellable because it involves multiple steps.
    public async SyncTask<bool> ClickOnPosition(Vector2 pos, CancellationToken token)
    {
        ReleaseHeldKeys(); // This synchronous method will run to completion quickly.

        // The rest of the method respects the cancellation token.
        await Task.Delay(DownUpDelay, token);
        Input.SetCursorPos(pos);
        await Task.Delay(DownUpDelay, token);
        Input.Click(MouseButtons.Left);
        return true;
    }

    // This is a private helper. We can have it either way, but for consistency,
    // let's make it non-cancellable as it's only used by Logout/Teleport.
    private async SyncTask<bool> SendKey(Keys key)
    {
        Input.KeyDown(key);
        await Task.Delay(DownUpDelay);
        Input.KeyUp(key);

        Instance.LogMessage($"Sent {key}");
        return true;
    }

    // This method is synchronous (void) as you designed.
    private void ReleaseHeldKeys()
    {
        var heldKeys = _keysToRelease.Where(Input.IsKeyDown);
        foreach (var key in heldKeys)
        {
            Input.KeyUp(key);
            Instance.LogMessage($"Released {key}");
        }
    }
}