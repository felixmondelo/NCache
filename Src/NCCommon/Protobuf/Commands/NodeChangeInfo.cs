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
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: NodeChangeInfo.proto
namespace Alachisoft.NCache.Common.Protobuf
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"NodeChangeInfo")]
  public partial class NodeChangeInfo : global::ProtoBuf.IExtensible
  {
    public NodeChangeInfo() {}
    

    private string _node = "";
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"node", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string node
    {
      get { return _node; }
      set { _node = value; }
    }
    private readonly global::System.Collections.Generic.List<string> _keys = new global::System.Collections.Generic.List<string>();
    [global::ProtoBuf.ProtoMember(2, Name=@"keys", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<string> keys
    {
      get { return _keys; }
    }
  
    private readonly global::System.Collections.Generic.List<int> _changeType = new global::System.Collections.Generic.List<int>();
    [global::ProtoBuf.ProtoMember(3, Name=@"changeType", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public global::System.Collections.Generic.List<int> changeType
    {
      get { return _changeType; }
    }
  
    private readonly global::System.Collections.Generic.List<int> _changeIds = new global::System.Collections.Generic.List<int>();
    [global::ProtoBuf.ProtoMember(4, Name=@"changeIds", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public global::System.Collections.Generic.List<int> changeIds
    {
      get { return _changeIds; }
    }
  
    private readonly global::System.Collections.Generic.List<byte[]> _value = new global::System.Collections.Generic.List<byte[]>();
    [global::ProtoBuf.ProtoMember(5, Name=@"value", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<byte[]> value
    {
      get { return _value; }
    }
  
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}
