using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ExileCore.PoEMemory;

namespace Automate.Tasks;

public class Hideout()
{
    private static readonly Automate Instance = Automate.Instance;
    private Stopwatch _otherHideoutTimer = new();
    private bool _teleportInitiated = false;
    private bool _wasInOwnHideout = false;

    private static TimeSpan Timeout => TimeSpan.FromSeconds(Instance.Settings.OtherHideoutTimeout.Value);
    private bool TimeoutReached => _otherHideoutTimer.Elapsed >= Timeout;
    private static Element LeaveHideoutButton => Instance.GameController.IngameState.IngameUi.LeagueMechanicButtons.GetChildAtIndex(2);
    private bool InHideout => Instance.GameController.Area.CurrentArea.IsHideout;
    private bool LeaveHideoutButtonVisible => LeaveHideoutButton != null && LeaveHideoutButton.IsVisibleLocal;
    private bool InOwnHideout => InHideout && !LeaveHideoutButtonVisible;
    private bool InOtherHideout => InHideout && LeaveHideoutButtonVisible;

    public void Tick()
    {
        // Detect when you first enter your own hideout
        if (InOwnHideout && !_wasInOwnHideout && (Instance.Settings.MyLittleCrafter || Instance.Settings.Stashie))
        {
            Instance.LogMessage("Entered own hideout. Scheduling OpenStash task.");

            // To add a CANCELLABLE task, pass the token to your method.
            // Signature: token => Method(token)
            Instance.Scheduler.AddTask(Instance.Stash.OpenStash, "Open Stash");

            if (Instance.Settings.Stashie)
                Instance.Scheduler.AddTask(Instance.Stash.StartStashie, "Start Stashie");

            if (Instance.Settings.MyLittleCrafter)
                Instance.Scheduler.AddTask(Instance.Stash.StartMyLittleCrafter, "Start MyLittleCrafter");
        }

        _wasInOwnHideout = InOwnHideout;

        if (!InHideout)
            return;

        if (InOtherHideout)
        {
            if (_teleportInitiated)
                return;

            if (_otherHideoutTimer.Elapsed == TimeSpan.Zero)
                _otherHideoutTimer.Restart();

            if (TimeoutReached)
            {
                _teleportInitiated = true;

                // To add a NON-CANCELLABLE task, ignore the token with `_`.
                // Signature: _ => Method()
                Instance.Scheduler.ForceAddTask(_ => Instance.Inputs.TeleportToHideout(), "Teleport to Hideout");
            }
        }
    }

    public void OnAreaChange()
    {
        if (LeaveHideoutButton == null)
        {
            Instance.LogError("Leave Hideout Button not found - Might have moved from 'IngameUi.LeagueMechanicButtons.GetChildAtIndex(2)'");
            return;
        }

        if (InOtherHideout)
        {
            if (_otherHideoutTimer.Elapsed == TimeSpan.Zero)
                _otherHideoutTimer.Restart();
        }
    }

    public void ResetOtherHideout()
    {
        if (_otherHideoutTimer.Elapsed != TimeSpan.Zero)
            _otherHideoutTimer.Reset();

        _teleportInitiated = false;
    }
}