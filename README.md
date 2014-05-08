# fbcore - FlitBit.Core

Core frameworks, utilities, and extensions for dotNet. 

## Break with the Past

Version 3+ is compatible with the .NET Framework 4.5.1 and above.

If you are stuck on .NET Framework 4 you'll have to use version 2.11.6 which is published on NuGet.

## Get it on NuGet

The [FlitBit.Core](https://www.nuget.org/packages/FlitBit.Core/) library is published on [NuGet.org](https://www.nuget.org/packages/FlitBit.Core/) and can be installed through the Visual Studio `TOOLS` menu.

## Evolution

When you dig in to the code you notice that there is overlap with the latest Microsoft .NET Frameworks (~4.5). 
Where overlap occurs we either developed an implementation before one was available in stock .NET or we chose 
our own approach due to our perception that the Microsoft provided implementation constrained us. As stock implementations
appear in the .NET Framework we try to transition our code to the stock implementations if we deem them adequate for our
purposes.

## Useful Stuff

### Cleanup Scope

The `CleanupScope` class establishes an important pattern in the _FlitBit Frameworks_. It leverages dotNet's [Dispose Pattern](http://msdn.microsoft.com/en-us/library/b1yfkh5e(v=vs.110).aspx) to coordinate the cleanup of multiple `IDisposable` objects.
Cleanup scopes also enable us to schedule ad-hoc `Action`s to be executed when the scope is closed. The _FlitBit Frameworks_ build on this mechanism to lessen coupling between frameworks. We also use these scopes in parallel code enabling an outer scope to delay cleanup until forked operations complete.

```
// TODO: Lots more to say here
```
