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

// Generated from: NodeLeftEventResponse.proto
// Note: requires additional types generated from: EventId.proto
namespace Alachisoft.NCache.Common.Protobuf
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"NodeLeftEventResponse")]
  public partial class NodeLeftEventResponse : global::ProtoBuf.IExtensible
  {
    public NodeLeftEventResponse() {}
    

    private string _clusterIp = "";
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"clusterIp", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string clusterIp
    {
      get { return _clusterIp; }
      set { _clusterIp = value; }
    }

    private string _clusterPort = "";
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"clusterPort", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string clusterPort
    {
      get { return _clusterPort; }
      set { _clusterPort = value; }
    }

    private string _serverIp = "";
    [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name=@"serverIp", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string serverIp
    {
      get { return _serverIp; }
      set { _serverIp = value; }
    }

    private string _serverPort = "";
    [global::ProtoBuf.ProtoMember(4, IsRequired = false, Name=@"serverPort", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string serverPort
    {
      get { return _serverPort; }
      set { _serverPort = value; }
    }

    private Alachisoft.NCache.Common.Protobuf.EventId _eventId = null;
    [global::ProtoBuf.ProtoMember(5, IsRequired = false, Name=@"eventId", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(null)]
    public Alachisoft.NCache.Common.Protobuf.EventId eventId
    {
      get { return _eventId; }
      set { _eventId = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}
