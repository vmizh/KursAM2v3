using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TasksHelper;

public class PriorityTaskScheduler : TaskScheduler
{
    private readonly LinkedList<Task> tasksList = new LinkedList<Task>();

    public bool Prioritize(Task task)
    {
        lock (tasksList)
        {
            var node = tasksList.Find(task);
            if (node == null) return false;
            tasksList.Remove(node);
            tasksList.AddFirst(node);
            return true;
        }
    }

    public bool Deprioritize(Task task)
    {
        lock (tasksList)
        {
            var node = tasksList.Find(task);
            if (node == null) return false;
            tasksList.Remove(node);
            tasksList.AddLast(node);
            return true;
        }
    }

    protected override IEnumerable<Task> GetScheduledTasks()
    {
        lock (tasksList)
        {
            return tasksList;
        }
    }

    protected override bool TryDequeue(Task task)
    {
        lock (tasksList)
        {
            return tasksList.Remove(task);
        }
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        return TryExecuteTask(task);
    }

    protected override void QueueTask(Task task)
    {
        lock (tasksList)
        {
            tasksList.AddLast(task);
        }

        ThreadPool.QueueUserWorkItem(ProcessNextQueuedItem, null);
    }

    private void ProcessNextQueuedItem(object _)
    {
        Task task;

        lock (tasksList)
        {
            if (tasksList.Count > 0)
            {
                task = tasksList.First.Value;
                tasksList.RemoveFirst();
            }
            else
            {
                return;
            }
        }

        TryExecuteTask(task);
    }
}
