using System.Runtime.Serialization;
#if UNITY_2022_3_OR_NEWER
using UnityEngine.Scripting;
#endif

namespace Unity.Cloud.Common
{
    [Preserve]
    [DataContract]
    struct FileDescriptorDto
    {
        [DataMember(Name = "datasetDescriptor")]
        [RequiredMember]
        public DatasetDescriptorDto DatasetDescriptor { get; set; }

        [DataMember(Name = "filePath")]
        [RequiredMember]
        public string FilePath { get; set; }

        [RequiredMember]
        public FileDescriptorDto(FileDescriptor fileDescriptor)
        {
            DatasetDescriptor = new DatasetDescriptorDto(fileDescriptor.DatasetDescriptor);
            FilePath = fileDescriptor.Path;
        }
    }
}
