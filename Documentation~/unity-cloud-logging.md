# Unity Cloud Logging

The Unity Cloud Common SDK provides a logging system that allows you to log messages to any log output. The SDK provides a default log output that writes messages to the Unity Console. You can also create custom log outputs to write messages to other destinations, such as directly to a file.

## Enabling Unity Console Logging
All Unity Cloud SDKs contain log messages using the Unity Cloud Logging system. By default, logging these to the Unity Console is automatically enabled at runtime.

## Implementing your own Log Output
To implement your own log output, you must create a class that implements the `ILogOutput` interface. The `ILogOutput` interface contains a single method, `Write(LogEvent logEvent)`, which can be called to log information to the defined output. The interface also contains a `LogLevel` property, which can be used to define the minimum log level that should be logged, as well as an `Enabled` property, which can be used to enable or disable the log output.

To register your custom log output, you must call the `LogOutputs.Add(new YourCustomLogOutput());` method, passing in an instance of your custom log output class.
