using System;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// This struct holds information about scene version identifier.
    /// </summary>
    public readonly struct SceneVersionId
    {
        readonly string m_String;

        /// <summary>
        /// Return the value of an identifier representing an invalid scene version id
        /// </summary>
        public static readonly SceneVersionId None = new(Guid.Empty.ToString());

         /// <summary>
        /// Returns a <see cref="SceneVersionId"/> using a <see cref="string"/>.
        /// </summary>
        /// <param name="value">The string representing the version identifier</param>
        public SceneVersionId(string value) => m_String = value;

        /// <summary>
        /// Returns whether two <see cref="SceneVersionId"/> objects are equals.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>
        /// <see langword="true"/> if both instance have the same values;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(SceneVersionId other) => m_String == other.m_String;

        /// <summary>
        /// Validate <paramref name="obj"/> is a <see cref="SceneVersionId"/> instance and have the same values as this instance.
        /// </summary>
        /// <param name="obj">Compare the values with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instance have the same values;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public override bool Equals(object obj) => obj is SceneVersionId other && Equals(other);

        /// <summary>
        /// Compute a hash code for the object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        /// <remarks>
        /// * You should not assume that equal hash codes imply object equality.
        /// * You should never persist or use a hash code outside the application domain in which it was created,
        ///   because the same object may hash differently across application domains, processes, and platforms.
        /// </remarks>
        public override int GetHashCode() => m_String != null ? m_String.GetHashCode() : 0;

        /// <summary>
        /// Get the string representation of this <see cref="SceneVersionId"/>.
        /// </summary>
        /// <returns>The string result.</returns>
        public override string ToString() => m_String;

        /// <summary>
        /// Get if two <see cref="SceneVersionId"/> represent the same.
        /// </summary>
        /// <param name="left">Compare with this first instance.</param>
        /// <param name="right">Compare with this other instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances represent the same;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool operator ==(SceneVersionId left, SceneVersionId right) => left.Equals(right);

        /// <summary>
        /// Get if two <see cref="SceneVersionId"/> does not represent the same.
        /// </summary>
        /// <param name="left">Compare with this first instance.</param>
        /// <param name="right">Compare with this other instance.</param>
        /// <returns>
        /// <see langword="true"/> if both instances are not the same;
        /// <see langword="false"/> if both instances are the same.
        /// </returns>
        public static bool operator !=(SceneVersionId left, SceneVersionId right) => !left.Equals(right);

        /// <summary>
        /// Explicitly cast a <see cref="SceneVersionId"/ to a <see cref="string"/>>
        /// </summary>
        /// <param name="sId">Object to cast</param>
        /// <returns>The resulting <see cref="string"/></returns>
        public static explicit operator string(SceneVersionId sId) => sId.m_String;
    }
}
