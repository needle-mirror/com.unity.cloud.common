using System.Runtime.Serialization;
#if UNITY_2022_3_OR_NEWER
using UnityEngine.Scripting;
#endif

namespace Unity.Cloud.Common
{
    [Preserve]
    [DataContract]
    struct AssetDescriptorDto
    {
        [DataMember(Name = "projectDescriptor")]
        [RequiredMember]
        public ProjectDescriptorDto ProjectDescriptor { get; set; }

        [DataMember(Name = "assetId")]
        [RequiredMember]
        public string AssetId { get; set; }

        [DataMember(Name = "assetVersion")]
        [RequiredMember]
        public string AssetVersion { get; set; }

        [DataMember(Name = "libraryId")]
        [RequiredMember]

        public string AssetLibraryId { get; set; }

        [RequiredMember]
        public AssetDescriptorDto(AssetDescriptor assetDescriptor)
        {
            ProjectDescriptor = new ProjectDescriptorDto(assetDescriptor.ProjectDescriptor);
            AssetId = assetDescriptor.AssetId.ToString();
            AssetVersion = assetDescriptor.AssetVersion.ToString();
            AssetLibraryId = assetDescriptor.AssetLibraryId.ToString();
        }
    }
}
