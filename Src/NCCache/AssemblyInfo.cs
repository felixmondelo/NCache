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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]


#if NETCORE
[assembly: AssemblyTitle("Alachisoft.NCache.Cache (.NETCore)")]
#else
[assembly: AssemblyTitle("Alachisoft.NCache.Cache")]
#endif

[assembly: AssemblyProduct("Alachisoft® NCache Open Source")]


[assembly: AssemblyTrademark("NCache ™ is a registered trademark of Alachisoft.")]


[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Alachisoft")]



[assembly: AssemblyCopyright("Copyright © 2005-2018 Alachisoft")]
[assembly: AssemblyCulture("")]

[assembly: InternalsVisibleTo("EladLicenseGenerator,PublicKey=002400000480000094000000060200000024000052534131000400000100010005a3e761ae2217"
+ "0e7f5cc1208e5a2e51fef749c98ee0cc3c94dc1d688fe0324370d327bb3e33248ad603831c8b5b"
+ "7316c451e26b5fcb99ec05884419f7102942e7446a51e0c5812530af21c49330e45baaba4247cb"
+ "07f4807a1d051466040c77d437fb79ffe78a2330d4d5a6830577b98907cba0365ced3f9c4bb91f"
+ "b9520bc9")]
//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

[assembly: AssemblyVersion("4.9.0")]

//
// In order to sign your assembly you must specify a key to use. Refer to the 
// Microsoft .NET Framework documentation for more information on assembly signing.
//
// Use the attributes below to control which key is used for signing. 
//
// Notes: 
//   (*) If no key is specified, the assembly is not signed.
//   (*) KeyName refers to a key that has been installed in the Crypto Service
//       Provider (CSP) on your machine. KeyFile refers to a file which contains
//       a key.
//   (*) If the KeyFile and the KeyName values are both specified, the 
//       following processing occurs:
//       (1) If the KeyName can be found in the CSP, that key is used.
//       (2) If the KeyName does not exist and the KeyFile does exist, the key 
//           in the KeyFile is installed into the CSP and used.
//   (*) In order to create a KeyFile, you can use the sn.exe (Strong Name) utility.
//       When specifying the KeyFile, the location of the KeyFile should be
//       relative to the project output directory which is
//       %Project Directory%\obj\<configuration>. For example, if your KeyFile is
//       located in the project directory, you would specify the AssemblyKeyFile 
//       attribute as [assembly: AssemblyKeyFile("..\\..\\mykey.snk")]
//   (*) Delay Signing is an advanced option - see the Microsoft .NET Framework
//       documentation for more information on this.
//
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyName("")]
#if DEBUG
[assembly: AssemblyKeyFile("..\\..\\..\\..\\Resources\\ncache.snk")]
#else
[assembly: AssemblyKeyFile("..\\..\\..\\..\\Resources\\ncache.snk")]
#endif
[assembly: AssemblyDescriptionAttribute("Cache Core")]
[assembly: AssemblyFileVersionAttribute("4.9.0.0")]
[assembly: AssemblyInformationalVersion("4.9.0")]
