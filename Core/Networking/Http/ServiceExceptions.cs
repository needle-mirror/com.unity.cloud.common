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
            public ErrorCode ErrorCode { get; set; } = new();
            public string ErrorMessage { get; set; } = "";
        }
    }



    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceErrorAttribute : Attribute
    {
        public ServiceError.ErrorCode ErrorCode { get; }
        public HttpStatusCode HttpStatusCode { get; }

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
        // Project
        public const string ProjectNotFound = "Project not found";
        public const string LinkNotFound = "Link not found";

        // Sync
        public const string ModelNotSupported = "Model not supported";

        // Auth
        public const string ResourceAccessForbidden = "Resource access forbidden";
        public const string UnauthorizedAccess = "Unauthorized access";
        public const string AuthenticationFailed = "Authentication failed";
        public const string DeviceCodeExpired = "The device code has expired";
        public const string DeviceCodePending = "Authorization pending";

        // Licensing
        public const string MaxSeatReached = "Maximum number of seats reached";
        public const string MaxDeviceReached = "Maximum number of devices reached";
        public const string NoSeatEntitlement = "No floating seats entitlement";
        public const string NoEntitlementAvailable = "No entitlement available";
        public const string NoLicenseAvailable = "No license available";

        // Generic
        public const string InvalidArgument = "Invalid argument";
        public const string UnexpectedServerError = "Unexpected server error";
        public const string UnknownApplication = "Unknown application";
        public const string ResourceNotFound = "Requested resource not found";

        // Connection
        public const string ConnectionFailed = "Connection to cloud services failed";
        public const string MaxCapacityReached = "The server is at maximum capacity";
    }

    static class ServiceErrorFactory
    {
        /// <summary>
        /// Creates a <see cref="ServiceError"/> corresponding to the <see cref="ServiceException"/> provided.
        /// The HttpError created contains the values defined in the <see cref="ServiceErrorAttribute"/> of the
        /// specific exception. If the child exception does not have an attribute, it defaults to the one defined for
        /// the base <see cref="ServiceException"/> class.
        /// </summary>
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

    [Serializable]
    [ServiceError(ServiceError.ErrorCode.Unknown, HttpStatusCode.BadRequest)]
    public class ServiceException : Exception
    {
        ServiceError serviceError { get; set; } = new () { Code = ServiceError.ErrorCode.Unknown };

        public string Title => serviceError.Title;
        public string RequestId => serviceError.RequestId;
        public string Detail => serviceError.Detail;
        public ServiceError.ErrorCode ErrorCode => serviceError.Code;
        public HttpStatusCode? StatusCode => serviceError.Status;
        public List<ServiceError.ErrorDetails> Details => serviceError.Details;
        public string Type => serviceError.Type;

        protected ServiceException() {}

        public ServiceException(ServiceError error) : base(error.Title)
        {
            serviceError = error;
        }

        protected ServiceException(string message) : base(message)
        {
            serviceError = ServiceErrorFactory.Build(this);
        }

        public ServiceException(string message, Exception innerException) : base(message, innerException)
        {
            serviceError = ServiceErrorFactory.Build(this);
        }

        protected ServiceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            serviceError = (ServiceError) info.GetValue(nameof(serviceError), typeof(ServiceError));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(serviceError), serviceError, typeof(ServiceError));
        }
    }

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
        public UnauthorizedException() : base(ServiceErrorMessage.UnauthorizedAccess) {}

        public UnauthorizedException(ServiceError error) : base(error)
        {
        }

        public UnauthorizedException(string msg) : base(msg) {}

        public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
        {
        }

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
        public ForbiddenException() : base(ServiceErrorMessage.ResourceAccessForbidden) {}

        public ForbiddenException(ServiceError error) : base(error)
        {
        }

        public ForbiddenException(string msg) : base(msg) {}

        public ForbiddenException(string message, Exception innerException) : base(message, innerException)
        {
        }

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
        public AuthenticationFailedException() : base(ServiceErrorMessage.AuthenticationFailed) {}

        public AuthenticationFailedException(ServiceError error) : base(error)
        {
        }

        public AuthenticationFailedException(string msg) : base(msg) {}

        public AuthenticationFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

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
        public DeviceCodeExpiredException() : base(ServiceErrorMessage.DeviceCodeExpired) {}

        public DeviceCodeExpiredException(ServiceError error) : base(error)
        {
        }

        public DeviceCodeExpiredException(string msg) : base(msg) {}

        public DeviceCodeExpiredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DeviceCodeExpiredException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }


    /// <summary>
    /// This exception is thrown by <see cref="ProjectServerClient"/> when a license is requested but none is available.
    /// </summary>
    [Serializable]
    [ServiceError(ServiceError.ErrorCode.LicensingNoEntitlementAvailable, HttpStatusCode.PaymentRequired)]
    public class LicenseUnavailableException : ServiceException
    {
        public LicenseUnavailableException() : base (ServiceErrorMessage.NoLicenseAvailable) {}

        public LicenseUnavailableException(string message) : base(message)
        {
        }

        public LicenseUnavailableException(ServiceError error) : base(error)
        {
        }

        public LicenseUnavailableException(string message, Exception innerException) : base(message, innerException)
        {
        }

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
        public MaxSeatReachedException(string message) :
            base(message)
        {
        }

        public MaxSeatReachedException(ServiceError error) : base(error)
        {
        }

        public MaxSeatReachedException() : base(ServiceErrorMessage.MaxSeatReached)
        {
        }

        public MaxSeatReachedException(string message, Exception innerException) : base(message, innerException)
        {
        }

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
        public NoSeatEntitlementException(string message) :
            base(message)
        {
        }

        public NoSeatEntitlementException(ServiceError error) : base(error)
        {
        }

        public NoSeatEntitlementException() : base(ServiceErrorMessage.NoSeatEntitlement)
        {
        }

        public NoSeatEntitlementException(string message, Exception innerException) : base(message, innerException)
        {
        }

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
        public NoEntitlementAvailableException(string message) :
            base(message)
        {
        }

        public NoEntitlementAvailableException(ServiceError error) : base(error)
        {
        }

        public NoEntitlementAvailableException() : base(ServiceErrorMessage.NoEntitlementAvailable)
        {
        }

        public NoEntitlementAvailableException(string message, Exception innerException) : base(message, innerException)
        {
        }

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
        public ConnectionException(ServiceError error) : base(error)
        {
        }

        public ConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ConnectionException(string message) : base(message)
        {
        }

        public ConnectionException() : base(ServiceErrorMessage.ConnectionFailed)
        {
        }

        protected ConnectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    [ServiceError(ServiceError.ErrorCode.GenericNotFound, HttpStatusCode.NotFound)]
    public class NotFoundException : ServiceException
    {
        public NotFoundException(string message) :
            base(message)
        {
        }

        public NotFoundException(ServiceError error) : base(error)
        {
        }

        public NotFoundException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        public NotFoundException() : base(ServiceErrorMessage.ResourceNotFound)
        {
        }

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
        public InvalidArgumentException() : base(ServiceErrorMessage.InvalidArgument) {}

        public InvalidArgumentException(ServiceError error) : base(error)
        {
        }

        public InvalidArgumentException(string msg) : base(msg) {}

        public InvalidArgumentException(string message, Exception innerException) : base(message, innerException)
        {
        }

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
        public ServerException() : base(ServiceErrorMessage.UnexpectedServerError) {}

        public ServerException(ServiceError error) : base(error)
        {
        }

        public ServerException(string msg) : base(msg) {}

        public ServerException(string message, Exception innerException) : base(message, innerException)
        {
        }

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
        public UnknownApplicationException(string message) :
            base(message)
        {
        }

        public UnknownApplicationException(ServiceError error) : base(error)
        {
        }

        public UnknownApplicationException() : base(ServiceErrorMessage.UnknownApplication)
        {
        }

        public UnknownApplicationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnknownApplicationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
