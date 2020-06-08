# Socket Scanner Driver
A simple and powerful driver for socket mobile sanners in Xamarin Android apps.
## Overview
### Driver.cs
This is the helper class that should be a part of the SDK, this class should never be used directly in your project. 
For reference, view the quick start project's implementation of this scanner.
### Middleware.cs
Mediates the helper class (Driver.cs) and the base activity (Activity.cs). Although this class contains some direct calls, the 
implementation and thus the memory, must be managed by the scanner activity (ScannerActivity.cs).
### ScannerActivity.cs
An abstract class that your application's activities should extend. The scanner activity should always be used when calling the scanner, 
it contains critical calls that manage Middleware's implementation
