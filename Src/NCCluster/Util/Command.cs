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
// $Id: Command.java,v 1.1.1.1 2003/09/09 01:24:12 belaban Exp $
using System;

namespace Alachisoft.NGroups.Util
{
	
	/// <summary> The Command patttern (se Gamma et al.). Implementations would provide their
	/// own <code>execute</code> method.
	/// </summary>
	/// <author>  Bela Ban
	/// </author>
	internal interface Command
	{
		bool execute();
	}
}