//---------------------------------------------------------------------------
//<copyright file="DS18B20.cs">
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
  /// <summary>
  /// DS18B20 Programmable Resolution 1-Wire Digital Thermometer
  /// </summary>
  public static class DS18B20
  {
    public const byte FamilyCode = 0x28;

    // Commands
    public const byte ConvertT        = 0x44;
    public const byte CopyScratchpad  = 0x48;
    public const byte WriteScratchpad = 0x4E;
    public const byte ReadPowerSupply = 0xB4;
    public const byte RecallE2        = 0xB8;
    public const byte ReadScratchpad  = 0xBE;

    // Helpers
    public static float GetTemperature(byte tempLo, byte tempHi)
    {
      return ((short)((tempHi << 8) | tempLo))/16F;
    }
  }
}