# Unity Cloud App Linking

![Common Feature Splash](images/splash-common.png)

Unity Cloud App Linking provides common application activation functionality to other Unity Cloud SDKs.

Some of the features provided by Unity Cloud App Linking include:

- **Application Activation:** A feature that enables activation of installed application on Android/iOS/OSX and Windows using the [Unity Cloud App Namespace](unity-cloud-app-namespace.md).
- **URL Redirection flow:** A feature that uses application activation feature to perform tasks and intercept awaited results. See [URL redirection flows](url-redirection-flows.md) for more details.

Unity Cloud App Linking supports the following Unity Cloud packages:

- **Unity Cloud Identity:** Supports the manual browser authentication flow where an installed application invokes the default OS browser and expects a response from it.
- **Unity Cloud Deep Linking:** Provides the application framework required to activate an application from deep links.

## Platform Specific Requirements for Distribution or Runtime Builds

### Windows

When distributing a Runtime Build on Windows, it is recommended to manage the registration and unregistration of the custom App Namespace in the Windows Registry, in install/uninstall sequences. This ensures that the activation of the installed App using the custom App Namespace can be achieved as soon as installation completed.
If the Windows Registry does not contain a key for the custom App Namespace, the App itself will create one in **HKEY_CURRENT_USER\Software\Classes** on the first login operation managed by the com.unity.cloud.identity SDK.

### iOS, MacOs

In Post-Build operations, the Cloud SDK injects the custom App Namespace inside the info.list of iOS and MacOS builds. No additional operation is required for the iOS or MacOs App to support activation when using the custom App Namespace on a device.

### Android

In Post-Build operations, the Cloud SDK injects the custom App Namespace inside the AppManifext.xml on Android builds. No additional operation is required for the Android App to support activation when using the custom App Namespace on a device.