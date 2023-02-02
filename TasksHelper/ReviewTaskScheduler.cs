using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TasksHelper;

public class ReviewTaskScheduler : TaskScheduler
{
    private readonly LinkedList<Task> tasksList = new LinkedList<Task>();

    protected override IEnumerable<Task> GetScheduledTasks()
    {
        lock (tasksList)
        {
            return tasksList;
        }
    }

    /// <summary>
    ///     Метод вызывается методом Start класса Task
    /// </summary>
    /// <param name="task"></param>
    protected override void QueueTask(Task task)
    {
        Console.WriteLine($"    [QueueTask] Задача #{task.Id} поставлена в очередь..");
        lock (tasksList)
        {
            tasksList.AddLast(task);
        }

        ThreadPool.QueueUserWorkItem(ExecuteTasks, null);
    }

    /// <summary>
    ///     Метод вызывается методами ожидания, к примеру Wait, WaitAll...
    /// </summary>
    /// <param name="task"></param>
    /// <param name="taskWasPreviouslyQueued"></param>
    /// <returns></returns>
    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        Console.WriteLine($"        [TryExecuteTaskInline] Попытка выполнить задачу #{task.Id} синхронно..");

        lock (tasksList)
        {
            tasksList.Remove(task);
        }

        return TryExecuteTask(task);
    }

    /// <summary>
    ///     Метод вызывается при отмене выполнения задачи
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    protected override bool TryDequeue(Task task)
    {
        Console.WriteLine($"            [TryDequeue] Попытка удалить задачу {task.Id} из очереди..");
        bool result;

        lock (tasksList)
        {
            result = tasksList.Remove(task);
        }

        if (result)
            Console.WriteLine(
                $"                [TryDequeue] Задача {task.Id} была удалена из очереди на выполнение..");

        return result;
    }

    private void ExecuteTasks(object _)
    {
        while (true)
        {
            Task task;

            lock (tasksList)
            {
                if (tasksList.Count == 0) break;

                task = tasksList.First.Value;
                tasksList.RemoveFirst();
            }

            if (task == null) break;

            TryExecuteTask(task);
        }
    }
}
