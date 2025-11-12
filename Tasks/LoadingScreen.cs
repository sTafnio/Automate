using System;
using System.Diagnostics;

namespace Automate.Tasks;

public class LoadingScreen()
{
    private static readonly Automate Instance = Automate.Instance;
    private Stopwatch _loadingTimer = new();
    private bool _logoutInitiated = false;
    private bool _wasLoading = false; // To detect the start of a loading screen

    private static TimeSpan Timeout => TimeSpan.FromSeconds(Instance.Settings.LoadingTimeout.Value);
    private bool TimeoutReached => _loadingTimer.Elapsed >= Timeout;

    public void Tick()
    {
        bool isLoading = Instance.GameController.IngameState.TheGame.IsLoading;

        // When the loading screen FIRST appears, cancel everything.
        if (isLoading && !_wasLoading)
        {
            Instance.Scheduler.CancelAllTasks();
            Instance.Hideout.ResetOtherHideout();
        }
        
        _wasLoading = isLoading;

        if (isLoading)
        {
            if (_logoutInitiated)
                return;

            if (_loadingTimer.Elapsed == TimeSpan.Zero)
                _loadingTimer.Restart();

            if (TimeoutReached)
            {
                _logoutInitiated = true;
                // We pass a lambda because ForceAddTask now expects a task factory.
                // This specific task is short, so it doesn't need the token itself.
                Instance.Scheduler.ForceAddTask(_ => Instance.Inputs.Logout(), "Logout");
            }
        }
        else
        {
            if (_loadingTimer.Elapsed != TimeSpan.Zero)
                _loadingTimer.Reset();

            _logoutInitiated = false;
        }
    }
}