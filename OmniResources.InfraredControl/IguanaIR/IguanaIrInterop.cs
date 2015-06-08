using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

namespace OmniResources.InfraredControl.IguanaIr
{
    /// <summary>
    /// Handles interop with the iguanaIR windows service.  Not a lot of docmentation is available, but the source is on
    /// github.  Best things to look at:
    /// https://github.com/iguanaworks/iguanair/blob/master/software/usb_ir/client.c - source for igclient.exe
    /// https://github.com/iguanaworks/iguanair/blob/master/software/usb_ir/iguanaIR.h - header file for iguanaIR.dll
    /// </summary>
    public class IguanaIrInterop : IInfraredCommunicator
    {
        private readonly IntPtr _handle;

        /// <summary>
        /// Initializes a new instance of the IguanaIrInterop class.
        /// Not sure exactly what the options for the first string parameter to connect are, but "0" works.
        /// The second parameter to connect is the protocol version, should always be 1.
        /// </summary>
        public IguanaIrInterop()
        {
            _handle = Connect("0", 1);

            if (_handle == IntPtr.Zero)
                throw new Win32Exception();
        }

        /// <summary>
        /// Sends a command to the device enabling IR reception
        /// </summary>
        public void EnableReceiver()
        {
            if (SendCommand(IguanaCode.IG_DEV_RECVON) != IguanaCode.IG_DEV_RECVON)
                throw new ApplicationException("Invalid response when enabling receiver!");
        }

        /// <summary>
        /// Sets the channels to transmit on
        /// </summary>
        public void SetChannels(params byte[] channels)
        {
            if (channels == null || channels.Length == 0)
                throw new ArgumentNullException("Please pass in at least one channel!");

            if (channels.Any(x => x > 4 || x < 1))
                throw new ArgumentException("Channels must be between 1 and 4!");

            byte sendValue = 0;

            foreach(var channel in channels)
            {
                sendValue |= (byte)(1 << (channel - 1));
            }

            var result = SendCommand(IguanaCode.IG_DEV_SETCHANNELS, new byte[] { sendValue });

            if (result != IguanaCode.IG_DEV_SETCHANNELS)
                throw new InvalidOperationException("Couldn't set channels...");
        }

        /// <summary>
        /// Reads data from the ir receiver
        /// </summary>
        public IEnumerable<PulseData> ReadData(uint timeout)
        {
            var result = new List<PulseData>();
            byte[] data;
            var code = ReceiveResponse(timeout, out data);

            if (code != IguanaCode.IG_DEV_RECV)
                throw new ApplicationException("Unexpected return code: " + code);

            for (var i = 0; i < data.Length; i += 4)
            {
                var curValue = BitConverter.ToUInt32(data, i);
                var isPulse = (curValue & 0x01000000) > 0;

                result.Add(new PulseData
                {
                    IsPulse = isPulse,
                    Length = isPulse ? (curValue & 0x00FFFFFF) : curValue
                });
            }

            return result;
        }

        /// <summary>
        /// Writes the given pulses to the IguanaIR.  Will block until the write is complete!
        /// </summary>
        public void WriteData(IEnumerable<PulseData> data)
        {
            var bytesToSend = new List<byte>();

            foreach (var pulse in data)
            {
                bytesToSend.AddRange(BitConverter.GetBytes(pulse.IsPulse ? pulse.Length | 0x01000000 : pulse.Length));
            }

            if (SendCommand(IguanaCode.IG_DEV_SEND, bytesToSend.ToArray()) != IguanaCode.IG_DEV_SEND)
                throw new ApplicationException("Invalid return code during send!");
        }

        [DllImport("iguanaIR.dll", EntryPoint = "iguanaConnect_real", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern IntPtr Connect(string name, uint protocol);

        [DllImport("iguanaIR.dll", EntryPoint = "iguanaClose", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern void Close(IntPtr handle);

        [DllImport("iguanaIR.dll", EntryPoint = "iguanaCreateRequest", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern IntPtr CreateRequest(byte code, uint length, IntPtr data);

        [DllImport("iguanaIR.dll", EntryPoint = "iguanaWriteRequest", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern bool WriteRequest(IntPtr request, IntPtr connection);

        [DllImport("iguanaIR.dll", EntryPoint = "iguanaReadResponse", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern IntPtr ReadResponse(IntPtr connection, uint timeout);

        [DllImport("iguanaIR.dll", EntryPoint = "iguanaResponseIsError", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern bool ResponseIsError(IntPtr response);

        [DllImport("iguanaIR.dll", EntryPoint = "iguanaFreePacket", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern void FreePacket(IntPtr pkt);

        [DllImport("iguanaIR.dll", EntryPoint = "iguanaRemoveData", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern IntPtr RemoveData(IntPtr pkt, ref uint dataLength);

        [DllImport("iguanaIR.dll", EntryPoint = "iguanaCode", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern byte Code(IntPtr pkt);

        /// <summary>
        /// Sends the specified code to the device as a command
        /// </summary>
        private IguanaCode SendCommand(IguanaCode command)
        {
            return SendCommand(command, new byte[0]);
        }

        /// <summary>
        /// Sends the specified code to the device as a command
        /// </summary>
        private IguanaCode SendCommand(IguanaCode command, byte[] data)
        {
            uint dataLen = 0;
            var dataPtr = IntPtr.Zero;

            if (data.Length > 0)
            {
                dataLen = (uint)data.Length;
                dataPtr = Marshal.AllocHGlobal((int)dataLen);
                Marshal.Copy(data, 0, dataPtr, (int)dataLen);
            }

            var request = CreateRequest((byte)command, dataLen, dataPtr);

            if (request == IntPtr.Zero)
                throw new ApplicationException("Error creating command request!");

            if (!WriteRequest(request, _handle))
                throw new ApplicationException("Couldn't write request to IguanaIR!");

            // Need to "remove data" from the request before freeing, not quite sure why...
            var nullLength = 0u;
            RemoveData(request, ref nullLength);
            FreePacket(request);

            if (data.Length > 0)
            {
                Marshal.FreeHGlobal(dataPtr);
            }

            return ReceiveResponse(10000);
        }

        /// <summary>
        /// Receives a response from the device, ignoring any additional data
        /// </summary>
        private IguanaCode ReceiveResponse(uint timeout)
        {
            byte[] data;
            return ReceiveResponse(timeout, out data);
        }

        /// <summary>
        /// Receives a response from the device, including additional data
        /// </summary>
        private IguanaCode ReceiveResponse(uint timeout, out byte[] data)
        {
            var response = ReadResponse(_handle, timeout);

            if (ResponseIsError(response))
                throw new Win32Exception();

            uint length = 0;
            data = new byte[0];
            var byteArrayPointer = RemoveData(response, ref length);

            if (byteArrayPointer != IntPtr.Zero)
            {
                data = new byte[length];
                Marshal.Copy(byteArrayPointer, data, 0, (int)length);
            }

            var cmd = (IguanaCode)Code(response);
            FreePacket(response);

            return cmd;
        }

        /// <summary>
        /// Disposes the IguanaIR
        /// </summary>
        public void Dispose()
        {
            if (_handle != IntPtr.Zero)
                Close(_handle);
        }

        /// <summary>
        /// From iguanaIR.h...
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private enum IguanaCode : byte
        {
            /* Let the client and daemon check their versions.  should be the
               first thing sent by the client. */
            IG_EXCH_VERSIONS = 0xFE, /* internal to client/daemon */

            /* used in response packets */
            IG_DEV_ERROR = 0x00,

            /* FILE:loader.inc bootloader functions */
            IG_DEV_GETVERSION  = 0x01,
            IG_DEV_WRITEBLOCK  = 0x02,
            IG_DEV_CHECKSUM    = 0x03,
            IG_DEV_INVALID_ARG = 0x04,
            IG_DEV_BAD_CHKSUM  = 0x05,
            IG_DEV_NOUSB       = 0x06, /* body defines action when enumeration fails */
            IG_DEV_RESET       = 0xFF,

            /* FILE:body.inc standard "body" functions */
            IG_DEV_GETFEATURES  = 0x10,
            IG_DEV_GETBUFSIZE   = 0x11,
            IG_DEV_RECVON       = 0x12,
            IG_DEV_RAWRECVON    = 0x13,
            IG_DEV_RECVOFF      = 0x14,
            IG_DEV_SEND         = 0x15,
            IG_DEV_GETCHANNELS  = 0x16, /* internal to client/daemon */
            IG_DEV_SETCHANNELS  = 0x17, /* internal to client/daemon */
            IG_DEV_GETCARRIER   = 0x18, /* internal to client/daemon */
            IG_DEV_SETCARRIER   = 0x19, /* internal to client/daemon */
            IG_DEV_GETPINCONFIG = 0x1A,
            IG_DEV_SETPINCONFIG = 0x1B,
            IG_DEV_GETPINS      = 0x1C,
            IG_DEV_SETPINS      = 0x1D,
            IG_DEV_PINBURST     = 0x1E,
            IG_DEV_EXECUTE      = 0x1F,
            IG_DEV_GETID        = 0x20, /* internal to client/daemon */
            IG_DEV_SETID        = 0x21, /* internal to client/daemon */
            IG_DEV_IDSOFF       = 0x22, /* internal to client/daemon */
            IG_DEV_IDSON        = 0x23, /* internal to client/daemon */
            IG_DEV_IDSTATE      = 0x24, /* internal to client/daemon */
            IG_DEV_REPEATER     = 0x25,
            IG_DEV_GETLOCATION  = 0x26, /* internal to client/daemon */
            IG_DEV_RESEND       = 0x27,
            IG_DEV_SENDSIZE     = 0x28, /* internal to client/daemon */

            /* FILE:body.inc packets initiated by the device */
            IG_DEV_RECV         = 0x30,
            IG_DEV_OVERRECV     = 0x31,
            IG_DEV_OVERSEND     = 0x32,

            /* for interpretting codes */
            //IG_PULSE_BIT  = 0x01000000,
            //IG_PULSE_MASK = 0x00FFFFFF,

            /* to handle raw signal data */
            IG_RAWSPACE_BIT  = 0x80,
            IG_RAWSPACE_MASK = 0x7F,

            /* FILE:body.inc device feature flags, 0 means old style device */
            IG_HAS_LEDS    = 0x01,
            IG_HAS_BOTH    = 0x02,
            IG_HAS_SOCKETS = 0x04,
            IG_HAS_LCD     = 0x08,
            IG_SLOT_DEV    = 0x10,

            /* used to include support for old protocol */
            IG_DEV_GETCONFIG0 = 0x07,
            IG_DEV_SETCONFIG0 = 0x08,
            IG_DEV_GETCONFIG1 = 0x09,
            IG_DEV_SETCONFIG1 = 0x0A
        };
    }
}
