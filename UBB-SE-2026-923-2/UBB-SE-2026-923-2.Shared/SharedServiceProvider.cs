namespace UBB_SE_2026_923_2.Shared
{
    using System;

    /// <summary>
    /// Static accessor for the application's DI container, populated by the
    /// host (Desktop App or Web Program) once <see cref="IServiceProvider"/> is
    /// built. Services that retain legacy parameterless constructors resolve
    /// their dependencies through this locator so they remain usable from call
    /// sites that have not yet been migrated to constructor injection.
    ///
    /// New code should use constructor injection — this exists only to keep
    /// the legacy desktop code paths working after the business logic moved
    /// into this class library.
    /// </summary>
    public static class SharedServiceProvider
    {
        public static IServiceProvider Services { get; set; } = null!;
    }
}
