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

// Generated from: ItemRemoveCallbackResponse.proto
// Note: requires additional types generated from: EventId.proto
namespace Alachisoft.NCache.Common.Protobuf
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"ItemRemoveCallbackResponse")]
  public partial class ItemRemoveCallbackResponse : global::ProtoBuf.IExtensible
  {
    public ItemRemoveCallbackResponse() {}
    

    private string _key = "";
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"key", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string key
    {
      get { return _key; }
      set { _key = value; }
    }

    private int _itemRemoveReason = default(int);
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"itemRemoveReason", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int itemRemoveReason
    {
      get { return _itemRemoveReason; }
      set { _itemRemoveReason = value; }
    }

    private int _flag = default(int);
    [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name=@"flag", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int flag
    {
      get { return _flag; }
      set { _flag = value; }
    }
    private readonly global::System.Collections.Generic.List<byte[]> _value = new global::System.Collections.Generic.List<byte[]>();
    [global::ProtoBuf.ProtoMember(4, Name=@"value", DataFormat = global::ProtoBuf.DataFormat.Default)]
    public global::System.Collections.Generic.List<byte[]> value
    {
      get { return _value; }
    }
  

    private int _callbackId = default(int);
    [global::ProtoBuf.ProtoMember(5, IsRequired = false, Name=@"callbackId", DataFormat = global::ProtoBuf.DataFormat.ZigZag)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int callbackId
    {
      get { return _callbackId; }
      set { _callbackId = value; }
    }

    private Alachisoft.NCache.Common.Protobuf.EventId _eventId = null;
    [global::ProtoBuf.ProtoMember(6, IsRequired = false, Name=@"eventId", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(null)]
    public Alachisoft.NCache.Common.Protobuf.EventId eventId
    {
      get { return _eventId; }
      set { _eventId = value; }
    }

    private int _dataFilter = default(int);
    [global::ProtoBuf.ProtoMember(7, IsRequired = false, Name=@"dataFilter", DataFormat = global::ProtoBuf.DataFormat.ZigZag)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int dataFilter
    {
      get { return _dataFilter; }
      set { _dataFilter = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}
