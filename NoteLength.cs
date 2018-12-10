namespace NezvalPiano {
	/// <summary>
	/// Contains all standard note lengths
	/// </summary>
	public enum NoteLength : int {
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
		SemiBreve = 128,
		DottedSemiBreve = 192,
		Breve = 256,
		DottedBreve = 384
	}
}