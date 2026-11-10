using Xunit;

// GTK must be driven from a single thread and xUnit runs collections in
// parallel by default. Disable parallelism for the whole assembly so widget
// tests and the pure GLib/Cairo/Pango tests can never race.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
