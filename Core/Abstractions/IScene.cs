using System;
using System.Collections.Generic;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// This interface holds information about a scene.
    /// </summary>
    public interface IScene
    {
        /// <summary>
        /// The name of the scene.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The id of the scene.
        /// </summary>
        SceneId Id { get; }

        /// <summary>
        /// The workspace id of the scene.
        /// </summary>
        WorkspaceId WorkspaceId { get; }

        /// <summary>
        /// The list of <see cref="Permission"/> of the scene.
        /// </summary>
        List<Permission> Permissions { get; }

        /// <summary>
        /// The id of the latest version of the scene.
        /// </summary>
        SceneVersionId LatestVersion { get; }
    }
}
