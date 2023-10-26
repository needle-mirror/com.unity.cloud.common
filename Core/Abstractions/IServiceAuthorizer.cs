using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// This interface abstracts the task of applying authorization information to a given resource.
    /// </summary>
    public interface IServiceAuthorizer
    {
        /// <summary>
        /// Applies authorization information to a given set of <see cref="HttpHeaders"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="headers"/> is null.</exception>
        Task AddAuthorization(HttpHeaders headers);
    }
}
