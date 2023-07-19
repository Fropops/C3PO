﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinarySerializer;

namespace Shared
{
    public class AgentTask
    {
        [FieldOrder(0)]
        public string Id { get; set; }
        [FieldOrder(1)]
        public CommandId CommandId { get; set; }
        [FieldOrder(2)]
        public ParameterDictionary Parameters { get; set; } = new ParameterDictionary();


        public bool HasParameter(ParameterId id)
        {
            if(Parameters == null)
                return false;
            return Parameters.ContainsKey(id);
        }
        public T GetParameter<T>(ParameterId id)
        {
            return Parameters[id].BinaryDeserializeAsync<T>().Result;
        }
    }
}
