using System.Linq;
using System.Numerics;
using System.Threading;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using System.Threading.Tasks;

namespace Automate.Tasks;

public class Stash()
{
    private readonly Automate Instance = Automate.Instance;
    private Entity StashElement => Instance.GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Stash]?.FirstOrDefault(x => x.RenderName == "Stash");
    private bool IsStashOpen => Instance.GameController.IngameState.IngameUi.StashElement.IsVisible == true;
    private bool IsStashieActive => Instance.StashieIsActive.Invoke();

    public async SyncTask<bool> OpenStash(CancellationToken token)
    {
        await Task.Delay(2000, token);

        var stashWorldPos = StashElement.BoundsCenterPosNum;
        var stashScreenPos = Instance.GameController.Game.IngameState.Camera.WorldToScreen(stashWorldPos);
        var winRectLoc = Instance.GameController.Window.GetWindowRectangleTimeCache.Location.ToVector2Num();
        var clickPos = new Vector2(stashScreenPos.X + winRectLoc.X, stashScreenPos.Y + winRectLoc.Y);

        await Instance.Inputs.ClickOnPosition(clickPos, token);

        return true;
    }

    public async SyncTask<bool> StartStashie(CancellationToken token)
    {
        await TaskUtils.CheckEveryFrame(() => IsStashOpen, token);
        await Task.Delay(2000, token);
        Instance.StartStashie.Invoke();
        return true;
    }

    public async SyncTask<bool> StartMyLittleCrafter(CancellationToken token)
    {
        await TaskUtils.CheckEveryFrame(() => IsStashieActive, token);
        await Task.Delay(2000, token);
        Instance.StartMyLittleCrafter.Invoke();
        return true;
    }
}