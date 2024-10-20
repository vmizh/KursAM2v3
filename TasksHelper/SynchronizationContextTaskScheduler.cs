﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TasksHelper;

public class SynchronizationContextTaskScheduler : TaskScheduler
{
    private readonly SynchronizationContext synchronizationContext;

    public SynchronizationContextTaskScheduler()
        : this(SynchronizationContext.Current)
    {
    }

    public SynchronizationContextTaskScheduler(SynchronizationContext synchronizationContext)
    {
        this.synchronizationContext = synchronizationContext;
    }

    protected override IEnumerable<Task> GetScheduledTasks()
    {
        return Enumerable.Empty<Task>();
    }

    protected override void QueueTask(Task task)
    {
        synchronizationContext.Post(_ => TryExecuteTask(task), null);
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        return synchronizationContext == SynchronizationContext.Current && TryExecuteTask(task);
    }
}
