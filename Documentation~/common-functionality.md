# Common Functionality

Unity Cloud Common provides a shared functionality used by the Unity Cloud SDKs. This functionality allows other Unity Cloud packages to offer a more consistent experience, and reduces the amount of code that might need to be duplicated or maintained in multiple places.

>[!NOTE]
>This page describes some of the key functionality provided by the Unity Cloud Common SDK at a high level. More detailed use cases and code examples using these concepts are further documented within the specific [Unity Cloud SDK pages](docs.unity.com/cloud) for those packages which make use of them.

## Data Abstractions

One of the primary functions of the Unity Cloud Common package are to provide the base abstractions and data types shared by all other packages. These include, but are not limited to, a series of identifiers for the various organizations, projects, and assets used by the Unity Cloud platform.

Some of the most commonly used identifiers are listed below. These identifiers related to the assets managed by the [Unity Cloud Assets SDK](https://docs.unity3d.com/Packages/com.unity.cloud.assets@latest):

- `OrganizationId`: A struct that represents the unique identifier of an organization.
- `ProjectId`: A struct that represents the unique identifier of a project.
- `AssetId`: A struct that represents the unique identifier of an asset.
- `DatasetId`: A struct that represents the unique identifier of a dataset.
- `ProjectDescriptor`: A struct that regroups the organization and project identifiers used to identify a project.
- `AssetDescriptor`: A struct that regroups the organization, project, and asset identifiers used to identify an asset.
- `DatasetDescriptor`: A struct that regroups the organization, project, asset, and dataset identifiers used to identify a dataset.

## Networking

The Unity Cloud Common package also provides a series of networking components that used by all other packages to communicate with the Unity Cloud platform services. These components provide ways to easily add the appropriate authentication headers to all requests, as well as to provide a common way to handle errors and exceptions.

The core networking components used across all Unity Cloud packages are the following:

- `IServiceAuthorizer`: An interface that provides a way to add the appropriate authentication headers to all HTTP requests made to the Unity Cloud platform services. The main implementations of this interface exist in the [Unity Cloud Identity SDK](https://docs.unity3d.com/Packages/com.unity.cloud.identity@latest).
- `ServiceHostResolver`: A class which resolves the domain for all requests made to the Unity Cloud platform services via the `ServiceHttpClient`, described below.
- `ServiceHttpClient`: A class that provides a base implementation for all HTTP requests made to the Unity Cloud platform services, specifically to add the necessary headers for authorization.
- `IWebSocketClient`: An interface that provide a way to communicate with the Unity Cloud platform services via WebSockets.
- `IServiceMessagingClient`: An interface for a messaging client used to connect to a service and send/receive messages.

## App Registration

Applications built with Unity Cloud require an application identifier when you build the application. The application identifier identifies your application in the Unity Cloud services and allows the custom URI scheme association with the OS that's used in Unity Deep Linking and login operations.

For more information on how to setup your application identifier in the Unity Editor, see the [Unity Cloud App Registration](unity-cloud-app-registration.md) section.
