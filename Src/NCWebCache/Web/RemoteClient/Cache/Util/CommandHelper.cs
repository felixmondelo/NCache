﻿// Copyright (c) 2018 Alachisoft
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
using Alachisoft.NCache.Runtime.Caching;

namespace Alachisoft.NCache.Web.Caching.Util
{
    internal class CommandHelper
    {
        public static bool IsIndexable(Type type)
        {
            return (type.IsPrimitive || type.Equals(typeof(string)) || type.Equals(typeof(DateTime)) || type.Equals(typeof(Decimal)) || type.Equals(typeof(Decimal)));
        }
        public static bool IsTag(Type type)
        {
            return type.Equals(typeof(Tag));
        }
    }
}
