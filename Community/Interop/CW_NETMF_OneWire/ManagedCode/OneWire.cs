//---------------------------------------------------------------------------
//<copyright file="OneWire.cs">
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
  using System.Runtime.CompilerServices;
  using Microsoft.SPOT.Hardware;

  /// <summary>
  /// Provides 1-Wire master interface functionality.
  /// </summary>
  public class OneWire : IDisposable
  {
    private bool disposed;

  #pragma warning disable 649 // Field is never assigned to
    private Cpu.Pin pin;      // It is initialized in the extern constructor
  #pragma warning restore 649

    /// <summary>
    /// Initializes a new instance of the <see cref="OneWire"/> class.
    /// </summary>
    /// <param name="portId">The identifier (ID) for the port.</param>
    /// <remarks>
    /// The method delays for the required reset recovery time (480 µs)
    /// to make sure the first rising edge is not interpreted by a slave
    /// device as the end of a reset pulse.
    /// </remarks>
    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern OneWire(Cpu.Pin portId);

    /// <summary>
    /// Deletes an instance of the <see cref="OneWire"/> class.
    /// </summary>
    ~OneWire()
    {
      Dispose(false);
    }

    /// <summary>
    /// Releases resources used by this <see cref="OneWire"/> object.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the pin associated with the <see cref="OneWire"/> object
    /// and marks it as available for reuse.
    /// </summary>
    /// <param name="disposing">
    /// <b>true</b> to release both managed and unmanaged resources;
    /// <b>false</b> to release only unmanaged resources.
    /// </param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    protected void Dispose(bool disposing)
    {
      if(!disposed)
      {
        try
        {
          if(pin != Cpu.Pin.GPIO_NONE)
          {
            Port.ReservePin(pin, false);
          }
        }
        finally
        {
          disposed = true;
        }
      }
    }

    /// <summary>
    /// Initiates transmission on the 1-Wire bus.
    /// </summary>
    /// <returns>
    /// <b>true</b> if one or more devices are present on the bus;
    /// <b>false</b> if no devices were detected on the bus.
    /// </returns>
    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern bool Reset();

    /// <summary>
    /// Reads a sequence of bytes from the bus.
    /// </summary>
    /// <param name="buffer">
    /// An array of bytes. When this method returns, the <paramref name="buffer"/>
    /// contains the specified byte array with the values between <paramref name="index"/>
    /// and (<paramref name="index"/> + <paramref name="count"/> - 1) replaced
    /// by the bytes read from the bus.
    /// </param>
    /// <param name="index">
    /// The zero-based byte index in <paramref name="buffer"/> at which
    /// to begin storing the data read from the bus.
    /// </param>
    /// <param name="count">
    /// The maximum number of bytes to be read from the bus, or -1 to read
    /// from <paramref name="index"/> to the end of <paramref name="buffer"/>.
    /// </param>
    /// <returns>
    /// The total number of bytes read into the buffer. This is always the
    /// number of bytes requested, if there is no device on the bus the byte
    /// read is 0xFF.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// The sum of <paramref name="index"/> and <paramref name="count"/>
    /// is larger than the <paramref name="buffer"/> length.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="buffer"/> is null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative or <paramref name="count"/> is less than -1.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// The method was called after the object was disposed.
    /// </exception>
    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern int Read(byte[] buffer, int index = 0, int count = -1);

    /// <summary>
    /// Reads a bit from the bus.
    /// </summary>
    /// <returns>
    /// The byte that has the least significant bit (LSB) set to the value read
    /// from the bus. If there is no device on the bus the return value is 0x01.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// The method was called after the object was disposed.
    /// </exception>
    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern byte ReadBit();

    /// <summary>
    /// Reads a byte from the bus.
    /// </summary>
    /// <returns>
    /// The byte read. If there is no device on the bus the return value is 0xFF.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// The method was called after the object was disposed.
    /// </exception>
    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern byte ReadByte();

    /// <summary>
    /// Writes a sequence of bytes onto the bus.
    /// </summary>
    /// <param name="buffer">
    /// An array of bytes. This method writes <paramref name="count"/> bytes
    /// onto the bus.
    /// </param>
    /// <param name="index">
    /// The zero-based byte index in <paramref name="buffer"/> at which
    /// to begin writing bytes onto the bus.
    /// </param>
    /// <param name="count">
    /// The number of bytes to be written onto the bus, or -1 to write bytes
    /// from <paramref name="index"/> to the end of <paramref name="buffer"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// The sum of <paramref name="index"/> and <paramref name="count"/>
    /// is larger than the <paramref name="buffer"/> length.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="buffer"/> is null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative or <paramref name="count"/> is less than -1.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// The method was called after the object was disposed.
    /// </exception>
    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern void Write(byte[] buffer, int index = 0, int count = -1);

    /// <summary>
    /// Writes a bit onto the bus.
    /// </summary>
    /// <param name="value">
    /// The bit to write onto the bus; the value is converted to boolean.
    /// </param>
    /// <exception cref="ObjectDisposedException">
    /// The method was called after the object was disposed.
    /// </exception>
    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern void WriteBit(byte value);

    /// <summary>
    /// Writes a byte onto the bus.
    /// </summary>
    /// <param name="value">
    /// The byte to write onto the bus.
    /// </param>
    /// <exception cref="ObjectDisposedException">
    /// The method was called after the object was disposed.
    /// </exception>
    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern void WriteByte(byte value);

    /// <summary>
    /// Performs search operations on the 1-Wire bus.
    /// </summary>
    /// <param name="pattern">
    /// An array of bytes. When this method returns, the <paramref name="pattern"/>
    /// contains the specified byte array with the values between <paramref name="index"/>
    /// and (<paramref name="index"/> + 7) replaced with the identifier of a device
    /// discovered on the bus. Passing the value from previous successful search
    /// performs discovery of the next device.
    /// </param>
    /// <param name="deviation">
    /// The bit position where the search made a choice the last time it was
    /// run. This argument is 0 when a search is initiated, or the return
    /// value from previous successful search.
    /// </param>
    /// <param name="index">
    /// The zero-based byte index in <paramref name="pattern"/> at which
    /// to begin storing the device identifier.
    /// </param>
    /// <returns>
    /// -1 if the search failed (e.g. a device was connected to the bus
    /// during the search), 0 if there are no more devices to be discovered,
    /// otherwise the value of last <paramref name="deviation"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// The number of bytes between <paramref name="index"/> and the end of
    /// <paramref name="pattern"/> is less than 8.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="pattern"/> is null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// The method was called after the object was disposed.
    /// </exception>
    /// <remarks>
    /// When one search has been performed, all slaves except of one should
    /// have entered an idle state and it is now possible to communicate with
    /// the active slave without specifically addressing it.
    /// </remarks>
    [MethodImpl(MethodImplOptions.InternalCall)]
    public extern int Search(byte[] pattern, int deviation, int index = 0);

    /// <summary>
    /// Computes 8-bit cyclic redundancy check (CRC) using 1-Wire specification.
    /// </summary>
    /// <param name="buffer">
    /// An array of bytes from which the CRC is computed.
    /// </param>
    /// <param name="index">
    /// The zero-based byte index in <paramref name="buffer"/> at which
    /// to begin the calculation.
    /// </param>
    /// <param name="count">
    /// The number of bytes to be included in calculation, or -1 to include bytes
    /// from <paramref name="index"/> to the end of <paramref name="buffer"/>.
    /// </param>
    /// <param name="seed">
    /// A seed for the CRC calculation. Constantly passing the return value
    /// of this method as the seed argument computes the CRC value of a longer
    /// block of data.
    /// </param>
    /// <returns>
    /// Computed CRC value.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// The sum of <paramref name="index"/> and <paramref name="count"/>
    /// is larger than the <paramref name="buffer"/> length.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="buffer"/> is null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative or <paramref name="count"/> is less than -1.
    /// </exception>
    /// <remarks>
    /// This method uses x^8 + x^5 + x^4 + 1 polynomial (CRC-8-Dallas/Maxim).
    /// </remarks>
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern byte ComputeCRC(byte[] buffer, int index = 0, int count = -1, byte seed = 0);

    /// <summary>
    /// Computes 16-bit cyclic redundancy check (CRC).
    /// </summary>
    /// <param name="buffer">
    /// An array of bytes from which the CRC is computed.
    /// </param>
    /// <param name="index">
    /// The zero-based byte index in <paramref name="buffer"/> at which
    /// to begin the calculation.
    /// </param>
    /// <param name="count">
    /// The number of bytes to be included in calculation, or -1 to include bytes
    /// from <paramref name="index"/> to the end of <paramref name="buffer"/>.
    /// </param>
    /// <param name="seed">
    /// A seed for the CRC calculation. Constantly passing the return value
    /// of this method as the seed argument computes the CRC value of a longer
    /// block of data.
    /// </param>
    /// <returns>
    /// Computed CRC value.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// The sum of <paramref name="index"/> and <paramref name="count"/>
    /// is larger than the <paramref name="buffer"/> length.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="buffer"/> is null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative or <paramref name="count"/> is less than -1.
    /// </exception>
    /// <remarks>
    /// This method uses x^16 + x^15 + x^2 + 1 polynomial (CRC-16, also known
    /// as CRC-16-ANSI and CRC-16-IBM).
    /// </remarks>
    [MethodImpl(MethodImplOptions.InternalCall)]
    public static extern ushort ComputeCRC16(byte[] buffer, int index = 0, int count = -1, ushort seed = 0);


    // ROM Function Commands

    /// <summary>
    /// The command used to read the unique identifier of a single slave device.
    /// </summary>
    public const byte ReadRom = 0x33;

    /// <summary>
    /// The command used for addressing when no specific slave is targeted.
    /// </summary>
    public const byte SkipRom = 0xCC;

    /// <summary>
    /// The command used to address individual slave device on the bus.
    /// </summary>
    /// <remarks>
    /// The command followed by a 64-bit unique identifier allows the bus master
    /// to address a specific slave device. Only the slave that exactly matches
    /// the 64-bit identifier will respond to the function command issued by
    /// the master; all other slaves on the bus will wait for a reset pulse.
    /// </remarks>
    public const byte MatchRom = 0x55;

    /// <summary>
    /// The command used to perform search operations on the bus.
    /// </summary>
    public const byte SearchRom = 0xF0;
  }
}
