using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Extensibility;
using NServiceBus.Logging;

namespace NServiceBus.Transport.InMemory
{
    public class InMemoryMessagePump : IPushMessages
    {
        private readonly ILog logger = LogManager.GetLogger<InMemoryMessagePump>();

        private NsbQueue queue;
        private bool purgeOnStartup;
        private TransportTransactionMode transactionMode;
        private SemaphoreSlim concurrencyLimiter;
        private CancellationTokenSource cancellationTokenSource;
        private Task processMessagesTask;
        private ConcurrentDictionary<Task, Task> runningReceiveTasks;
        private Func<MessageContext, Task> onMessage;
        private Func<ErrorContext, Task<ErrorHandleResult>> onError;
        private CriticalError criticalError;

        public InMemoryMessagePump(InMemoryDatabase inMemoryDatabase)
        {
            InMemoryDatabase = inMemoryDatabase;
        }

        private InMemoryDatabase InMemoryDatabase { get; }

        public Task Init(Func<MessageContext, Task> onMessage, Func<ErrorContext, Task<ErrorHandleResult>> onError, CriticalError criticalError, PushSettings settings)
        {
            InMemoryDatabase.CreateQueueIfNecessary(settings.InputQueue, new NsbQueue());
            queue = InMemoryDatabase.GetQueue(settings.InputQueue);
            if (queue == null)
            {
                throw new InvalidProgramException($"Unable to get or add the queue '{settings.InputQueue}'.");
            }

            purgeOnStartup = settings.PurgeOnStartup;
            transactionMode = settings.RequiredTransactionMode;
            this.onMessage = onMessage;
            this.onError = onError;
            this.criticalError = criticalError;

            return Task.CompletedTask;
        }

        public void Start(PushRuntimeSettings limitations)
        {
            runningReceiveTasks = new ConcurrentDictionary<Task, Task>();
            concurrencyLimiter = new SemaphoreSlim(limitations.MaxConcurrency);
            cancellationTokenSource = new CancellationTokenSource();

            if (purgeOnStartup)
            {
                queue.Clear();
            }

            processMessagesTask = Task.Factory
                .StartNew(
                    function: ProcessMessages, 
                    cancellationToken: CancellationToken.None,
                    creationOptions: TaskCreationOptions.LongRunning, 
                    scheduler: TaskScheduler.Default)
                .Unwrap();
        }

        async Task IPushMessages.Stop()
        {
            cancellationTokenSource.Cancel();

            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30), cancellationTokenSource.Token);
            var allTasks = runningReceiveTasks.Values.Append(processMessagesTask);
            var finishedTask = await Task.WhenAny(
                Task.WhenAll(allTasks),
                timeoutTask).ConfigureAwait(false);

            if (finishedTask.Equals(timeoutTask))
            {
                logger.Error("The message pump failed to stop with in the time allowed(30s)");
            }

            concurrencyLimiter.Dispose();
            runningReceiveTasks.Clear();
        }

        private async Task ProcessMessages()
        {
            try
            {
                await InnerProcessMessages()
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.Error("File Message pump failed", ex);
            }

            if (!cancellationTokenSource.IsCancellationRequested)
            {
                await ProcessMessages()
                    .ConfigureAwait(false);
            }
        }

        private async Task InnerProcessMessages()
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                while (queue.TryDequeue(out var message))
                {
                    await ProcessMessage(message).ConfigureAwait(false);
                }
            }
        }

        private async Task ProcessMessage(SerializedMessage message)
        {
            await concurrencyLimiter.WaitAsync(cancellationTokenSource.Token).ConfigureAwait(false);

            var task = Task.Run(async () =>
            {
                try
                {
                    await ProcessMessageWithTransaction(message).ConfigureAwait(false);
                }
                finally
                {
                    concurrencyLimiter.Release();
                }
            }, cancellationTokenSource.Token);

            _ = task.ContinueWith(t =>
            {
                runningReceiveTasks.TryRemove(t, out _);
            }, TaskContinuationOptions.ExecuteSynchronously);

            _ = runningReceiveTasks.AddOrUpdate(task, task, (k, v) => task);
        }

        private async Task ProcessMessageWithTransaction(SerializedMessage message)
        {
            var transportTransaction = new TransportTransaction();

            var succeeded = await HandleMessageWithRetries(message, transportTransaction, 1).ConfigureAwait(false);

            if (!succeeded)
            {
                queue.AddMessage(message);
            }
        }

        private async Task<bool> HandleMessageWithRetries(SerializedMessage message, TransportTransaction transportTransaction, int processingAttempt)
        {
            try
            {
                var receiveCancellationTokenSource = new CancellationTokenSource();
                var (messageId, headers, body) = message.Deserialize();

                var pushContext = new MessageContext(messageId, headers, body, transportTransaction, receiveCancellationTokenSource, new ContextBag());

                await onMessage(pushContext).ConfigureAwait(false);

                return !receiveCancellationTokenSource.IsCancellationRequested;
            }
            catch (Exception e)
            {
                var (messageId, headers, body) = message.Deserialize();
                var errorContext = new ErrorContext(e, headers, messageId, body, transportTransaction, processingAttempt);

                processingAttempt++;

                try
                {
                    var errorHandlingResult = await onError(errorContext).ConfigureAwait(false);

                    if (errorHandlingResult == ErrorHandleResult.RetryRequired)
                    {
                        return await HandleMessageWithRetries(message, transportTransaction, processingAttempt).ConfigureAwait(false);
                    }
                }
                catch (Exception exception)
                {
                    criticalError.Raise($"Failed to execute recoverability policy for message with native ID: `{messageId}`", exception);

                    return await HandleMessageWithRetries(message, transportTransaction, processingAttempt).ConfigureAwait(false);
                }

                return true;
            }
        }
    }
}
