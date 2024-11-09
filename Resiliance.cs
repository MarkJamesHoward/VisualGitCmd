using Polly;
using Polly.Retry;

namespace MyProject;

public abstract class Resiliance {
    public static readonly ResiliencePipeline _resilienace =  new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                Delay = TimeSpan.FromSeconds(5),
                MaxRetryAttempts = 10,
        })
        .AddTimeout(TimeSpan.FromSeconds(60))
        .Build();
}