using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace PCAapp {
	public  class Settings {
	// We want to force them to read the help at least once
		public string Path { get; private set; } = "storage//emulated//0//PCA App//userSettings.json";


		/// <summary>
		/// Should the user be forced to read the tutorials
		/// Used to enforce a "first time read" of the help as this application relies on the user to know what they are doing with their phone.
		/// </summary>
		public bool ProfTutInit { get; set; } = true;
		/// <summary>
		/// Should the user be forced to read the tutorials
		/// Used to enforce a "first time read" of the help as this application relies on the user to know what they are doing with their phone.
		/// </summary>
		public bool ProfTutPicture { get; set; } = true;
		/// <summary>
		/// Should the user be forced to read the tutorials
		/// Used to enforce a "first time read" of the help as this application relies on the user to know what they are doing with their phone.
		/// </summary>
		public bool ProfTutImportPhoto { get; set; } = true;
		/// <summary>
		/// Should the user be forced to read the tutorials
		/// Used to enforce a "first time read" of the help as this application relies on the user to know what they are doing with their phone.
		/// </summary>
		public bool ProfTutImportFrag { get; set; } = true;

		/// <summary>
		/// Should the user be forced to read the tutorials
		/// Used to enforce a "first time read" of the help as this application relies on the user to know what they are doing with their phone.
		/// </summary>
		public bool ProfTutImportRaw { get; set; } = true;

										   /// <summary>
										   /// Should the user be forced to read the tutorials
										   /// Used to enforce a "first time read" of the help as this application relies on the user to know what they are doing with their phone.
										   /// </summary>
		public bool StuTutInit { get; set; } = true;
		/// <summary>
		/// Should the user be forced to read the tutorials
		/// Used to enforce a "first time read" of the help as this application relies on the user to know what they are doing with their phone.
		/// </summary>
		public bool StuTutPicture { get; set; } = true;
		/// <summary>
		/// Should the user be forced to read the tutorials
		/// Used to enforce a "first time read" of the help as this application relies on the user to know what they are doing with their phone.
		/// </summary>
		public bool StuTutImportPhoto { get; set; } = true;
		/// <summary>
		/// Should the user be forced to read the tutorials
		/// Used to enforce a "first time read" of the help as this application relies on the user to know what they are doing with their phone.
		/// </summary>
		public bool StuTutImportFrag { get; set; } = true;


		public Settings() {
			GetSettings();
		}
		public Settings(string path) {
			GetSettings(path);
		}

		/// <summary>
		/// Get the settings (support custom paths)
		/// </summary>
		public void GetSettings() {
			if (!File.Exists(Path)) {
				Write();
			}
			else {
				update();
			}
		}
		public void GetSettings(string path) {
			// Create it if it doesnt exist
			if (!File.Exists(path)) {
				Path = path;
				Write();
			}
			else {
				update();
			}
		}

		private void update() {
			// read in the file as a jtoken
			JToken token = JObject.Parse(File.ReadAllText(Path));

			// update the settings that need to be updated from the file
			ProfTutInit = (bool)token.SelectToken("ProfTutInit");
			ProfTutPicture = (bool)token.SelectToken("ProfTutPicture");
			ProfTutImportPhoto = (bool)token.SelectToken("ProfTutImportPhoto");
			ProfTutImportFrag = (bool)token.SelectToken("ProfTutImportFrag");

			StuTutInit = (bool)token.SelectToken("StuTutInit");
			StuTutPicture = (bool)token.SelectToken("StuTutPicture");
			StuTutImportPhoto = (bool)token.SelectToken("StuTutImportPhoto");
			StuTutImportFrag = (bool)token.SelectToken("StuTutImportFrag");


		}
		public void Write() {
			//Write to file
			//if (!System.IO.File.Exists(Path)) {
			//	System.IO.File.Create(Path);
			//}
			string json = JsonConvert.SerializeObject(this);
			File.WriteAllText(Path, json);			
		}
	}
}