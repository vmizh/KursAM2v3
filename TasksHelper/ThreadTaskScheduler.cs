﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TasksHelper;

public class ThreadTaskScheduler : TaskScheduler
{
    protected override IEnumerable<Task> GetScheduledTasks()
    {
        return Enumerable.Empty<Task>();
    }

    protected override void QueueTask(Task task)
    {
        new Thread(() => TryExecuteTask(task)) { IsBackground = true }.Start();
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        return TryExecuteTask(task);
    }
}
