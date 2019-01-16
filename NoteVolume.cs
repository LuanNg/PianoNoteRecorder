namespace PianoNoteRecorder {
	/// <summary>
	/// Contains all available note loudness presets
	/// </summary>
	public enum NoteVolume : byte {
		/// <summary>
		/// Silence (Rest)
		/// </summary>
		silent = 0,
		/// <summary>
		/// Piano pianississimo
		/// </summary>
		pppp = 8,
		/// <summary>
		/// Pianississimo
		/// </summary>
		ppp = 20,
		/// <summary>
		/// Pianissimo
		/// </summary>
		pp = 31,
		/// <summary>
		/// Piano
		/// </summary>
		p = 42,
		/// <summary>
		/// Mezzo piano
		/// </summary>
		mp = 53,
		/// <summary>
		/// Mezzo forte
		/// </summary>
		mf = 64,
		/// <summary>
		/// Forte
		/// </summary>
		f = 80,
		/// <summary>
		/// Fortissimo
		/// </summary>
		ff = 96,
		/// <summary>
		/// Fortississimo
		/// </summary>
		fff = 112,
		/// <summary>
		/// Forte fortississimo
		/// </summary>
		ffff = 127
	}
}