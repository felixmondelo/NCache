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
// limitations under the License.

namespace Alachisoft.NCache.Common.Protobuf
{
    [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"FileDependency")]
    public partial class FileDependency : global::ProtoBuf.IExtensible
    {
        public FileDependency() {}
    
        private readonly global::System.Collections.Generic.List<string> _filePaths = new global::System.Collections.Generic.List<string>();
        [global::ProtoBuf.ProtoMember(1, Name=@"filePaths", DataFormat = global::ProtoBuf.DataFormat.Default)]
        public global::System.Collections.Generic.List<string> filePaths
        {
            get { return _filePaths; }
        }
  

        private long _startAfter = default(long);
        [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"startAfter", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(long))]
        public long startAfter
        {
            get { return _startAfter; }
            set { _startAfter = value; }
        }
        private global::ProtoBuf.IExtension extensionObject;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
    }
}