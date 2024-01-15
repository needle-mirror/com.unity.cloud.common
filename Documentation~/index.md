# Unity Cloud Common

![Common Feature Splash](images/splash-common.png)

Unity Cloud Common provides a shared functionality used by the Unity Cloud SDKs. This functionality allows other Unity Cloud packages to offer a more consistent experience, and reduces the amount of code that might need to be duplicated or maintained in multiple places.

The Unity Cloud Common package does not provide any standalone features, and is not intended to be used directly. The Common package is a dependency of the other Unity Cloud packages, and will be installed automatically when Unity Cloud packages are installed.

Some of the features provided by Unity Cloud Common include:

- **Data Abstractions:** A common set of data types and abstractions used by the other Unity Cloud packages.
- **Networking:** Networking components that can be used to make authenticated requests to the Unity Cloud REST API.
