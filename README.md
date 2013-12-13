# fbcore - FlitBit.Core

Core frameworks, utilities, and extensions for dotNet. 

## Get it on NuGet

The [FlitBit.Core](https://www.nuget.org/packages/FlitBit.Core/) library is published on [NuGet.org](https://www.nuget.org/packages/FlitBit.Core/) and can be installed through the Visual Studio `TOOLS` menu.

## Evolution

As when you dig in you will likely notice that there is overlap
with the latest Microsoft .NET Framework libraries (~4.5). In some cases we chose an alternate implementation due to our own perception that the Microsoft provided implementation constrained us. In many
other cases, we simply implemented a capability that was not already a part of the framework. Newer versions of .NET have introduced classes that overlap the scope of some of our own and as we constantly 
evolve these libraries, be warned that our intention is to switch over to the Framework's implementations if they are deemed sufficient for our purposes.

## Useful Stuff

### Cleanup Scope

The `CleanupScope` class establishes an important pattern in the _FlitBit Frameworks_. It leverages dotNet's [Dispose Pattern](http://msdn.microsoft.com/en-us/library/b1yfkh5e(v=vs.110).aspx) to coordinate the cleanup of multiple `IDisposable` objects.
Cleanup scopes also enable you to schedule ad-hoc `Action`s to be executed when the scope is closed. The _FlitBit Frameworks_ build on this mechanism to lessen the coupling between frameworks. In sequential code it is simply an alternate syntax for multiple,
nested `using` statements. In parallel code it enables an outer scope to delay cleanup until forked-parallel scopes complete.

```
// TODO: Lots more to say here
```
