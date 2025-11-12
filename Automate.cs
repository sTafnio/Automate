using System;
using System.Numerics;
using Automate.Tasks;
using ExileCore;
using SharpDX;

namespace Automate;

public class Automate : BaseSettingsPlugin<AutomateSettings>
{
    internal static Automate Instance;
    public Scheduler Scheduler;
    public Inputs Inputs;
    public LoadingScreen LoadingScreen;
    public Hideout Hideout;
    public Stash Stash;
    public Login Login;
    public Action StartStashie;
    public Func<bool> StashieIsActive;
    public Action StartMyLittleCrafter;


    public override bool Initialise()
    {
        Force = true;
        Instance = this;
        Scheduler = new();
        Inputs = new();
        LoadingScreen = new();
        Hideout = new();
        Stash = new();
        Login = new();

        StartStashie = GameController.PluginBridge.GetMethod<Action>("Stashie.Start");
        if (StartStashie == null)
            LogError("Stahie.Start is null");

        StashieIsActive = GameController.PluginBridge.GetMethod<Func<bool>>("Stashie.IsActive");
        if (StashieIsActive == null)
            LogError("Stashie.IsActive is null");

        StartMyLittleCrafter = GameController.PluginBridge.GetMethod<Action>("MyLittleCrafter.Start");
        if (StartMyLittleCrafter == null)
            LogError("MyLittleCrafter.Start is null");


        return true;
    }

    public override void AreaChange(AreaInstance area)
    {
        Hideout.OnAreaChange();
    }

    public override Job Tick()
    {
        Hideout.Tick();
        LoadingScreen.Tick();
        Login.Tick();
        Scheduler.Run();
        return null;
    }

    public override void Render()
    {
        if (!GameController.Window.IsForeground())
            return;

        var hoverElement = GameController.IngameState.UIHoverElement;
        Graphics.DrawTextWithBackground($"Hover Type: {hoverElement.Type}", new System.Numerics.Vector2(1300, 300), Color.White, Color.Black);
    }
}