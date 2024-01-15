# Unity Cloud App Linking

![Common Feature Splash](images/splash-common.png)

Unity Cloud App Linking provides common application activation functionality to other Unity Cloud SDKs.

Some of the features provided by Unity Cloud App Linking include:

- **Application Activation:** A feature that enables activation of installed application on Android/iOS/OSX and Windows using the [Unity Cloud App Namespace](unity-cloud-app-namespace.md).
- **URL Redirection flow:** A feature that uses application activation feature to perform tasks and intercept awaited results. See [URL redirection flows](url-redirection-flows.md) for more details.

Unity Cloud App Linking supports the following Unity Cloud packages:

- **Unity Cloud Identity:** Supports the manual browser authentication flow where an installed application invokes the default OS browser and expects a response from it.
- **Unity Cloud Deep Linking:** Provides the application framework required to activate an application from deep links.
