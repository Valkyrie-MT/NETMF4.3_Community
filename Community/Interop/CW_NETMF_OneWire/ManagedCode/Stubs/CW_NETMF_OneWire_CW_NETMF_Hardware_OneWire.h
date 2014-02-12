//-----------------------------------------------------------------------------
//
//                   ** WARNING! ** 
//    This file was generated automatically by a tool.
//    Re-running the tool will overwrite this file.
//    You should copy this file to a custom location
//    before adding any customization in the copy to
//    prevent loss of your changes when the tool is
//    re-run.
//
//-----------------------------------------------------------------------------


#ifndef _CW_NETMF_ONEWIRE_CW_NETMF_HARDWARE_ONEWIRE_H_
#define _CW_NETMF_ONEWIRE_CW_NETMF_HARDWARE_ONEWIRE_H_

namespace CW
{
    namespace NETMF
    {
        namespace Hardware
        {
            struct OneWire
            {
                // Helper Functions to access fields of managed object
                static INT8& Get_disposed( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_INT8( pMngObj, Library_CW_NETMF_OneWire_CW_NETMF_Hardware_OneWire::FIELD__disposed ); }

                static void& Get_pin( CLR_RT_HeapBlock* pMngObj )    { return Interop_Marshal_GetField_void( pMngObj, Library_CW_NETMF_OneWire_CW_NETMF_Hardware_OneWire::FIELD__pin ); }

                // Declaration of stubs. These functions are implemented by Interop code developers
                static void _ctor( CLR_RT_HeapBlock* pMngObj, void param0, HRESULT &hr );
                static INT8 Reset( CLR_RT_HeapBlock* pMngObj, HRESULT &hr );
                static INT32 Read( CLR_RT_HeapBlock* pMngObj, CLR_RT_TypedArray_UINT8 param0, INT32 param1, INT32 param2, HRESULT &hr );
                static UINT8 ReadBit( CLR_RT_HeapBlock* pMngObj, HRESULT &hr );
                static UINT8 ReadByte( CLR_RT_HeapBlock* pMngObj, HRESULT &hr );
                static void Write( CLR_RT_HeapBlock* pMngObj, CLR_RT_TypedArray_UINT8 param0, INT32 param1, INT32 param2, HRESULT &hr );
                static void WriteBit( CLR_RT_HeapBlock* pMngObj, UINT8 param0, HRESULT &hr );
                static void WriteByte( CLR_RT_HeapBlock* pMngObj, UINT8 param0, HRESULT &hr );
                static INT32 Search( CLR_RT_HeapBlock* pMngObj, CLR_RT_TypedArray_UINT8 param0, INT32 param1, INT32 param2, HRESULT &hr );
                static UINT8 ComputeCRC( CLR_RT_TypedArray_UINT8 param0, INT32 param1, INT32 param2, UINT8 param3, HRESULT &hr );
                static UINT16 ComputeCRC16( CLR_RT_TypedArray_UINT8 param0, INT32 param1, INT32 param2, UINT16 param3, HRESULT &hr );
            };
        }
    }
}
#endif  //_CW_NETMF_ONEWIRE_CW_NETMF_HARDWARE_ONEWIRE_H_
