using Xunit;

// Integration tests share in-memory SQLite; run sequentially.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
