using System.Collections.Immutable;

namespace ConnectionManager.Cli.Services.Environment;

internal static class SystemDependencies
{
    internal static readonly SystemDependency Ssh = new(
        "ssh",
        SystemDependency.SystemDependencyType.Required,
        "OpenSSH remote login client"
    );

    internal static readonly SystemDependency SshPass = new(
        "sshpass",
        SystemDependency.SystemDependencyType.Optional,
        "Utility for non-interactive SSH password authentication"
    );

    internal static IReadOnlyCollection<SystemDependency> All => [Ssh, SshPass];
    internal static IReadOnlyDictionary<string, SystemDependency> AllByName =>
        All.ToImmutableDictionary(d => d.Name, d => d);
}

internal sealed class SystemDependency
{
    internal string Name { get; init; }
    internal SystemDependencyType Type { get; init; }
    internal string Description { get; init; }

    internal SystemDependency(string name, SystemDependencyType type, string description)
    {
        Name = name;
        Type = type;
        Description = description;
    }

    internal enum SystemDependencyType : byte
    {
        Unknown = 0,
        Required = 1,
        Optional = 2,
    }
}
