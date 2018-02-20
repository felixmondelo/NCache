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

using System;

namespace Alachisoft.NCache.Web.Command
{
    [Serializable]
    public class ReadFromStreamCommandInfo
    {
        public long requestId;
        public string key;
        public string lockHandle;
        public int offset;
        public int length;

        public ReadFromStreamCommandInfo()
        {
        }

        public ReadFromStreamCommandInfo(string key, string lockHandle, int offset, int length)
        {
            this.lockHandle = lockHandle;
            this.key = key;
            this.offset = offset;
            this.length = length;
        }
    }
}