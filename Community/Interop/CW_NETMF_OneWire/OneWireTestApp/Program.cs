//---------------------------------------------------------------------------
//<copyright file="Program.cs">
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
namespace OneWireTestApp
{
  using System;
  using System.Threading;
  using CW.NETMF.Hardware;
  using Microsoft.SPOT;
  using Microsoft.SPOT.Hardware;
  using SecretLabs.NETMF.Hardware.Netduino;

  /// <summary>
  /// This application demonstrates usage of various 1-Wire commands.
  /// </summary>
  public class Program
  {
    private static OneWire oneWire;
    private static InterruptPort sw1;

    public static void Main()
    {
      sw1 = new InterruptPort(Pins.ONBOARD_SW1, true, ResistorModes.Disabled, InterruptModes.InterruptEdgeLow);
      sw1.OnInterrupt += new NativeEventHandler(sw1_OnInterrupt);

      // TODO: Change pin according to the actual wiring
      oneWire = new OneWire(Pins.GPIO_PIN_D0);

      Thread.Sleep(Timeout.Infinite);
    }

    private static void sw1_OnInterrupt(uint portId, uint state, DateTime time)
    {
      //---------------------------------------------------------------------
      // Reset/Presence
      if(oneWire.Reset())
      {
        Debug.Print("1-Wire device present");
      }
      else
      {
        Debug.Print("1-Wire device NOT present");
      }

      var rom = new byte[8];

      //---------------------------------------------------------------------
      // Read ROM
      if(oneWire.Reset())
      {
        oneWire.WriteByte(OneWire.ReadRom);
        oneWire.Read(rom);
        if(OneWire.ComputeCRC(rom, count:7) != rom[7])
        {
          // Failed CRC indicates presence of multiple slave devices on the bus
          Debug.Print("Multiple devices present");
        }
        else
        {
          Debug.Print("Single device present");
        }
      }

      //---------------------------------------------------------------------
      // Search ROM: First & Next (Enumerate all devices)
      var deviation = 0;  // Search result
      do
      {
        if((deviation = oneWire.Search(rom, deviation)) == -1)
          break;
        if(OneWire.ComputeCRC(rom, count:7) == rom[7])
        {
          Debug.Print(OneWireExtensions.BytesToHexString(rom));
        }
      }
      while(deviation > 0);

      //---------------------------------------------------------------------
      // Search ROM: Verify (Is device with a known ROM present on the bus?)
      if(deviation == 0)
      {
        // Previous search succeeded, search for the last device
      }
      else
      {
        // TODO: Fill rom with the value to search for
      }
      var foundRom = new byte[8];
      Array.Copy(rom, foundRom, 8); // Save the ROM for comparison
      if(oneWire.Search(foundRom, 64) == 0)
      {
        // If the search was successful and the foundRom remains the ROM number
        // that was being searched for, then the device is currently on the bus.
        if(rom[7] == foundRom[7]) // Comparing CRC is enough for test purposes
        {
          Debug.Print("Device is on the bus");

          // It is now possible to communicate with the active slave without
          // specifically addressing it
          if(rom[0] == DS18B20.FamilyCode)
          {
            var scratchpad = new byte[9];
            oneWire.WriteByte(DS18B20.ReadScratchpad);
            oneWire.Read(scratchpad);
            if(OneWire.ComputeCRC(scratchpad, count:8) == scratchpad[8])
            {
              // Default scratchpad content (assignments just for reference)
              scratchpad[0] = 0x50; // Temperature LSB
              scratchpad[1] = 0x05; // Temperature MSB
              scratchpad[5] = 0xFF; // Reserved
              scratchpad[7] = 0x10; // Reserved
            }
          }
        }
      }

      //---------------------------------------------------------------------
      // DS18B20 Programmable Resolution Digital Thermometer
      if(rom[0] == DS18B20.FamilyCode)
      {
        oneWire.Reset();
        oneWire.WriteByte(OneWire.SkipRom); // Address all devices
        oneWire.WriteByte(DS18B20.ConvertT);
        Thread.Sleep(750);  // Wait Tconv (for default 12-bit resolution)

        // Write command and identifier at once
        var matchRom = new byte[9];
        Array.Copy(rom, 0, matchRom, 1, 8);
        matchRom[0] = OneWire.MatchRom;

        oneWire.Reset();
        oneWire.Write(matchRom);
        oneWire.WriteByte(DS18B20.ReadScratchpad);

        // Read just the temperature (2 bytes)
        var tempLo = oneWire.ReadByte();
        var tempHi = oneWire.ReadByte();
        Debug.Print(DS18B20.GetTemperature(tempLo, tempHi).ToString());

        // Read power supply mode
        oneWire.Reset();
        oneWire.Write(matchRom);
        oneWire.WriteByte(DS18B20.ReadPowerSupply);
        if(oneWire.ReadBit() == 0)
        {
          // Note: VDD must be connected to ground
          Debug.Print("Parasite powered");
        }
        else
        {
          // Powered by an external supply, supports progress feedback
          oneWire.Reset();
          oneWire.Write(matchRom);
          oneWire.WriteByte(DS18B20.ConvertT);
          while(oneWire.ReadBit() == 0) { };

          oneWire.Reset();
          oneWire.Write(matchRom);
          oneWire.WriteByte(DS18B20.ReadScratchpad);

          // Read just the temperature (2 bytes)
          tempLo = oneWire.ReadByte();
          tempHi = oneWire.ReadByte();
          Debug.Print(DS18B20.GetTemperature(tempLo, tempHi).ToString());
        }
      }
    }
  }
}
