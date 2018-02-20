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
// limitations under the License

using System;
using System.Collections.Generic;
using System.Text;
using Alachisoft.NCache.Common.AssemblyBrowser;

namespace Alachisoft.NCache.Common
{
    public interface IDiscoverAssembly
    {
        string[] GetClassTypeNames(string assemblyPath);

        string[] GetClassTypeNames(string assemblyPath, ClassTypeFilter filter);

        string[] GetClassTypeNames(string[] assemblyPaths);       

        string[] GetClassTypeNames(string[] assemblyPaths, ClassTypeFilter filter);

        Type[] GetTypes(string assemblyPath);

        Type[] GetTypes(string assemblyPath, ClassTypeFilter filter);

        Dictionary<string, Type[]> GetTypes(string[] assemblyPaths);

        Dictionary<string, Type[]> GetTypes(string[] assemblyPaths, ClassTypeFilter filter);

        string AssemblyFullName(string assemblyPath);

        string[] AssemblyFullName(string[] assemblyPath);
    }
}
