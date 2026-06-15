using System.Runtime.Serialization;
#if UNITY_2022_3_OR_NEWER
using UnityEngine.Scripting;
#endif

namespace Unity.Cloud.Common
{
    [Preserve]
    [DataContract]
    struct DatasetDescriptorDto
    {
        [DataMember(Name = "assetDescriptor")]
        [RequiredMember]
        public AssetDescriptorDto AssetDescriptor { get; set; }

        [DataMember(Name = "datasetId")]
        [RequiredMember]
        public string DatasetId { get; set; }

        [RequiredMember]
        public DatasetDescriptorDto(DatasetDescriptor datasetDescriptor)
        {
            AssetDescriptor = new AssetDescriptorDto(datasetDescriptor.AssetDescriptor);
            DatasetId = datasetDescriptor.DatasetId.ToString();
        }
    }
}
