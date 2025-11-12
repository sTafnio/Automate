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

    // Store task factories (a function that creates a task) instead of started tasks.
    private Queue<(Func<CancellationToken, SyncTask<bool>> TaskFactory, string Name)> _tasks = new();

    // The source that will generate the cancellation token for the running task.
    private CancellationTokenSource _cancellationTokenSource;

    // Change the parameter to be a factory function that accepts a CancellationToken.
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

            // Create and start the task by invoking the factory with the new token.
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

    /// <summary>
    /// Cancels the current task and clears all scheduled tasks.
    /// </summary>
    public void CancelAllTasks()
    {
        // Signal cancellation to the running task.
        if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            Instance.LogMessage($"Cancelling current task: {_currentTaskInfo.Name}");
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        // Clear any tasks that haven't started yet.
        if (_tasks.Any())
        {
            Instance.LogMessage($"Clearing {_tasks.Count} scheduled tasks.");
            _tasks.Clear();
        }

        _currentTaskInfo = default;
    }

    // The old Stop and Clear methods are effectively replaced by CancelAllTasks.
    public void Stop()
    {
        CancelAllTasks();
    }

    public void Clear()
    {
        _tasks.Clear();
    }
}