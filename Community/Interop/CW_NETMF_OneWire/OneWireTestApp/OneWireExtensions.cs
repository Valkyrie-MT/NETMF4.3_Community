//---------------------------------------------------------------------------
//<copyright file="OneWireExtensions.cs">
//
// Copyright 2010 Stanislav "CW" Simicek
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
//</copyright>
//---------------------------------------------------------------------------
namespace CW.NETMF.Hardware
{
  using System;

  /// <summary>
  /// 1-Wire extension and helper methods
  /// </summary>
  internal static class OneWireExtensions
  {
    public static bool IsValid(byte[] rom)
    {
      if(rom == null)
      {
        throw new ArgumentNullException();
      }
      if(rom.Length != 8)
      {
        throw new ArgumentException();
      }
      var crc = OneWire.ComputeCRC(rom, count:7);
      return crc == rom[7];
    }

    private static string hexDigits = "0123456789ABCDEF";

    public static string BytesToHexString(byte[] buffer)
    {
      var chars = new char[buffer.Length*2];
      for(int i = buffer.Length - 1, c = 0; i >= 0; i--)
      {
        chars[c++] = hexDigits[(buffer[i] & 0xF0) >> 4];
        chars[c++] = hexDigits[(buffer[i] & 0x0F)];
      }
      return new string(chars);
    }
  }
}

// TODO: Uncomment to support extension methods
//namespace System.Runtime.CompilerServices
//{
//  [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
//  public sealed class ExtensionAttribute : Attribute { }
//}
