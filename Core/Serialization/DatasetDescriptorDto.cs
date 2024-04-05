using System.Runtime.Serialization;

namespace Unity.Cloud.Common
{
    [DataContract]
    struct DatasetDescriptorDto
    {
        [DataMember(Name = "assetDescriptor")]
        public AssetDescriptorDto AssetDescriptor { get; set; }

        [DataMember(Name = "datasetId")]
        public string DatasetId { get; set; }

        public DatasetDescriptorDto(DatasetDescriptor datasetDescriptor)
        {
            AssetDescriptor = new AssetDescriptorDto(datasetDescriptor.AssetDescriptor);
            DatasetId = datasetDescriptor.DatasetId.ToString();
        }
    }
}
