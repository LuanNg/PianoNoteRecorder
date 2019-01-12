using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows.Forms;

namespace PianoNoteRecorder {
	public static class MidiPlayer {
		private delegate void MidiCallBack(IntPtr handle, int msg, int instance, int param1, int param2);
		private static IntPtr Handle;

		static MidiPlayer() {
			MidiOutCaps capabilities = new MidiOutCaps();
			CheckError(midiOutGetDevCaps(0, ref capabilities, (uint) Marshal.SizeOf(capabilities)));
			CheckError(midiOutOpen(ref Handle, 0, null, 0, 0));
		}

		private static void CheckError(MMRESULT error) {
			if (error != MMRESULT.MMSYSERR_NOERROR)
				MessageBox.Show("An error occurred while initializing MIDI: " + error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		/*private static string Mci(string command) {
			StringBuilder reply = new StringBuilder(256);
			mciSendString(command, reply, 256, IntPtr.Zero);
			return reply.ToString();
		}

		private static void MciMidiTest() {
			var res = Mci("open \"M:\\anger.mid\" alias music");
			res = Mci("play music");
			Console.ReadLine();
			res = Mci("close crooner");
		}*/

		public static void PlayNote(NoteEnum note, NoteVolume volume = NoteVolume.ffff) {
			const byte command = 0x90; //play piano note
			CheckError(midiOutShortMsg(Handle, (((byte) volume) << 16) + (((byte) note + 35) << 8) + command)); //12 notes per octave
		}

		[DllImport("winmm.dll")]
		[SuppressUnmanagedCodeSecurity]
		private static extern long mciSendString(string command, StringBuilder returnValue, int returnLength, IntPtr winHandle);

		[DllImport("winmm.dll")]
		[SuppressUnmanagedCodeSecurity]
		private static extern MMRESULT midiOutGetDevCaps(int deviceID, ref MidiOutCaps lpMidiOutCaps, uint cbMidiOutCaps);

		[DllImport("winmm.dll")]
		[SuppressUnmanagedCodeSecurity]
		private static extern MMRESULT midiOutOpen(ref IntPtr handle, int deviceID, MidiCallBack proc, int instance, int flags);

		[DllImport("winmm.dll")]
		[SuppressUnmanagedCodeSecurity]
		private static extern MMRESULT midiOutShortMsg(IntPtr handle, int message);

		[DllImport("winmm.dll")]
		[SuppressUnmanagedCodeSecurity]
		private static extern MMRESULT midiOutClose(IntPtr handle);

		[StructLayout(LayoutKind.Sequential)]
		private struct MidiOutCaps {
			public ushort wMid;
			public ushort wPid;
			public uint vDriverVersion;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string szPname;
			public ushort wTechnology;
			public ushort wVoices;
			public ushort wNotes;
			public ushort wChannelMask;
			public uint dwSupport;
		}

		private enum MMRESULT : uint {
			MMSYSERR_NOERROR = 0,
			MMSYSERR_ERROR = 1,
			MMSYSERR_BADDEVICEID = 2,
			MMSYSERR_NOTENABLED = 3,
			MMSYSERR_ALLOCATED = 4,
			MMSYSERR_INVALHANDLE = 5,
			MMSYSERR_NODRIVER = 6,
			MMSYSERR_NOMEM = 7,
			MMSYSERR_NOTSUPPORTED = 8,
			MMSYSERR_BADERRNUM = 9,
			MMSYSERR_INVALFLAG = 10,
			MMSYSERR_INVALPARAM = 11,
			MMSYSERR_HANDLEBUSY = 12,
			MMSYSERR_INVALIDALIAS = 13,
			MMSYSERR_BADDB = 14,
			MMSYSERR_KEYNOTFOUND = 15,
			MMSYSERR_READERROR = 16,
			MMSYSERR_WRITEERROR = 17,
			MMSYSERR_DELETEERROR = 18,
			MMSYSERR_VALNOTFOUND = 19,
			MMSYSERR_NODRIVERCB = 20,
			WAVERR_BADFORMAT = 32,
			WAVERR_STILLPLAYING = 33,
			WAVERR_UNPREPARED = 34
		}
	}
}