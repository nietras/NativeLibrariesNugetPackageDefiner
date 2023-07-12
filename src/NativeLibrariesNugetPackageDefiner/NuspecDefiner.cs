using System.Collections.Generic;
using System.Text;

namespace NativeLibrariesNugetPackageDefiner;

public static class NuspecDefiner
{
    public static string RuntimeSpecificPackage(string packageId, string version, string author, IReadOnlyCollection<string> fragments) =>
    $"""
    <?xml version="1.0" encoding="utf-8"?>
    <package xmlns="http://schemas.microsoft.com/packaging/2013/01/nuspec.xsd">
      <metadata minClientVersion="5.10">
        <id>{packageId}</id>
        <version>{version}</version>
        <authors>{author}</authors>
        <owners>{author}</owners>
        <license type="expression">MIT</license>
        <description>native library runtime specific package{(fragments.Count > 0 ? $" ({fragments.Count} fragments split out to limit nuget package size)" : string.Empty)}</description>
        <copyright>Copyright © </copyright>
        <readme>README.md</readme>
    {(fragments.Count > 0 ? Dependencies(packageId, version, fragments) : string.Empty)}
      </metadata>
    </package>
    """;

    public static string RuntimeSpecificFragmentPackage(string packageId, string version, string author,
        string fragment, int index, int lastIndex) =>
    $"""
    <?xml version="1.0" encoding="utf-8"?>
    <package xmlns="http://schemas.microsoft.com/packaging/2013/01/nuspec.xsd">
      <metadata minClientVersion="5.10">
        <id>{FragmentPackageId(packageId, fragment)}</id>
        <version>{version}</version>
        <authors>{author}</authors>
        <owners>{author}</owners>
        <license type="expression">MIT</license>
        <description>native library runtime specific fragment {index}/{lastIndex} package</description>
        <copyright>Copyright © </copyright>
        <readme>README.md</readme>
      </metadata>
    </package>
    """;

    public static string MetaPackage(string packageId, string version, string author) =>
    $"""
    <?xml version="1.0" encoding="utf-8"?>
    <package xmlns="http://schemas.microsoft.com/packaging/2013/01/nuspec.xsd">
      <metadata minClientVersion="5.10">
        <id>{packageId}</id>
        <version>{version}</version>
        <authors>{author}</authors>
        <owners>{author}</owners>
        <license type="expression">MIT</license>
        <description>native library meta package</description>
        <copyright>Copyright © </copyright>
        <readme>README.md</readme>
        <dependencies>
          <group targetFramework=".NETStandard1.1" />
        </dependencies>
      </metadata>
    </package>
    """;

    internal static string MetaPackage(string packageId, string version, string author, IReadOnlyCollection<PackageInfo> dependencies) =>
    $"""
    <?xml version="1.0" encoding="utf-8"?>
    <package xmlns="http://schemas.microsoft.com/packaging/2013/01/nuspec.xsd">
      <metadata minClientVersion="5.10">
        <id>{packageId}</id>
        <version>{version}</version>
        <authors>{author}</authors>
        <owners>{author}</owners>
        <license type="expression">MIT</license>
        <description>meta package</description>
        <copyright>Copyright © </copyright>
        <readme>README.md</readme>
    {(dependencies.Count > 0 ? Dependencies(dependencies) : string.Empty)}
      </metadata>
    </package>
    """;

    static string Dependencies(string packageId, string version, IReadOnlyCollection<string> fragments)
    {
        var indent = "    ";
        var header = $"{indent}<dependencies>";
        var footer = $"{indent}</dependencies>";
        var sb = new StringBuilder();
        sb.AppendLine(header);
        foreach (var fragment in fragments)
        {
            sb.AppendLine($"{indent}  <dependency id=\"{FragmentPackageId(packageId, fragment)}\" version=\"{version}\" exclude=\"Build,Analyzers\" />");
        }
        sb.Append(footer);
        return sb.ToString();
    }

    static string Dependencies(IReadOnlyCollection<PackageInfo> dependencies)
    {
        var indent = "    ";
        var header = $"{indent}<dependencies>";
        var footer = $"{indent}</dependencies>";
        var sb = new StringBuilder();
        sb.AppendLine(header);
        foreach (var dependency in dependencies)
        {
            sb.AppendLine($"{indent}  <dependency id=\"{dependency.Name}\" version=\"{dependency.Version}\" />");
        }
        sb.Append(footer);
        return sb.ToString();
    }

    public static string FragmentPackageId(string packageId, string fragment) => $"{packageId}.{fragment}";
}
