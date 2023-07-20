# NativeLibrariesNugetPackageDefiner (aka `dotnet-ntvlibpkgdef`)
A quickly hacked together tool for defining nuget packages for native libraries
in a fine-grained to top-level way with support for splitting large native
library files into multiple fragment nuget packages that are then joined
together as part of a build.