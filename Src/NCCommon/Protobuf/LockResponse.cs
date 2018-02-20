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


//------------------------------------------------------------------------------
// 
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// 
//------------------------------------------------------------------------------

// Generated from: LockResponse.proto
namespace Alachisoft.NCache.Common.Protobuf
{
    [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"LockResponse")]
    public partial class LockResponse : global::ProtoBuf.IExtensible
    {
      public LockResponse() {}
      

    private bool _locked = default(bool);
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"locked", DataFormat = global::ProtoBuf.DataFormat.Default)][global::System.ComponentModel.DefaultValue(default(bool))]
    public bool locked
    {
      get { return _locked; }
      set { _locked = value; }
    }

    private string _lockId = "";
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"lockId", DataFormat = global::ProtoBuf.DataFormat.Default)][global::System.ComponentModel.DefaultValue("")]
    public string lockId
    {
      get { return _lockId; }
      set { _lockId = value; }
    }

    private long _lockTime = default(long);
    [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name=@"lockTime", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)][global::System.ComponentModel.DefaultValue(default(long))]
    public long lockTime
    {
      get { return _lockTime; }
      set { _lockTime = value; }
    }
      private global::ProtoBuf.IExtension extensionObject;
      global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
    }
  
}
