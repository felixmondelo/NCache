// Copyright (c) 2018 Alachisoft
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Alachisoft.NCache.Common.Net;
#if JAVA
using Alachisoft.TayzGrid.Runtime.Serialization;
#else
using Alachisoft.NCache.Runtime.Serialization;
#endif
#if JAVA
using Runtime = Alachisoft.TayzGrid.Runtime;
#else
using Runtime = Alachisoft.NCache.Runtime;
#endif
namespace Alachisoft.NCache.Common.Monitoring
{   
    /// <summary>
    /// Node represent a physical machine participating either as server
    /// or client.
    /// </summary>
    [Serializable]
    public class Node: ICompactSerializable
    {
        private string _name;
        private Address _address;

        public Node() { }

        public Node(string name, Address address)
        {
            Name = name;
            Address = address;
        }

        /// <summary>
        /// Gets/Sets the name of the node.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set 
            {
                if(value != null)
                    _name = value.ToLower(); 
            }
        }

        
        /// <summary>
        /// Gets/Sets the IPAddress of the node.
        /// </summary>
        public Address Address
        {
            get { return _address; }
            set { _address = value; }
        }

        public override bool Equals(object obj)
        {
            Node other = obj as Node;
            bool equal =false;
            if (other != null)
            {
                if (Name == null && other.Name == null) equal = true;
                if (equal)
                {
                    equal = false;
                    if (Address == null && other.Address == null)
                        equal = true;
                    if (Address != null && other.Address != null && Address.Equals(other.Address))
                        equal = true;
                }
                
            }
            return equal;
        }

        #region ICompactSerializable Members

        public void Deserialize(Runtime.Serialization.IO.CompactReader reader)
        {
            _name = reader.ReadObject() as string;
            _address = reader.ReadObject() as Address;
        }

        public void Serialize(Runtime.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(_name);
            writer.WriteObject(_address);
        }

        #endregion
    }


}
