using System;
using System.Threading.Tasks;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// This interface abstracts the Task of returning the string value of the user access token to get authenticated on cloud endpoints.
    /// </summary>
    public interface IAccessTokenProvider
    {
        /// <summary>
        /// Abstract a Task that results in the string value of user access token when completed.
        /// </summary>
        /// <returns>
        /// Returns a Task that results in the string value of user access token when completed.
        /// </returns>
        Task<string> GetAccessTokenAsync();
    }
}
