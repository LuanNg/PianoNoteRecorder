namespace PianoNoteRecorder {
	/// <summary>
	/// Contains the standard musical note lengths
	/// </summary>
	public enum NoteLength : int {
		/// <summary>
		/// Empty note
		/// </summary>
		None = 0,
		HemiDemiSemiQuaver = 2,
		DottedHemiDemiSemiQuaver = 3,
		DemiSemiQuaver = 4,
		DottedDemiSemiQuaver = 6,
		SemiQuaver = 8,
		DottedSemiQuaver = 12,
		Quaver = 16,
		DottedQuaver = 24,
		Crotchet = 32,
		DottedCrotchet = 48,
		Minim = 64,
		DottedMinim = 96,
		SemiBreve = 128
	}
}