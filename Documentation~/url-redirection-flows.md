# URL redirection flows

This section outlines the different URL redirection flows supported in a registered Unity Cloud application.

## Supported flows

The following are the supported flows:

- Automatic URL redirection triggered from the default OS browser after a successful login
- User triggered URL redirection when manually selecting a link

## UrlRedirectionInterceptor class

URL redirection flows are automatically supported using an internal runtime implementation of the IUrlRedirectionInterceptor interface. This cross-platform implementation intercepts OS level invocation through `Application.deepLinkActivated` events or other alternate interception mechanisms when the platform doesn't support OS level invocation.

## Url Activation of application per platform

The supported flows will use different URL schema, per platform, to trigger the activation of the application:

| URL Schema/Platform                                                         | iOS | Android | MacOS | Windows | WebGL | Unity Editor |
|---------------------------------------------------------------------------- |:---:|:-------:|:-----:|:-------:|:-----:|:------------:|
| `https://services.api.unity.com/app-linking/v1/link/\[LINK_PATH\]`                                   | x   | x       | x     | x       | o     | o            |
| `\[CUSTOM_URI_SCHEME\]://services.api.unity.com/app-linking/v1/link/\[LINK_PATH\]`                   | x   | x       | x     | x       | o     | o            |
| `https://\[CUSTOM_HOST_AND_PATH\]?\[LINK_PARAMS\]`                          | o   | o       | o     | o       | x     | o            |
| `\[CUSTOM_URI_SCHEME\]://implicit/callback/[OPERATION]?\[RESPONSE_PARAMS\]` | x   | x       | x     | x       | o     | o            |
| `http://localhost:[ANY_PORT]/\[RESPONSE_PATH\]/?\[RESPONSE_PARAMS\]`        | o   | o       | o     | o       | o     | x            |
