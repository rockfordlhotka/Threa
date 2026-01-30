using Microsoft.VisualStudio.TestTools.UnitTesting;

// Disable parallel test execution because several DAL implementations
// use static in-memory collections that aren't thread-safe.
// This can be removed once all DALs use proper SQLite storage.
[assembly: DoNotParallelize]
