# Processors Overview
**Cannoli Processors** allow you to pass jobs into a dedicated job queue and process them with an asynchonous handler using dependency injection (DI). This is useful for processing long-running tasks, or centralizing processing for tasks that may come from multiple sources.

 ## Using Cannoli Processors

### Setting up a processor
To create a processor, create a class that implements `ICannoliProcessor<T>`, where `T` is the job type. This will require you to implement the method `HandleJobAsync(T job)`. The processor will be automatically discovered at startup and registered with DI.

### Enqueueing new jobs
To enqueue a new job, inject an instance of `ICannoliJobQueue<T>` into any DI compatible class. From there, you can use `EnqueueJob(T job, Priority priority)`. Jobs enqueued with `Priority.High` will be dequeued sooner than `Priority.Normal`. This method will return a `TaskCompletionSource` which you can use to track the job's status.

### Scheduling repeating jobs
You can schedule a repeating job via `ICannoliJobQueue<T>`, by calling `ScheduleRepeatingJob(Timespan t, T Job, bool runImmediately)`.

## Lifetime

Cannoli Processors are transient. When a new job arrives in the job queue, a new instance of the class will be created using DI. If you need to access shared variables across requests, you may need to implement a singleton service.

Jobs are not persisted to the database and are lost when the application exits.

## Concurrency
By default, job queues will, without restriction, spin up a new processor for each new job. If you would like to limit the maximim number of concurrent jobs for a given processor, you can decorate the class with the `CannoliProcessor` attribute. For example, `[CannoliProcessor(maxConcurrentJobs: 1)]`.

## Example

```csharp
[CannoliProcessor(maxConcurrentJobs: 2)]
public class EmailNotificationProcessor : ICannoliProcessor<EmailNotificationJob>
{
    private readonly ILogger<EmailNotificationProcessor> _logger;
    private readonly IEmailService _emailService;

    public EmailNotificationProcessor(
        IEmailService emailService,
        ILogger<EmailNotificationProcessor> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task HandleJobAsync(EmailNotificationJob job)
    {
        _logger.LogInformation("Processing email notification for {email}", job.EmailAddress);
        await _emailService.SendEmailAsync(job.EmailAddress, job.Subject, job.Body);
    }
}

public class Foo
{
    private readonly ICannoliJobQueue<EmailNotificationJob> _emailJobQueue;

    public Foo(
        ICannoliJobQueue<EmailNotificationJob> emailJobQueue)
    {
        _emailJobQueue = emailJobQueue;
    }

    public void DoSomething()
    {
        _emailJobQueue.EnqueueJob(new EmailNotificationJob
        {
          EmailAddress = "foo@bar.com",
          Subject = "Hello world!",
          Body = "This is a test email."
        });
    }
}
```