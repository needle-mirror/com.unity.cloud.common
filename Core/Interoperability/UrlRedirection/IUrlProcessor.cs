namespace Unity.Cloud.Common
{
    /// <summary>
    /// An interface to handle the execution of an URL.
    /// </summary>
    public interface IUrlProcessor
    {
        /// <summary>
        /// Handles the execution of an URL.
        /// </summary>
        void ProcessURL(string url);
    }
}
