using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// Represents an error returned by a service.
    /// </summary>
    [Serializable]
    public class ServiceError
    {
        /// <summary>
        /// The error title.
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// The ID of the failed request.
        /// </summary>
        public string RequestId { get; set; } = "";

        /// <summary>
        /// The error detail.
        /// </summary>
        public string Detail { get; set; } = "";

        /// <summary>
        /// The error code.
        /// </summary>
        public ErrorCode Code { get; set; } = 0;

        /// <summary>
        /// The HTTP status code.
        /// </summary>
        public HttpStatusCode Status { get; set; } = 0;

        /// <summary>
        /// The error details.
        /// </summary>
        public List<ErrorDetails> Details { get; set; } = new ();

        /// <summary>
        /// The error type.
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// Enum of all known error codes that can be thrown by a service.
        /// </summary>
        public enum ErrorCode
        {
            // Don't forget to add any new error code to the documentation at docs/UnifiedErrors.md
            // New errors also need to be added to the HttpExceptionFactory, HttpErrorFactory and
            // HttpErrorExtensions.

            // X Legacy and unknown

            /// <summary>
            /// The server encountered an unspecified error.
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// Legacy error, usually means that the user's license is invalid.
            /// </summary>
            LegacyNotCompliant = 1,
            /// <summary>
            /// Legacy error, the client version is not supported anymore.
            /// </summary>
            LegacyVersionMismatch = 2,

            // 1XX License errors

            /// <summary>
            /// Some floating licenses exist but are all taken or the maximum number of concurrent devices for a seat
            /// has been reached.
            /// </summary>
            LicensingMaximumSeatReached = 100,
            /// <summary>
            /// No entitlement exist for the requested floating license.
            /// </summary>
            LicensingNoSeat = 101,
            /// <summary>
            /// The requested entitlement does not exist.
            /// </summary>
            LicensingNoEntitlementAvailable = 102,

            // 2XX Authentication errors

            /// <summary>
            /// The user is not authenticated and cannot access the requested resource.
            /// </summary>
            AuthUnauthorized = 200,
            /// <summary>
            /// The user is authenticated but does not have the right access the requested resource.
            /// </summary>
            AuthForbidden = 201,
            /// <summary>
            /// The device code has expired
            /// </summary>
            DeviceCodeExpired = 202,

            /// <summary>
            /// The authentication failed.
            /// </summary>
            AuthFailed = 203,

            // 3XX Sync errors

            /// <summary>
            /// The provided SyncModel is not supported by the target sync service.
            /// </summary>
            SyncModelNotSupported = 300,

            // 4XX Project errors

            /// <summary>
            /// A project with the same name already exist.
            /// </summary>
            ProjectAlreadyExists = 400,

            // 5XX Generic errors

            /// <summary>
            /// The request sent is missing some values or arguments.
            /// </summary>
            GenericBadRequest = 500,
            /// <summary>
            /// The requested resource cannot be found on the server.
            /// </summary>
            GenericNotFound = 501,
            /// <summary>
            /// The requested resource already exists.
            /// </summary>
            GenericConflict = 502,
            /// <summary>
            /// The application header sent was not recognized by the server.
            /// </summary>
            GenericUnknownApp = 503,
            /// <summary>
            /// The server encountered an unexpected error.
            /// </summary>
            GenericServerError = 504,

            // 6XX Multiplayer errors

            /// <summary>
            /// The server is at maximum capacity.
            /// </summary>
            MultiplayerMaxCapacityReached = 600,
        }

        /// <summary>
        /// Returns true if it's a licensing error.
        /// </summary>
        public bool IsLicensingError => (int)Code >= 100 && (int)Code < 200;

        /// <summary>
        /// Returns true if it's an authentication error.
        /// </summary>
        public bool IsAuthError => (int)Code >= 200 && (int)Code < 300;

        /// <summary>
        /// Represents the details of an error.
        /// </summary>
        [Serializable]
        public class ErrorDetails
        {
            /// <summary>
            /// The error code.
            /// </summary>
            public ErrorCode ErrorCode { get; set; } = new();

            /// <summary>
            /// The error message.
            /// </summary>
            public string ErrorMessage { get; set; } = "";
        }
    }

    /// <summary>
    /// An attribute for defining a <see cref="ServiceError"/> for a specific error code and HTTP status code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceErrorAttribute : Attribute
    {
        /// <summary>
        /// The error code of the service error.
        /// </summary>
        public ServiceError.ErrorCode ErrorCode { get; }

        /// <summary>
        /// The HTTP status code of the service error.
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="httpStatusCode"></param>
        public ServiceErrorAttribute(ServiceError.ErrorCode errorCode, HttpStatusCode httpStatusCode)
        {
            ErrorCode = errorCode;
            HttpStatusCode = httpStatusCode;
        }
    }

    /// <summary>
    /// Contains default error messages of <see cref="ServiceException"/>.
    /// </summary>
    public static class ServiceErrorMessage
    {
        ////////////////////
        // Project

        /// <summary>
        /// Project not found error message.
        /// </summary>
        public const string ProjectNotFound = "Project not found";

        /// <summary>
        /// Link not found error message.
        /// </summary>
        public const string LinkNotFound = "Link not found";

        ////////////////////
        // Sync

        /// <summary>
        /// Model not supported error message.
        /// </summary>
        public const string ModelNotSupported = "Model not supported";

        ////////////////////
        // Auth

        /// <summary>
        /// Resource access forbidden error message.
        /// </summary>
        public const string ResourceAccessForbidden = "Resource access forbidden";

        /// <summary>
        /// Unauthorized access error message.
        /// </summary>
        public const string UnauthorizedAccess = "Unauthorized access";

        /// <summary>
        /// Authentication failed error message.
        /// </summary>
        public const string AuthenticationFailed = "Authentication failed";

        /// <summary>
        /// Device code expired error message.
        /// </summary>
        public const string DeviceCodeExpired = "The device code has expired";

        /// <summary>
        /// Device code pending error message.
        /// </summary>
        public const string DeviceCodePending = "Authorization pending";

        ////////////////////
        // Licensing

        /// <summary>
        /// Maximum number of seats reached error message.
        /// </summary>
        public const string MaxSeatReached = "Maximum number of seats reached";

        /// <summary>
        /// Maximum number of devices reached error message.
        /// </summary>
        public const string MaxDeviceReached = "Maximum number of devices reached";

        /// <summary>
        /// No floating seats entitlement error message.
        /// </summary>
        public const string NoSeatEntitlement = "No floating seats entitlement";

        /// <summary>
        /// No entitlement available error message.
        /// </summary>
        public const string NoEntitlementAvailable = "No entitlement available";

        /// <summary>
        /// No license available error message.
        /// </summary>
        public const string NoLicenseAvailable = "No license available";

        ////////////////////
        // Generic

        /// <summary>
        /// Invalid argument error message.
        /// </summary>
        public const string InvalidArgument = "Invalid argument";

        /// <summary>
        /// Unexpected server error message.
        /// </summary>
        public const string UnexpectedServerError = "Unexpected server error";

        /// <summary>
        /// Unknown application error message.
        /// </summary>
        public const string UnknownApplication = "Unknown application";

        /// <summary>
        /// Resource not found error message.
        /// </summary>
        public const string ResourceNotFound = "Requested resource not found";

        ////////////////////
        // Connection

        /// <summary>
        /// Connection to cloud services failed error message.
        /// </summary>
        public const string ConnectionFailed = "Connection to cloud services failed";

        /// <summary>
        /// Maximum capacity reached error message.
        /// </summary>
        public const string MaxCapacityReached = "The server is at maximum capacity";
    }

    /// <summary>
    /// A factory for creating <see cref="ServiceError"/>s.
    /// </summary>
    static class ServiceErrorFactory
    {
        /// <summary>
        /// Creates a <see cref="ServiceError"/> corresponding to the <see cref="ServiceException"/> provided.
        /// The HttpError created contains the values defined in the <see cref="ServiceErrorAttribute"/> of the
        /// specific exception. If the child exception does not have an attribute, it defaults to the one defined for
        /// the base <see cref="ServiceException"/> class.
        /// </summary>
        /// <param name="exception">The exception from which to build a service error.</param>
        /// <returns>The resulting service error.</returns>
        public static ServiceError Build(ServiceException exception)
        {
            var attribute = (ServiceErrorAttribute)Attribute.GetCustomAttribute(
                exception.GetType(),
                typeof(ServiceErrorAttribute)
            );

            return new ServiceError
            {
                Title = exception.Message,
                Code = attribute.ErrorCode,
                Status = attribute.HttpStatusCode,
            };
        }
    }

    /// <summary>
    /// An exception related to a <see cref="ServiceError"/> returned by a service.
    /// </summary>
    [Serializable]
    [ServiceError(ServiceError.ErrorCode.Unknown, HttpStatusCode.BadRequest)]
    public class ServiceException : Exception
    {
        ServiceError serviceError { get; set; } = new () { Code = ServiceError.ErrorCode.Unknown };

        /// <summary>
        /// The service error title.
        /// </summary>
        public string Title => serviceError.Title;

        /// <summary>
        /// The ID of the failed request.
        /// </summary>
        public string RequestId => serviceError.RequestId;

        /// <summary>
        /// The service error detail.
        /// </summary>
        public string Detail => serviceError.Detail;

        /// <summary>
        /// The service error code.
        /// </summary>
        public ServiceError.ErrorCode ErrorCode => serviceError.Code;

        /// <summary>
        /// The service error HTTP status code.
        /// </summary>
        public HttpStatusCode? StatusCode => serviceError.Status;

        /// <summary>
        /// The service error details.
        /// </summary>
        public List<ServiceError.ErrorDetails> Details => serviceError.Details;

        /// <summary>
        /// The service error type.
        /// </summary>
        public string Type => serviceError.Type;

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected ServiceException() {}

        /// <summary>
        /// Creates and returns a <see cref="ServiceException"/> from the provided <see cref="ServiceError"/>.
        /// </summary>
        /// <param name="error">The service error.</param>
        public ServiceException(ServiceError error) : base(error.Title)
        {
            serviceError = error;
        }

        /// <summary>
        /// Creates and returns a <see cref="ServiceException"/> from the provided error message.
        /// </summary>
        /// <param name="message">The exception message.</param>
        protected ServiceException(string message) : base(message)
        {
            serviceError = ServiceErrorFactory.Build(this);
        }

        /// <summary>
        /// Creates and returns a <see cref="ServiceException"/> from the provided error message and inner exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ServiceException(string message, Exception innerException) : base(message, innerException)
        {
            serviceError = ServiceErrorFactory.Build(this);
        }

        /// <summary>
        /// Creates and returns a <see cref="ServiceException"/> from the provided serialization info and streaming context.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">the streaming context.</param>
        protected ServiceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            serviceError = (ServiceError) info.GetValue(nameof(serviceError), typeof(ServiceError));
        }

        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(serviceError), serviceError, typeof(ServiceError));
        }
    }

    /// <summary>
    /// A factory for creating <see cref="ServiceException"/>s.
    /// </summary>
    public static class ServiceExceptionFactory
    {
        /// <summary>
        /// Creates an exception corresponding to the error code contained in the provided <see cref="ServiceError"/>.
        /// If the code is not known or 0, a more generic <see cref="ServiceException"/> will be returned.
        /// </summary>
        /// <param name="error">The provided service error.</param>
        /// <returns>The created exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown when error is null.</exception>
        public static ServiceException Build(ServiceError error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));

            switch (error.Code)
            {
                // If the error code is unknown, try to throw a more generic exception using http codes
                case ServiceError.ErrorCode.Unknown:
                    break;
                case ServiceError.ErrorCode.LicensingMaximumSeatReached:
                    return new MaxSeatReachedException(error);
                case ServiceError.ErrorCode.LicensingNoSeat:
                    return new NoSeatEntitlementException(error);
                case ServiceError.ErrorCode.LicensingNoEntitlementAvailable:
                    return new NoEntitlementAvailableException(error);
                // Auth
                case ServiceError.ErrorCode.AuthUnauthorized:
                    return new UnauthorizedException(error);
                case ServiceError.ErrorCode.AuthForbidden:
                    return new ForbiddenException(error);
                case ServiceError.ErrorCode.DeviceCodeExpired:
                    return new DeviceCodeExpiredException(error);
                case ServiceError.ErrorCode.GenericBadRequest:
                    return new InvalidArgumentException(error);
                case ServiceError.ErrorCode.GenericNotFound:
                    return new NotFoundException(error);
                case ServiceError.ErrorCode.GenericUnknownApp:
                    return new UnknownApplicationException(error);
                case ServiceError.ErrorCode.GenericServerError:
                    return new ServerException(error);
            }

            switch (error.Status)
            {
                case HttpStatusCode.BadRequest:
                    error.Code = ServiceError.ErrorCode.GenericBadRequest;
                    return new InvalidArgumentException(error);
                case HttpStatusCode.Unauthorized:
                    error.Code = ServiceError.ErrorCode.AuthUnauthorized;
                    return new UnauthorizedException(error);
                case HttpStatusCode.Forbidden:
                    error.Code = ServiceError.ErrorCode.AuthForbidden;
                    return new ForbiddenException(error);
                case HttpStatusCode.NotFound:
                    error.Code = ServiceError.ErrorCode.GenericNotFound;
                    return new NotFoundException(error);
                case HttpStatusCode.RequestTimeout:
                    error.Code = ServiceError.ErrorCode.DeviceCodeExpired;
                    return new DeviceCodeExpiredException(error);
                case HttpStatusCode.InternalServerError:
                    error.Code = ServiceError.ErrorCode.GenericServerError;
                    return new ServerException(error);
            }

            return new ServiceException(error);
        }
    }

    /// <summary>
    /// This exception is thrown when an unauthenticated user tries to access a resource that requires authentication.
    /// </summary>
    [Serializable]
    [ServiceError(ServiceError.ErrorCode.AuthUnauthorized, HttpStatusCode.Unauthorized)]
    public class UnauthorizedException : ServiceException
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public UnauthorizedException() : base(ServiceErrorMessage.UnauthorizedAccess) {}

        /// <summary>
        /// Creates an instance from the provided <see cref="ServiceError"/>.
        /// </summary>
        /// <param name="error">The service error.</param>
        public UnauthorizedException(ServiceError error) : base(error)
        {
        }

        /// <summary>
        /// Creates an instance from the provided error message.
        /// </summary>
        /// <param name="msg">The error message.</param>
        public UnauthorizedException(string msg) : base(msg) {}

        /// <summary>
        /// Creates an instance from the provided error message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an instance from the provided serialization info and streaming context.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected UnauthorizedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception is thrown when a user request a resource but does not have the right to access it.
    /// </summary>
    [Serializable]
    [ServiceError(ServiceError.ErrorCode.AuthForbidden, HttpStatusCode.Forbidden)]
    public class ForbiddenException : ServiceException
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ForbiddenException() : base(ServiceErrorMessage.ResourceAccessForbidden) {}

        /// <summary>
        /// Creates an instance from the provided <see cref="ServiceError"/>.
        /// </summary>
        /// <param name="error">The service error.</param>
        public ForbiddenException(ServiceError error) : base(error)
        {
        }

        /// <summary>
        /// Creates an instance from the provided error message.
        /// </summary>
        /// <param name="msg">The error message.</param>
        public ForbiddenException(string msg) : base(msg) {}

        /// <summary>
        /// Creates an instance from the provided error message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ForbiddenException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an instance from the provided serialization info and streaming context.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected ForbiddenException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception is thrown when a user authentication failed. Mostly occurs when the authentication token
    /// sent to the server is invalid or expired.
    /// </summary>
    [Serializable]
    [ServiceError(ServiceError.ErrorCode.AuthFailed, HttpStatusCode.Unauthorized)]
    public class AuthenticationFailedException : ServiceException
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public AuthenticationFailedException() : base(ServiceErrorMessage.AuthenticationFailed) {}

        /// <summary>
        /// Creates an instance from the provided <see cref="ServiceError"/>.
        /// </summary>
        /// <param name="error">The service error.</param>
        public AuthenticationFailedException(ServiceError error) : base(error)
        {
        }

        /// <summary>
        /// Creates an instance from the provided error message.
        /// </summary>
        /// <param name="msg">The error message.</param>
        public AuthenticationFailedException(string msg) : base(msg) {}

        /// <summary>
        /// Creates an instance from the provided error message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public AuthenticationFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an instance from the provided serialization info and streaming context.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected AuthenticationFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception is thrown when a device code is expired.
    /// </summary>
    [Serializable]
    [ServiceError(ServiceError.ErrorCode.DeviceCodeExpired, HttpStatusCode.RequestTimeout)]
    public class DeviceCodeExpiredException : ServiceException
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public DeviceCodeExpiredException() : base(ServiceErrorMessage.DeviceCodeExpired) {}

        /// <summary>
        /// Creates an instance from the provided <see cref="ServiceError"/>.
        /// </summary>
        /// <param name="error">The service error.</param>
        public DeviceCodeExpiredException(ServiceError error) : base(error)
        {
        }

        /// <summary>
        /// Creates an instance from the provided error message.
        /// </summary>
        /// <param name="msg">The error message.</param>
        public DeviceCodeExpiredException(string msg) : base(msg) {}

        /// <summary>
        /// Creates an instance from the provided error message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DeviceCodeExpiredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an instance from the provided serialization info and streaming context.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected DeviceCodeExpiredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }


    /// <summary>
    /// This exception is thrown when a license is requested but none is available.
    /// </summary>
    [Serializable]
    [ServiceError(ServiceError.ErrorCode.LicensingNoEntitlementAvailable, HttpStatusCode.PaymentRequired)]
    public class LicenseUnavailableException : ServiceException
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public LicenseUnavailableException() : base (ServiceErrorMessage.NoLicenseAvailable) {}

        /// <summary>
        /// Creates an instance from the provided error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public LicenseUnavailableException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates an instance from the provided <see cref="ServiceError"/>.
        /// </summary>
        /// <param name="error">The service error.</param>
        public LicenseUnavailableException(ServiceError error) : base(error)
        {
        }

        /// <summary>
        /// Creates an instance from the provided error message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public LicenseUnavailableException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an instance from the provided serialization info and streaming context.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected LicenseUnavailableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception is thrown when some floating licenses exist, but are all taken.
    /// </summary>
    [Serializable]
    [ServiceError(ServiceError.ErrorCode.LicensingMaximumSeatReached, HttpStatusCode.Forbidden)]
    public class MaxSeatReachedException : ServiceException
    {
        /// <summary>
        /// Creates an instance from the provided error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public MaxSeatReachedException(string message) :
            base(message)
        {
        }

        /// <summary>
        /// Creates an instance from the provided <see cref="ServiceError"/>.
        /// </summary>
        /// <param name="error">The service error.</param>
        public MaxSeatReachedException(ServiceError error) : base(error)
        {
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MaxSeatReachedException() : base(ServiceErrorMessage.MaxSeatReached)
        {
        }

        /// <summary>
        /// Creates an instance from the provided error message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public MaxSeatReachedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an instance from the provided serialization info and streaming context.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected MaxSeatReachedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception is thrown when no entitlement exist for the requested floating license.
    /// </summary>
    [Serializable]
    [ServiceError(ServiceError.ErrorCode.LegacyNotCompliant, HttpStatusCode.BadRequest)]
    public class NoSeatEntitlementException : ServiceException
    {
        /// <summary>
        /// Creates an instance from the provided error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public NoSeatEntitlementException(string message) :
            base(message)
        {
        }


        /// <summary>
        /// Creates an instance from the provided <see cref="ServiceError"/>.
        /// </summary>
        /// <param name="error">The service error.</param>
        public NoSeatEntitlementException(ServiceError error) : base(error)
        {
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public NoSeatEntitlementException() : base(ServiceErrorMessage.NoSeatEntitlement)
        {
        }

        /// <summary>
        /// Creates an instance from the provided error message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public NoSeatEntitlementException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an instance from the provided serialization info and streaming context.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected NoSeatEntitlementException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception is thrown when the requested entitlement is not available.
    /// </summary>
    [Serializable]
    [ServiceError(ServiceError.ErrorCode.LicensingNoEntitlementAvailable, HttpStatusCode.PaymentRequired)]
    public class NoEntitlementAvailableException : ServiceException
    {
        /// <summary>
        /// Creates an instance from the provided error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public NoEntitlementAvailableException(string message) :
            base(message)
        {
        }

        /// <summary>
        /// Creates an instance from the provided <see cref="ServiceError"/>.
        /// </summary>
        /// <param name="error">The service error.</param>
        public NoEntitlementAvailableException(ServiceError error) : base(error)
        {
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public NoEntitlementAvailableException() : base(ServiceErrorMessage.NoEntitlementAvailable)
        {
        }

        /// <summary>
        /// Creates an instance from the provided error message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public NoEntitlementAvailableException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an instance from the provided serialization info and streaming context.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected NoEntitlementAvailableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception is thrown if the connection to the server fails.
    /// </summary>
    [Serializable]
    public class ConnectionException : ServiceException
    {
        /// <summary>
        /// Creates an instance from the provided <see cref="ServiceError"/>.
        /// </summary>
        /// <param name="error">The service error.</param>
        public ConnectionException(ServiceError error) : base(error)
        {
        }

        /// <summary>
        /// Creates an instance from the provided error message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an instance from the provided error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public ConnectionException(string message) : base(message)
        {
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ConnectionException() : base(ServiceErrorMessage.ConnectionFailed)
        {
        }

        /// <summary>
        /// Creates an instance from the provided serialization info and streaming context.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected ConnectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception is thrown if the resource is not found.
    /// </summary>
    [Serializable]
    [ServiceError(ServiceError.ErrorCode.GenericNotFound, HttpStatusCode.NotFound)]
    public class NotFoundException : ServiceException
    {
        /// <summary>
        /// Creates an instance from the provided error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public NotFoundException(string message) :
            base(message)
        {
        }

        /// <summary>
        /// Creates an instance from the provided <see cref="ServiceError"/>.
        /// </summary>
        /// <param name="error">The service error.</param>
        public NotFoundException(ServiceError error) : base(error)
        {
        }

        /// <summary>
        /// Creates an instance from the provided error message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public NotFoundException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public NotFoundException() : base(ServiceErrorMessage.ResourceNotFound)
        {
        }

        /// <summary>
        /// Creates an instance from the provided serialization info and streaming context.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected NotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception is thrown when the request sent is missing some values or arguments.
    /// </summary>
    [Serializable]
    [ServiceError(ServiceError.ErrorCode.GenericBadRequest, HttpStatusCode.BadRequest)]
    public class InvalidArgumentException : ServiceException
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public InvalidArgumentException() : base(ServiceErrorMessage.InvalidArgument) {}

        /// <summary>
        /// Creates an instance from the provided <see cref="ServiceError"/>.
        /// </summary>
        /// <param name="error">The service error.</param>
        public InvalidArgumentException(ServiceError error) : base(error)
        {
        }

        /// <summary>
        /// Creates an instance from the provided error message.
        /// </summary>
        /// <param name="msg">The error message.</param>
        public InvalidArgumentException(string msg) : base(msg) {}

        /// <summary>
        /// Creates an instance from the provided error message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public InvalidArgumentException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an instance from the provided serialization info and streaming context.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected InvalidArgumentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception is thrown when the server encountered an unexpected error.
    /// </summary>
    [Serializable]
    [ServiceError(ServiceError.ErrorCode.GenericServerError, HttpStatusCode.InternalServerError)]
    public class ServerException : ServiceException
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ServerException() : base(ServiceErrorMessage.UnexpectedServerError) {}

        /// <summary>
        /// Creates an instance from the provided <see cref="ServiceError"/>.
        /// </summary>
        /// <param name="error">The service error.</param>
        public ServerException(ServiceError error) : base(error)
        {
        }

        /// <summary>
        /// Creates an instance from the provided error message.
        /// </summary>
        /// <param name="msg">The error message.</param>
        public ServerException(string msg) : base(msg) {}

        /// <summary>
        /// Creates an instance from the provided error message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ServerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an instance from the provided serialization info and streaming context.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected ServerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// This exception is thrown when the application ID header is not valid.
    /// </summary>
    [Serializable]
    [ServiceError(ServiceError.ErrorCode.GenericUnknownApp, HttpStatusCode.BadRequest)]
    public class UnknownApplicationException : ServiceException
    {
        /// <summary>
        /// Creates an instance from the provided error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public UnknownApplicationException(string message) :
            base(message)
        {
        }

        /// <summary>
        /// Creates an instance from the provided <see cref="ServiceError"/>.
        /// </summary>
        /// <param name="error">The service error.</param>
        public UnknownApplicationException(ServiceError error) : base(error)
        {
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public UnknownApplicationException() : base(ServiceErrorMessage.UnknownApplication)
        {
        }

        /// <summary>
        /// Creates an instance from the provided error message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public UnknownApplicationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an instance from the provided serialization info and streaming context.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected UnknownApplicationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
