namespace UBB_SE_2026_923_2
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Minimal stub so that linked source files referencing App.Services compile
    /// inside the test project. Tests should always use the DI constructor
    /// overloads and never rely on this provider.
    /// </summary>
    internal static class App
    {
        private static IServiceProvider? services;

        public static IServiceProvider Services
        {
            get => services ?? new ServiceCollection().BuildServiceProvider();
            set => services = value;
        }
    }
}
