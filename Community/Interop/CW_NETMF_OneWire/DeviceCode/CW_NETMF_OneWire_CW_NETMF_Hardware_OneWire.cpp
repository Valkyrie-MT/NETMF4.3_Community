//---------------------------------------------------------------------------
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
//---------------------------------------------------------------------------
//
// Dallas/Maxim 1-Wire Master
//
// 1-Wire Communication Through Software
// http://www.maxim-ic.com/app-notes/index.mvp/id/126
//
// 1-Wire Search Algorithm
// http://www.maxim-ic.com/app-notes/index.mvp/id/187
//
// Understanding and Using Cyclic Redundancy Checks with Maxim iButton Products
// http://www.maxim-ic.com/app-notes/index.mvp/id/27
//
// AVR318: Dallas 1-Wire master
// http://www.atmel.com/dyn/resources/prod_documents/doc2579.pdf
//

#include "CW_NETMF_OneWire.h"
#include "CW_NETMF_OneWire_CW_NETMF_Hardware_OneWire.h"
#include <TinyCLR_Runtime.h>

using namespace CW::NETMF::Hardware;

/////////////////////////////////////////////////////////////////////////////
// 1-Wire Standard Mode Delays
//
// The timing is a little bit tricky, because there is no function for accurate
// microsecond delays and HAL_Time_Sleep_MicroSeconds uses slow timer that has
// resolution ~21 us (see SLOW_CLOCKS_PER_SECOND in patform_selector_h). Thus,
// slow timer is used where possible (guarantees minimal delay), and calls
// of pin state functions participate in achieving short (<10 us) delays.
//

#define SLOW_CLOCK_US          (1000000/SLOW_CLOCKS_PER_SECOND)
#define SLOW_TIMER_DELAY(uSec) (((uSec/SLOW_CLOCK_US) + 4)*SLOW_CLOCK_US)

// Slow Timer Delays                                        Min   Rec   Max
//const UINT32 OneWire_Delay_A = -- Too short -- (  6);  //   5     6    15
  const UINT32 OneWire_Delay_B = SLOW_TIMER_DELAY( 63);  //  59    64   n/a
  const UINT32 OneWire_Delay_C = SLOW_TIMER_DELAY( 63);  //  60    60   120
//const UINT32 OneWire_Delay_D = -- Too short -- ( 10);  //   8    10   n/a
//const UINT32 OneWire_Delay_E = -- Too short -- (  9);  //   5     9    12
  const UINT32 OneWire_Delay_F = SLOW_TIMER_DELAY( 50);  //  50    55   n/a
//const UINT32 OneWire_Delay_G = -- Not used --- (  0);  //   0     0     0
  const UINT32 OneWire_Delay_H = SLOW_TIMER_DELAY(480);  // 480   480   640
  const UINT32 OneWire_Delay_I = SLOW_TIMER_DELAY( 63);  //  63    70    78
  const UINT32 OneWire_Delay_J = SLOW_TIMER_DELAY(410);  // 410   410   n/a

/////////////////////////////////////////////////////////////////////////////
// 1-Wire Basic Signal Functions
//
// These functions require precise timing, they are called by their
// respective interop wrappers from blocks with disabled interrupts.
//
// The calculation of short delays is based on measured performance
// of RELEASE code generated by RealView compiler.
#ifndef __ARMCC_VERSION
  //#error 1-Wire function timing must be adjusted for this compiler
#endif

static INT8 OneWire_Reset(UINT32 pin)
{
  // Pull bus down, wait
  ::CPU_GPIO_EnableOutputPin(pin, FALSE);
  HAL_Time_Sleep_MicroSeconds(OneWire_Delay_H);

  // Release bus, wait
  ::CPU_GPIO_EnableInputPin(pin, FALSE, NULL, GPIO_INT_NONE, RESISTOR_PULLUP);
  HAL_Time_Sleep_MicroSeconds(OneWire_Delay_I);

  // Sample, wait
  INT8 retVal = !::CPU_GPIO_GetPinState(pin);
  HAL_Time_Sleep_MicroSeconds(OneWire_Delay_J);
  return retVal;
}

static BOOL OneWire_ReadBit(UINT32 pin)
{
  // Pull bus down, wait
  ::CPU_GPIO_EnableOutputPin(pin, FALSE);   // ~3.8 us, LOW at ~2 us
  ::CPU_GPIO_SetPinState(pin, FALSE);       // ~3.3 us
  ::CPU_GPIO_SetPinState(pin, TRUE);        // ~3.3 us, HIGH at ~1.8 us
  // Delay A [LOW - HIGH]: (3.8 - 2) + 3.3 + (3.3 - 1.8) = ~6.6 us

  // Release bus, wait, sample, wait
  ::CPU_GPIO_EnableInputPin(pin, FALSE, NULL, GPIO_INT_NONE, RESISTOR_PULLUP);  // ~5.3 us
  BOOL bitRead = ::CPU_GPIO_GetPinState(pin); // ~1.7 us
  // Delay E [Sample at 15 us]: 6.6 + 5.3 + 1.7 = ~13.6 us
  // Average time measured to the middle of GetPinState function: ~14.5 us
  HAL_Time_Sleep_MicroSeconds(OneWire_Delay_F);
  return bitRead;
}

static UINT8 OneWire_ReadByte(UINT32 pin)
{
  UINT8 retVal = 0; 
  for(UINT8 bit = 1; bit != 0; bit <<= 1)
  {
    if(OneWire_ReadBit(pin))
    {
      retVal |= bit;
    }
  }
  return retVal;
}

static void OneWire_WriteBit(UINT32 pin, BOOL value)
{
  // Pull bus down, wait
  ::CPU_GPIO_EnableOutputPin(pin, FALSE);   // ~3.8 us, LOW at ~2 us
  if(value)
  {
    ::CPU_GPIO_SetPinState(pin, FALSE);     // ~3.3 us
    // Release bus, wait
    ::CPU_GPIO_SetPinState(pin, TRUE);      // ~3.3 us, HIGH at ~1.8 us
    // Delay A [LOW - HIGH]: (3.8 - 2) + 3.3 + (3.3 - 1.8) = ~6.6 us
    HAL_Time_Sleep_MicroSeconds(OneWire_Delay_B);
  }
  else
  {
    HAL_Time_Sleep_MicroSeconds(OneWire_Delay_C);
    // Release bus, wait
    ::CPU_GPIO_SetPinState(pin, TRUE);      // ~3.3 us, HIGH at ~1.8
    ::CPU_GPIO_EnableOutputPin(pin, TRUE);  // ~3.8 us
    ::CPU_GPIO_EnableOutputPin(pin, TRUE);  // ~3.8 us
    // Delay D [HIGH - HIGH]: (3.3 - 1.8) + 3.8 + 3.8 = ~9.1 us
  }
}

static inline void OneWire_WriteByte(UINT32 pin, UINT8 value)
{
  for(UINT8 bit = 1; bit != 0; bit <<= 1)
  {
    OneWire_WriteBit(pin, (value & bit));
  }
}

/////////////////////////////////////////////////////////////////////////////
// Interop
//
// For more detailed description of the functionality, parameters,
// return values, exceptions etc. see XMLDoc comments in OneWire.cs
//

void OneWire::_ctor(CLR_RT_HeapBlock* pMngObj, UINT32 portId, HRESULT &hr)
{
  // Initialize fields
  pMngObj[Library_CW_NETMF_OneWire_CW_NETMF_Hardware_OneWire::FIELD__pin].NumericByRef().u4 = GPIO_PIN_NONE;

  // TODO: Check pint input/output attributes

  if(!::CPU_GPIO_ReservePin(portId, TRUE))
  {
    hr = CLR_E_PIN_UNAVAILABLE; // System.Exception
    return;
  }

  // Store pin to member variable
  pMngObj[Library_CW_NETMF_OneWire_CW_NETMF_Hardware_OneWire::FIELD__pin].NumericByRef().u4 = portId;

  // The first rising edge can be interpreted by a slave as the end
  // of a Reset pulse. Delay for the required reset recovery time
  // to be sure that the real reset is interpreted correctly.
  GLOBAL_LOCK(irq);
  {
    ::CPU_GPIO_EnableOutputPin(portId, TRUE);
    HAL_Time_Sleep_MicroSeconds(OneWire_Delay_H);
  }
}


INT8 OneWire::Reset(CLR_RT_HeapBlock* pMngObj, HRESULT &hr)
{
  UINT32 pin = Get_pin(pMngObj);
  ASSERT(pin != GPIO_PIN_NONE);

  GLOBAL_LOCK(irq);

  INT8 retVal = OneWire_Reset(pin);
  return retVal;
}


INT32 OneWire::Read(CLR_RT_HeapBlock* pMngObj, CLR_RT_TypedArray_UINT8 buffer, INT32 index, INT32 count, HRESULT &hr)
{
  UINT32 pin = Get_pin(pMngObj);
  ASSERT(pin != GPIO_PIN_NONE);

  GLOBAL_LOCK(irq);
  {
    for(INT32 endIndex = index + count; index < endIndex; index++)
    {
      buffer[index] = OneWire_ReadByte(pin);
    }
  }
  return count; // Always read
}


UINT8 OneWire::ReadBit(CLR_RT_HeapBlock* pMngObj, HRESULT &hr)
{
  UINT32 pin = Get_pin(pMngObj);
  ASSERT(pin != GPIO_PIN_NONE);

  GLOBAL_LOCK(irq);

  UINT8 retVal = OneWire_ReadBit(pin);
  return retVal;
}


UINT8 OneWire::ReadByte(CLR_RT_HeapBlock* pMngObj, HRESULT &hr)
{
  UINT32 pin = Get_pin(pMngObj);
  ASSERT(pin != GPIO_PIN_NONE);

  GLOBAL_LOCK(irq);

  UINT8 retVal = OneWire_ReadByte(pin);
  return retVal;
}


void OneWire::Write(CLR_RT_HeapBlock* pMngObj, CLR_RT_TypedArray_UINT8 buffer, INT32 index, INT32 count, HRESULT &hr)
{
  UINT32 pin = Get_pin(pMngObj);
  ASSERT(pin != GPIO_PIN_NONE);

  GLOBAL_LOCK(irq);
  {
    for(INT32 endIndex = index + count; index < endIndex; index++)
    {
      OneWire_WriteByte(pin, buffer[index]);
    }
  }
}


void OneWire::WriteBit(CLR_RT_HeapBlock* pMngObj, UINT8 value, HRESULT &hr)
{
  UINT32 pin = Get_pin(pMngObj);
  ASSERT(pin != GPIO_PIN_NONE);

  GLOBAL_LOCK(irq);
  {
    OneWire_WriteBit(pin, value);
  }
}


void OneWire::WriteByte(CLR_RT_HeapBlock* pMngObj, UINT8 value, HRESULT &hr)
{
  UINT32 pin = Get_pin(pMngObj);
  ASSERT(pin != GPIO_PIN_NONE);

  GLOBAL_LOCK(irq);
  {
    OneWire_WriteByte(pin, value);
  }
}


#define ONE_WIRE_SEARCH_FAILED      -1
#define ONE_WIRE_SEARCH_FINISHED     0

// 1-Wire search algorithm as in AVR318 Application Note
INT32 OneWire::Search(CLR_RT_HeapBlock* pMngObj, CLR_RT_TypedArray_UINT8 pattern, INT32 deviation, INT32 index, HRESULT &hr)
{
  UINT32 pin = Get_pin(pMngObj);
  ASSERT(pin != GPIO_PIN_NONE);

  UINT8* bitPattern = &pattern.GetBuffer()[index];
  UINT8 bitMask = 1;
  INT32 newDeviation = ONE_WIRE_SEARCH_FINISHED;  // Return value

  GLOBAL_LOCK(irq);
  {
    // The search algorithm begins with the devices on the bus being reset
    if(!OneWire_Reset(pin))
    {
      return ONE_WIRE_SEARCH_FAILED;
    }
    // If this is successful then the search command is sent
    OneWire_WriteByte(pin, 0xF0);  // Search ROM
    // TODO: Alarm Search (0xEC)

    for(int currentBit = 1; currentBit < 65; currentBit++)
    {
      UINT8 bitA = OneWire_ReadBit(pin);
      UINT8 bitB = OneWire_ReadBit(pin);  // Complement

      if(bitA & bitB)
      {
        return ONE_WIRE_SEARCH_FAILED;
      }
      if(bitA ^ bitB)
      {
        // Bits are different - all devices have the same bit here
        if(bitA)
        {
          (*bitPattern) |= bitMask;
        }
        else
        {
          (*bitPattern) &= ~bitMask;
        }
      }
      else  // Both bits are '0'
      {
        if(currentBit == deviation)
        {
          // If this is where a choice was made the last time,
          // a '1' bit is selected this time.
          (*bitPattern) |= bitMask;
        }
        else
        {
          if(currentBit > deviation)
          {
            // For the rest of the id, '0' bits are selected when discrepancies occur.
            (*bitPattern) &= ~bitMask;
            newDeviation = currentBit;
          }
          else
          {
            if(!(*bitPattern & bitMask))
            {
              // If current bit in bit pattern = 0, then this is new deviation.
              newDeviation = currentBit;
            }
          }
        }
      }
      // Send the selected bit to the bus.
      OneWire_WriteBit(pin, (*bitPattern) & bitMask);

      bitMask <<= 1;
      if(!bitMask)
      {
        bitMask = 1;
        bitPattern++;
      }
    }
  }
  return newDeviation;
}


// Computes 8-bit CRC using x^8 + x^5 + x^4 + 1 polynomial
UINT8 OneWire::ComputeCRC(CLR_RT_TypedArray_UINT8 buffer, INT32 index, INT32 count, UINT8 crc, HRESULT &hr)
{
  for(INT32 endIndex = index + count; index < endIndex; index++)
  {
    UINT8 b = buffer[index];
    for(int bit = 0; bit < 8; bit++)
    {
      int feedbackBit = (crc ^ b) & 1;
      crc >>= 1;
      if(feedbackBit)
      {
        crc ^= 0x8C;  // 0x31 reversed
      }
      b >>= 1;
    }
  }
  return crc;
}


// Computes 16-bit CRC using x^16 + x^15 + x^2 + 1 polynomial
UINT16 OneWire::ComputeCRC16(CLR_RT_TypedArray_UINT8 buffer, INT32 index, INT32 count, UINT16 crc, HRESULT &hr)
{
  for(INT32 endIndex = index + count; index < endIndex; index++)
  {
    UINT8 b = buffer[index];
    for(int bit = 0; bit < 8; bit++)
    {
      int feedbackBit = (crc ^ b) & 1;
      crc >>= 1;
      if(feedbackBit)
      {
        crc ^= 0xA001;  // 0x8005 reversed
      }
      b >>= 1;
    }
  }
  return crc;
}
