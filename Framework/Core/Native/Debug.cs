////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.SPOT
{
    public static class Trace
    {
        [System.Diagnostics.ConditionalAttribute("TINYCLR_TRACE")]
        static public void Print(string text)
        {
            Debug.Print(text);
        }
    }

    public static class Debug
    {

        //
        // Summary:
        //     Writes a specified message followed by a line terminator to the debugger
        //     by using the OutputDebugString function.
        //
        // Parameters:
        //   message:
        //     The message to write to the debugger.

        /// <summary>
        /// Writes a specified message followed by a line terminator to the debugger
        /// by using the OutputDebugString function.
        /// </summary>
        /// <param name="message">The message to write to the debugger.</param>
        [System.Diagnostics.ConditionalAttribute("DEBUG")]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public void Print(string message);
        //--//

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public uint GC(bool force);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static public void EnableGCMessages(bool enable);

        //--//

        [System.Diagnostics.ConditionalAttribute("DEBUG")]
        static public void Assert(bool condition)
        {
            if (!condition)
            {
                Debugger.Break();
            }
        }

        [System.Diagnostics.ConditionalAttribute("DEBUG")]
        static public void Assert(bool condition, string message)
        {
            if (!condition)
            {
                Debugger.Break();
            }
        }

        [System.Diagnostics.ConditionalAttribute("DEBUG")]
        static public void Assert(bool condition, string message, string detailedMessage)
        {
            if (!condition)
            {
                Debugger.Break();
            }
        }
    }
}



