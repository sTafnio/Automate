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

    public async SyncTask<bool> Logout()
    {
        ReleaseHeldKeys();
        await SendKey(Instance.Settings.LogoutKey.Value.Key);
        return true;
    }

    public async SyncTask<bool> SendEnter()
    {
        ReleaseHeldKeys();
        await SendKey(Keys.Enter);
        await Task.Delay(1000);
        return true;
    }


    public async SyncTask<bool> TeleportToHideout()
    {
        ReleaseHeldKeys();
        await SendKey(Instance.Settings.HideoutKey.Value.Key);
        return true;
    }


    public async SyncTask<bool> ClickOnPosition(Vector2 pos, CancellationToken token)
    {
        ReleaseHeldKeys();

        await Task.Delay(DownUpDelay, token);
        Input.SetCursorPos(pos);
        await Task.Delay(DownUpDelay, token);
        Input.Click(MouseButtons.Left);
        return true;
    }

    private async SyncTask<bool> SendKey(Keys key)
    {
        Input.KeyDown(key);
        await Task.Delay(DownUpDelay);
        Input.KeyUp(key);

        Instance.LogMessage($"Sent {key}");
        return true;
    }

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