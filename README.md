# fbcore - FlitBit.Core

Core frameworks, utilities, and extensions for dotNet. 

## Get it on NuGet

The [FlitBit.Core](https://www.nuget.org/packages/FlitBit.Core/) library is published on [NuGet.org](https://www.nuget.org/packages/FlitBit.Core/) and can be installed through the Visual Studio `TOOLS` menu.

## Useful Stuff

### Cleanup Scope

The `CleanupScope` class establishes an important pattern in the _FlitBit Frameworks_. It leverages dotNet's [Dispose Pattern](http://msdn.microsoft.com/en-us/library/b1yfkh5e(v=vs.110).aspx) to coordinate the cleanup of multiple `IDisposable` objects.
Cleanup scopes also enable you to schedule ad-hoc `Action`s to be executed when the scope is closed. The _FlitBit Frameworks_ build on this mechanism to lessen the coupling between frameworks.