using System.Runtime.Serialization;
#if UNITY_2022_3_OR_NEWER
using UnityEngine.Scripting;
#endif

namespace Unity.Cloud.Common
{
    [Preserve]
    [DataContract]
    struct ProjectDescriptorDto
    {
        [DataMember(Name = "organizationId")]
        [RequiredMember]
        public string OrganizationId { get; set; }

        [DataMember(Name = "projectId")]
        [RequiredMember]
        public string ProjectId { get; set; }

        [RequiredMember]
        public ProjectDescriptorDto(ProjectDescriptor projectDescriptor)
        {
            OrganizationId = projectDescriptor.OrganizationId.ToString();
            ProjectId = projectDescriptor.ProjectId.ToString();
        }
    }
}
