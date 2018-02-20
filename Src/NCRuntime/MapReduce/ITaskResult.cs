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
using System.Collections;

namespace Alachisoft.NCache.Runtime.MapReduce
{
    /// <summary>
    /// Returns the result of Map Reduce Task result form server.
    /// </summary>
    public interface ITaskResult
    {
        /// <summary>
        /// Obtain the result in form of dictionary.
        /// </summary>
        /// <returns>dictionary containing the MapReduce result</returns>
        IDictionaryEnumerator GetEnumerator();
        /// <summary>
        /// Return status of Task which can be failure, success or cancelled.
        /// </summary>
        TaskCompletionStatus TaskStatus
        { get; }
        /// <summary>
        /// Returns reason behind the failure of task. 
        /// </summary>
        string TaskFailureReason
        { get; }
    }
}
