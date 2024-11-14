using Sentry;
using Sentry.Profiling;

namespace MyProject {

public abstract class SentryMethods {
public static void ConfigureSentry() {

SentrySdk.Init(options =>
{
    // A Sentry Data Source Name (DSN) is required.
    // See https://docs.sentry.io/product/sentry-basics/dsn-explainer/
    // You can set it in the SENTRY_DSN environment variable, or you can set it in code here.
    options.Dsn = "https://952676e11df67910c694d5b6f3e71c7d@o4507897151356928.ingest.us.sentry.io/4507897155682304";

    // When debug is enabled, the Sentry client will emit detailed debugging information to the console.
    // This might be helpful, or might interfere with the normal operation of your application.
    // We enable it here for demonstration purposes when first trying Sentry.
    // You shouldn't do this in your applications unless you're troubleshooting issues with Sentry.
    options.Debug = false;

    // This option is recommended. It enables Sentry's "Release Health" feature.
    options.AutoSessionTracking = true;

    // Set TracesSampleRate to 1.0 to capture 100%
    // of transactions for tracing.
    // We recommend adjusting this value in production.
    options.TracesSampleRate = 1.0;

    // Sample rate for profiling, applied on top of othe TracesSampleRate,
    // e.g. 0.2 means we want to profile 20 % of the captured transactions.
    // We recommend adjusting this value in production.
    options.ProfilesSampleRate = 1.0;
    // Requires NuGet package: Sentry.Profiling
    // Note: By default, the profiler is initialized asynchronously. This can
    // be tuned by passing a desired initialization timeout to the constructor.
    options.AddIntegration(new ProfilingIntegration(
        // During startup, wait up to 500ms to profile the app startup code.
        // This could make launching the app a bit slower so comment it out if you
        // prefer profiling to start asynchronously
        TimeSpan.FromMilliseconds(500)
    ));
});
}

}
}