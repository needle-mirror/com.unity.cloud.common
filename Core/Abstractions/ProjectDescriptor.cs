
namespace Unity.Cloud.Common
{
    /// <summary>
    /// This struct contains the identifiers for a project.
    /// </summary>
    public readonly struct ProjectDescriptor
    {
        /// <summary>
        /// The project's organization ID.
        /// </summary>
        public readonly OrganizationId OrganizationId;

        /// <summary>
        /// The project's ID.
        /// </summary>
        public readonly ProjectId ProjectId;

        /// <summary>
        /// Creates an instance of the <see cref="ProjectDescriptor"/> struct.
        /// </summary>
        /// <param name="organizationId">The project's organization genesis ID.</param>
        /// <param name="projectId">The project's ID.</param>
        public ProjectDescriptor(OrganizationId organizationId, ProjectId projectId)
        {
            OrganizationId = organizationId;
            ProjectId = projectId;
        }

        /// <summary>
        /// Returns whether two <see cref="ProjectDescriptor"/> objects are equals.
        /// </summary>
        /// <param name="other">Compare the values with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instance have the same values;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(ProjectDescriptor other)
        {
            return OrganizationId.Equals(other.OrganizationId) &&
                   ProjectId.Equals(other.ProjectId);
        }

        /// <summary>
        /// Validate <paramref name="obj"/> is a <see cref="ProjectDescriptor"/> instance and have the same values as this instance.
        /// </summary>
        /// <param name="obj">Compare the values with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instance have the same values;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is ProjectDescriptor other && Equals(other);

        /// <summary>
        /// Compute a hash code for the object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        /// <remarks>
        /// * You should not assume that equal hash codes imply object equality.
        /// * You should never persist or use a hash code outside the application domain in which it was created,
        ///   because the same object may hash differently across application domains, processes, and platforms.
        /// </remarks>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = OrganizationId.GetHashCode();
                hashCode = (hashCode * 397) ^ ProjectId.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Get if two <see cref="ProjectDescriptor"/> represent the same.
        /// </summary>
        /// <param name="left">Compare with this first instance.</param>
        /// <param name="right">Compare with this other instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances represent the same;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator ==(ProjectDescriptor left, ProjectDescriptor right) => left.Equals(right);

        /// <summary>
        /// Get if two <see cref="ProjectDescriptor"/> does not represent the same.
        /// </summary>
        /// <param name="left">Compare with this first instance.</param>
        /// <param name="right">Compare with this other instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances are not the same;
        /// <see langword="false"/> if both instances are the same.
        /// </returns>
        public static bool operator !=(ProjectDescriptor left, ProjectDescriptor right) => !left.Equals(right);
    }
}
