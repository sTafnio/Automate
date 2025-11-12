using System.Threading.Tasks;
using ExileCore.Shared;

namespace Automate.Tasks;

public class Login()
{
    private static readonly Automate Instance = Automate.Instance;
    private bool _loggedIn = false;

    private bool NeedLogin => !Instance.GameController.Game.IsInGameState;

    public void Tick()
    {
        if (NeedLogin && !_loggedIn)
        {
            Instance.Scheduler.ForceAddTask(_ => LogIntoGame(), "Logging In");
            _loggedIn = true;
        }

        if (!NeedLogin)
            _loggedIn = false;
    }

    public async SyncTask<bool> LogIntoGame()
    {
        while (NeedLogin)
        {
            await Instance.Inputs.SendEnter();
            await Task.Delay(1500);

            if (!NeedLogin)
                break;
        }
        return true;
    }
}