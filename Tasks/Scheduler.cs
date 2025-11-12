using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ExileCore.Shared;

namespace Automate.Tasks;

public class Scheduler()
{
    private static readonly Automate Instance = Automate.Instance;
    private (SyncTask<bool> Task, string Name) _currentTaskInfo;
    private Queue<(Func<CancellationToken, SyncTask<bool>> TaskFactory, string Name)> _tasks = new();
    private CancellationTokenSource _cancellationTokenSource;

    public void AddTask(Func<CancellationToken, SyncTask<bool>> taskFactory, string name)
    {
        if (name != null)
            Instance.LogMessage($"Adding Task: {name}");
        else if (taskFactory != null) Instance.LogMessage($"Adding Task: {taskFactory}");

        _tasks.Enqueue((taskFactory, name));
    }

    public void ForceAddTask(Func<CancellationToken, SyncTask<bool>> taskFactory, string name)
    {
        CancelAllTasks();
        AddTask(taskFactory, name);
    }

    public void AddTasks(params (Func<CancellationToken, SyncTask<bool>> TaskFactory, string Name)[] tasks)
    {
        foreach (var (TaskFactory, Name) in tasks)
        {
            AddTask(TaskFactory, Name);
        }
    }

    public void Run()
    {
        // If no task is running and tasks are in the queue, start a new one.
        if (_currentTaskInfo.Task == null && _tasks.Count > 0)
        {
            var (taskFactory, name) = _tasks.Dequeue();

            // Create a new CancellationTokenSource for the new task.
            _cancellationTokenSource = new CancellationTokenSource();

            var task = taskFactory(_cancellationTokenSource.Token);
            _currentTaskInfo = (task, name);

            _currentTaskInfo.Task.GetAwaiter().OnCompleted(() =>
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                _currentTaskInfo = default;
            });
        }

        if (_currentTaskInfo.Task != null)
        {
            TaskUtils.RunOrRestart(ref _currentTaskInfo.Task, () => null);
        }
    }

    public void CancelAllTasks()
    {
        if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            Instance.LogMessage($"Cancelling current task: {_currentTaskInfo.Name}");
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        if (_tasks.Any())
        {
            Instance.LogMessage($"Clearing {_tasks.Count} scheduled tasks.");
            _tasks.Clear();
        }

        _currentTaskInfo = default;
    }

    public void Stop()
    {
        CancelAllTasks();
    }

    public void Clear()
    {
        _tasks.Clear();
    }
}