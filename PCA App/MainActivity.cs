using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Content;
using System.Collections.Generic;
using Java.IO;
using Android.Graphics;

using perm = Android.Manifest.Permission;

namespace PCAapp
{
	public static class AppSettings {
		public static Settings settings;
		public static File _file;
		public static File dirPhoto;
		public static File dirData;
		public static Bitmap bitmap;
	}
	[Activity(Label = "PCA App", MainLauncher = true)]
	public class MainActivity : Activity
    {
		private List<string> perms;

        protected override void OnCreate(Bundle savedInstanceState) {

			base.OnCreate(savedInstanceState);

			// Check to make sure all permissions needed are still granted 
			// add any needed permissions here
			perms = new List<string>();
            perms.Add(perm.ReadExternalStorage);
            perms.Add(perm.WriteExternalStorage);
            perms.Add(perm.Camera);

			// this well check if each permission is currently granted and if not it will request it.
			// this should just request all missing permissions
			RequestPermissions(new string[] { perms[0],perms[1],perms[2] },1);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

            Button studentButton = FindViewById<Button>(Resource.Id.studentButton);
            studentButton.Click += Start_Student_Mode;
			studentButton.Click += init;

            Button professorButton = FindViewById<Button>(Resource.Id.profButton);
            professorButton.Click += Start_Prof_Mode;
			professorButton.Click += init;

			Button aboutButton = FindViewById<Button>(Resource.Id.aboutButton);
			aboutButton.Click += About;
			
        }

		private void About(Object sender, EventArgs e) {
			AlertDialog.Builder helpAlert = new AlertDialog.Builder(this);

			helpAlert.SetTitle("Reference Mode Help");
			helpAlert.SetMessage(
				"About \n"+
				"Processing core and post launch development: Joseph Hallmark\n"+
				"Initial mobile app development: Seth Tucker, Joseph Hallmark, Zach Bockskopf, Daxtyn Hiestand, Ted Green\n" +
				"Supervisor: Dr. Razib Iqbal\n"+
				"Customer: Dr. Keiichi Yoshimatsu"
						);

			Dialog helpDialog = helpAlert.Create();
			helpDialog.Show();

		}

		private void init(object sender, EventArgs e) {
			CreateDirectoryForPictures();
			AppSettings.settings = new Settings();
		}

		private void CreateDirectoryForPictures() {
			AppSettings.dirData = new Java.IO.File("storage//emulated//0//PCA App//Data");
			AppSettings.dirPhoto = new Java.IO.File("storage//emulated//0//PCA App//Photos");
			if (!AppSettings.dirData.Exists()) {
				AppSettings.dirData.Mkdirs();
			}
			if (!AppSettings.dirPhoto.Exists()) {
				AppSettings.dirPhoto.Mkdirs();
			}
		}

		private void Start_Student_Mode(object sender, EventArgs e)
        {
            SetContentView(Resource.Layout.TakePicture);
            Intent intent = new Intent(this, typeof(StudentMode));
            StartActivity(intent);
        }

        private void Start_Prof_Mode(object sender, EventArgs e)
        {
            SetContentView(Resource.Layout.ProfessorMode);
            Intent intent = new Intent(this, typeof(ReferenceMode));
            StartActivity(intent);
        }
    }  
}

