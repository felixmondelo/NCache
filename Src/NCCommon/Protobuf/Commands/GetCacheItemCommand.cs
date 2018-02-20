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

// Generated from: GetCacheItemCommand.proto
// Note: requires additional types generated from: LockInfo.proto
namespace Alachisoft.NCache.Common.Protobuf
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"GetCacheItemCommand")]
  public partial class GetCacheItemCommand : global::ProtoBuf.IExtensible
  {
    public GetCacheItemCommand() {}
    

    private string _key = "";
    [global::ProtoBuf.ProtoMember(1, IsRequired = false, Name=@"key", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string key
    {
      get { return _key; }
      set { _key = value; }
    }

    private long _requestId = default(long);
    [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name=@"requestId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(long))]
    public long requestId
    {
      get { return _requestId; }
      set { _requestId = value; }
    }

    private string _group = "";
    [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name=@"group", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string group
    {
      get { return _group; }
      set { _group = value; }
    }

    private string _subGroup = "";
    [global::ProtoBuf.ProtoMember(4, IsRequired = false, Name=@"subGroup", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string subGroup
    {
      get { return _subGroup; }
      set { _subGroup = value; }
    }

    private Alachisoft.NCache.Common.Protobuf.LockInfo _lockInfo = null;
    [global::ProtoBuf.ProtoMember(5, IsRequired = false, Name=@"lockInfo", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue(null)]
    public Alachisoft.NCache.Common.Protobuf.LockInfo lockInfo
    {
      get { return _lockInfo; }
      set { _lockInfo = value; }
    }

    private int _flag = default(int);
    [global::ProtoBuf.ProtoMember(6, IsRequired = false, Name=@"flag", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(int))]
    public int flag
    {
      get { return _flag; }
      set { _flag = value; }
    }

    private ulong _version = default(ulong);
    [global::ProtoBuf.ProtoMember(7, IsRequired = false, Name=@"version", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    [global::System.ComponentModel.DefaultValue(default(ulong))]
    public ulong version
    {
      get { return _version; }
      set { _version = value; }
    }

    private string _providerName = "";
    [global::ProtoBuf.ProtoMember(8, IsRequired = false, Name=@"providerName", DataFormat = global::ProtoBuf.DataFormat.Default)]
    [global::System.ComponentModel.DefaultValue("")]
    public string providerName
    {
      get { return _providerName; }
      set { _providerName = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}
