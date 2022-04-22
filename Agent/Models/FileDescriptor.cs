﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Models
{
    [DataContract]
    public class FileDescriptor
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "length")]
        public long Length { get; set; }
        [DataMember(Name = "chunkSize")]
        public int ChunkSize { get; set; }
        [DataMember(Name = "chunkCount")]
        public int ChunkCount { get; set; }
    }
}
