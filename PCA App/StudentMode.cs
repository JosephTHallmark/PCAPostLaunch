using System;
using System.Collections.Generic;
using Java.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Content.PM;
using Android.Graphics;
using Android.Provider;

using System.Text.RegularExpressions;

using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;

using Com.Theartofdev.Edmodo.Cropper;

namespace PCAapp
{
	//   public static class AppSettings
	//   {
	//	public static File _file;
	//	public static File dirPhoto;
	//	public static File dirData;
	//	public static Bitmap bitmap;
	//}

	[Activity(Label = "Student Mode")]
	public class StudentMode : Activity {
		String referencePath;

		// Pic counter
		int count = 0;

		// Number of areas
		int areaCounter = 0;
		int areaLimit = 0;


		// Keep track of capture intent
		const int CAMERA_CAPTURE = 1;

		// Keep track of cropping intent
		const int GALLERY_PICK = 2;

		// Keep track of data chosing intent 
		const int DATAFRAG_IMPORT = 3;
		const int DATASET_IMPORT = 4;

		// Captured picture uri
		private Android.Net.Uri picUri;

		// Button declaration
		ImageView selectAreaImageView;
		ImageButton captureButton, helpBtn;
		Button cropButton, labelButton, finishButton, importImgButton, importDataButton;
		CheckBox autoScaling;

		// TextView declaration
		TextView selectAreaCounterLbl, pictureTitleLabel;
		// Lists for Student Data 
		List<Double> ImageTuple;

		//%diff 
		double maxDist = 0;
		List<List<double>> psData0;
		List<List<double>> psData255;
		//Path String Stuff
		//string path;
		//string filePath;

		protected override void OnCreate(Bundle bundle) {
			// attempting to fix file uri exposed execption
			StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
			StrictMode.SetVmPolicy(builder.Build());
			//

			base.OnCreate(bundle);

			if (IsThereAnAppToTakePictures()) {
				//CreateDirectoryForPictures();
				//AppSettings.settings = new Settings();

				// Set our view from the "main" layout resource
				SetContentView(Resource.Layout.TakePicture);

				// Take a picture button	
				helpBtn = FindViewById<ImageButton>(Resource.Id.helpButton);
				helpBtn.Click += Help;

				pictureTitleLabel = FindViewById<TextView>(Resource.Id.pictureTitleLbl);
				pictureTitleLabel.Text = "Import a Reference File";

				captureButton = FindViewById<ImageButton>(Resource.Id.picButton);
				captureButton.SetImageResource(Resource.Drawable.importFileImage);
				captureButton.Click += importDataSet;

				importImgButton = FindViewById<Button>(Resource.Id.picImport);
				importImgButton.Visibility = Android.Views.ViewStates.Invisible;
				importImgButton.Click += importImage;

				importDataButton = FindViewById<Button>(Resource.Id.dataImport);
				importDataButton.Visibility = Android.Views.ViewStates.Invisible;
				importDataButton.Click += importDataFrag;

				autoScaling = FindViewById<CheckBox>(Resource.Id.StuCheckBox);
				autoScaling.Click += autoScale;
				autoScaling.Visibility = Android.Views.ViewStates.Invisible;

				// programmatically return the take picture view to the way it should look without import data option 

				//Init data stuff
				ImageTuple = new List<double>();

				StufTutInit();
			}
		}

		private void autoScale(object sender, EventArgs e) {
			referenceRead(referencePath);
		}

		private double eDistance(List<List<double>> i1, List<List<double>> i2) {
			//Eculdian distance = sqrt((x1-x2)^2 + (y1-y2)^2 + (z1-z2)^2)
			double xdiff = i1[0][0] - i2[0][0];
			double ydiff = i1[0][1] - i2[0][1];
			double zdiff = i1[0][2] - i2[0][2];

			xdiff = Math.Pow(xdiff, 2);
			ydiff = Math.Pow(ydiff, 2);
			zdiff = Math.Pow(zdiff, 2);

			double total = xdiff + ydiff + zdiff;
			total = Math.Sqrt(total);

			return total;
		}

		// Override the default behavior of the nav bar back button to return to the
		// main menu.
		public override void OnBackPressed() {
			SetContentView(Resource.Layout.Main);
			Intent intent = new Intent(this, typeof(MainActivity));
			StartActivity(intent);
		}

		// Try to decode a URI as a bitmap
		private Bitmap decodeUriAsBitmap(Uri uri) {
			Bitmap bitmap = null;
			try {
				//bitmap = BitmapFactory.DecodeFile(uri.Path);
				bitmap = BitmapFactory.DecodeStream(this.ContentResolver.OpenInputStream(uri));
			}
			catch (FileNotFoundException e) {
				e.PrintStackTrace();
				return null;
			}
			return bitmap;
		}

		// Convert RGB values to grayscale 
		public int Grayscale(Android.Graphics.Color avgCol) {
			var red = Android.Graphics.Color.GetRedComponent(avgCol);
			var green = Android.Graphics.Color.GetGreenComponent(avgCol);
			var blue = Android.Graphics.Color.GetBlueComponent(avgCol);

			return (int)(red * .299 + green * .587 + blue * .114);
		}

		public void FileLabel(List<String> List) {
			// Creates a path to documents
			List<List<Double>> data = new List<List<double>>();
			data.Add(ImageTuple);

			//create path
			var path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
			var filePath = System.IO.Path.Combine(path, "RefernceFile.txt");

			UserInput.Data = data;
			UserInput.Process();


			//Show Results
			// This displays an alert box to show the Results 
			AlertDialog.Builder alert = new AlertDialog.Builder(this);

			string alert1 = ("You were closest to " + DataStructure.Labels[UserInput.closestIndex] + "\nWith a distance of: " +
					UserInput.closestDist + "\nPercent Difference: " + (UserInput.closestDist / maxDist));

			alert.SetMessage(alert1);
			Dialog dialog = alert.Create();

		}

		private void Help(object sender, EventArgs e) {
			AlertDialog.Builder helpAlert = new AlertDialog.Builder(this);

			helpAlert.SetTitle("Student Mode Help");
			helpAlert.SetMessage(
			"Taking a Picture:\n" +
			"\t1. Ensure protein tray is well lit.\n" +
			"\t2. Try to take the picture from a 90 degree angle above the tray. Avoid odd angles.\n" +
			"\t3. Avoid shadows on the tray.\n" +
			"\nArea Selection:\n" +
			"\t1. You must select the correct number of areas specified by the reference file. The number " +
			"of necessary areas will be shown by the area selection button.\n" +
			"\t2. All areas should be as close to the same size as possible.\n" +
			"\t3. Select areas in order starting from the upper left and moving to the right row by row.\n"
						);

			Dialog helpDialog = helpAlert.Create();
			helpDialog.Show();

		}

		// Pretty self explanatory. Checks if the device has a camera for picture taking
		private bool IsThereAnAppToTakePictures() {
			Intent intent = new Intent(MediaStore.ActionImageCapture);
			IList<ResolveInfo> availableActivities =
				PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
			return availableActivities != null && availableActivities.Count > 0;
		}

		// Creates a directory to store images if it doesn't already exist.
		/*** THIS FUNCTION MAY NOT BE NEEDED LATER ON AS IMAGES MIGHT NOT NEED TO BE SAVED TO THE DEVICE ***/
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

		// Function to average pixel color of cropped images
		private static Android.Graphics.Color CalculateAverageColor(Bitmap bm) {
			/* This function adds the RGB values for each pixel in the bitmap and finds
             * the average.
             */
			int red = 0;
			int green = 0;
			int blue = 0;

			// Total number of pixels in image
			int pixCount = 0;

			int bitmapWidth = bm.Width;
			int bitmapHeight = bm.Height;

			for (int x = 0; x < bitmapWidth; x++) {
				for (int y = 0; y < bitmapHeight; y++) {
					// Temporary variable to store the color of each pixel
					int tmpColor = bm.GetPixel(x, y);

					// Add RGB values for each pixel
					red += Android.Graphics.Color.GetRedComponent(tmpColor);
					green += Android.Graphics.Color.GetGreenComponent(tmpColor);
					blue += Android.Graphics.Color.GetBlueComponent(tmpColor);

					// Keep track of how many pixes processed
					pixCount++;
				}
			}

			// Calculate Averages
			red /= pixCount;
			green /= pixCount;
			blue /= pixCount;

			return Android.Graphics.Color.Rgb(red, green, blue);
		}

		// Take a picture intent
		private void TakeAPicture(object sender, EventArgs eventArgs) {
			if (AppSettings.settings.StuTutPicture) StuTutPicture();
			else {
				Intent intent = new Intent(MediaStore.ActionImageCapture);
				AppSettings._file = new File(AppSettings.dirPhoto, String.Format("PCAPhoto_{0}.jpg", Guid.NewGuid()));
				intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(AppSettings._file));
				intent.PutExtra(MediaStore.ExtraSizeLimit, 256);
				StartActivityForResult(intent, CAMERA_CAPTURE);
				count++;
				captureButton.Enabled = false;
			}
		}

		// Crop, aka area selection, intent
		private void CropPic(object sender, EventArgs eventArgs) {
			// Take care of exceptions
			try {
				//Possibly scale down the image here
				CropImage.Builder(picUri)
					.Start(this);

				areaCounter++;
			}
			// Respond to users whose devices do not support the crop action
			catch (ActivityNotFoundException anfe) {
				// Display an error message
				anfe.PrintStackTrace();
				String errorMessage = "Whoops - your device doesn't support the crop action!";
				Toast toast = Toast.MakeText(this, errorMessage, ToastLength.Short);
				toast.Show();
			}
		}

		private void importImage(object sender, EventArgs e) {
			if (AppSettings.settings.StuTutImportPhoto) StuTutImportPhoto();
			else {
				Intent intent = new Intent();
				intent.SetType("image/*");
				intent.SetAction(Intent.ActionGetContent);

				StartActivityForResult(intent, GALLERY_PICK);
			}
		}
		private void importDataFrag(object sender, EventArgs e) {
			if (AppSettings.settings.StuTutImportFrag) StuTutImportFrag();
			else {
				// This event handler will import created file/fragments
				Intent intent = new Intent();
				intent.SetType("text/comma-separated-values");
				intent.SetAction(Intent.ActionGetContent);

				StartActivityForResult(intent, DATAFRAG_IMPORT);
			}
		}
		private void importDataSet(object sender, EventArgs e) {
			// This event handler will import created file/fragments
			Intent intent = new Intent();
			intent.SetType("text/comma-separated-values");
			intent.SetAction(Intent.ActionGetContent);

			StartActivityForResult(intent, DATASET_IMPORT);
		}


		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
			base.OnActivityResult(requestCode, resultCode, data);
			if (resultCode == Result.Ok) {
				// User is returning from capturing an image using the camera
				if (requestCode == CAMERA_CAPTURE) {
					resultCameraCapture(data);
				}
				// User is returning from the image picker 
				else if (requestCode == GALLERY_PICK) {
					resultGalleryPick(data);
				}
				else if (requestCode == DATAFRAG_IMPORT) {
					resultDataFragImport(data);
				}
				else if (requestCode == DATASET_IMPORT) {
					resultDataSetImport(data);
				}
				// User is returning from cropping the image
				else if (requestCode == CropImage.CropImageActivityRequestCode) {
					resultCrop(data);
				}
			}
		}

		private void resultCameraCapture(Intent data) {
			picUri = Uri.FromFile(AppSettings._file);

			// Make picture available in the gallery
			Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
			Uri contentUri = Uri.FromFile(AppSettings._file);
			mediaScanIntent.SetData(contentUri);
			SendBroadcast(mediaScanIntent);

			captureButton.Enabled = false;
			SetContentView(Resource.Layout.AreaSelection);

			// Crop, aka select area, button
			helpBtn = FindViewById<ImageButton>(Resource.Id.helpButton);
			helpBtn.Click += Help;
			cropButton = FindViewById<Button>(Resource.Id.cropButton);
			selectAreaCounterLbl = FindViewById<TextView>(Resource.Id.selectAreaCounterLbl);
			cropButton.Enabled = false;  // Can't crop a photo that doesn't exist yet
			cropButton.Click += CropPic;
			cropButton.Enabled = true;


			// Display in ImageView
			ImageView picView = FindViewById<ImageView>(Resource.Id.imageView1);
			int height = Resources.DisplayMetrics.HeightPixels;
			int width = Resources.DisplayMetrics.WidthPixels;
			AppSettings.bitmap = AppSettings._file.Path.LoadandResizeBitmap(width, height);
			if (AppSettings.bitmap != null) {
				picView.SetImageBitmap(AppSettings.bitmap);
				AppSettings.bitmap = null;
			}

			// Enable area selection button

			// Dispose of Java side bitmap
			GC.Collect();
		}
		private void resultGalleryPick(Intent data) {
			picUri = data.Data;

			// Show picture on image view

			captureButton.Enabled = false;
			SetContentView(Resource.Layout.AreaSelection);

			// Enable area selection button

			helpBtn = FindViewById<ImageButton>(Resource.Id.helpButton);
			helpBtn.Click += Help;
			cropButton = FindViewById<Button>(Resource.Id.cropButton);
			selectAreaCounterLbl = FindViewById<TextView>(Resource.Id.selectAreaCounterLbl);
			cropButton.Enabled = false;  // Can't crop a photo that doesn't exist yet
			cropButton.Click += CropPic;
			cropButton.Enabled = true;
			ImageView picView = FindViewById<ImageView>(Resource.Id.imageView1);

			picView.SetImageURI(picUri);
		}
		private void resultDataFragImport(Intent data) {
			string dataStr = data.Data.ToString();
			string path = data.Data.Path;
			path = getFragPath(path);

			path = "storage/emulated/0/" + path;
			if (path != "storage/emulated/0/") {
				//check path IMPORTANT
				string[] lines = System.IO.File.ReadAllLines(path);
				// Read in the 2nd line for number of areas recorded
				string[] numbers = lines[1].Split(',');
				// If it matches the area limit 
				if (numbers.Length == areaLimit) {
					ImageTuple = new List<double>();
					foreach (string item in numbers) {
						ImageTuple.Add(double.Parse(item));
					}
				}

				//Finished(null, null);
				results();
			}
			else {
				Toast.MakeText(Application.Context, "File does not match data fragment format or cannot be found", ToastLength.Long).Show();
			}

			////Disable the camrea picture button
			//captureButton.Visibility = Android.Views.ViewStates.Gone;
			////disable the import data fragment button
			//importDataButton.Visibility = Android.Views.ViewStates.Gone;
			////disable the import image button 
			//importImgButton.Visibility = Android.Views.ViewStates.Gone;

		}
		private string getFragPath(string fakePath) {
			//This could cause issues if the naming scheme changes later
			string output = "";

			Regex regex = new Regex(@"((PCA App/Data/DataFrag).*csv)");
			Match match = regex.Match(fakePath);
			output = match.Value;

			return output;
		}
		private void resultDataSetImport(Intent data) {
			string dataStr = data.Data.ToString();
			string path = data.Data.Path;
			path = getSetPath(path);

			path = "storage/emulated/0/" + path;

			if (System.IO.File.Exists(path)) {
				if (path != "storage/emulated/0/") {
					//If file exists on the path

					// Only move on if positive button 
					pictureTitleLabel = FindViewById<TextView>(Resource.Id.pictureTitleLbl);
					pictureTitleLabel.Text = "Take a Picture";
					captureButton.SetImageResource(Resource.Drawable.cameraBtnImage);
					captureButton.Click += TakeAPicture;
					captureButton.Click -= importDataSet;

					importImgButton.Visibility = Android.Views.ViewStates.Visible;
					importDataButton.Visibility = Android.Views.ViewStates.Visible;
					autoScaling.Visibility = Android.Views.ViewStates.Visible;

					//read reference file at user specified path
					referencePath = path;
					referenceRead(referencePath); // keep
				}
				else {
					Toast.MakeText(Application.Context, "File is incorrect format or cannot be found", ToastLength.Long).Show();
				}
			}
			else {
				Toast.MakeText(Application.Context, "File is incorrect format or cannot be found", ToastLength.Long).Show();
			}
		}
		private string getSetPath(string fakePath) {
			//This could cause issues if the naming scheme changes later
			string output = "";

			Regex regex = new Regex(@"((PCA App/Data/Reference).*csv)");
			Match match = regex.Match(fakePath);
			output = match.Value;

			return output;
		}
		private void resultCrop(Intent data) {
			CropImage.ActivityResult result = CropImage.GetActivityResult(data);
			Uri resultUri = result.Uri;
			Bitmap bm = decodeUriAsBitmap(resultUri);


			// Calculate the average color of the bitmap
			// this is the slow method, scale down the bitmap before then

			// Scale the bitmap down before calculating average color
			Bitmap smallBitmap = Bitmap.CreateScaledBitmap(bm, 100, 100, false);

			Color avgColorSmall = CalculateAverageColor(smallBitmap);

			// Calculate the average color of the bitmap
			//Android.Graphics.Color avgColor = CalculateAverageColor(bm);

			//Add to the data structure
			int gray = new int();
			gray = Grayscale(avgColorSmall);
			ImageTuple.Add(gray);

			// Update area selection counter
			selectAreaCounterLbl.Text = "Number of crops: " + areaCounter.ToString() + " / " + areaLimit.ToString();

			// User must select at least 3 areas from the picture

			if (areaCounter == areaLimit) {
				selectAreaImageView = FindViewById<ImageView>(Resource.Id.imageView1);
				selectAreaImageView.LayoutParameters.Height = 500;
				selectAreaImageView.LayoutParameters.Width = 350;


				//Finished(null, null);
				results();
				//cropButton.Text = "Finished";
				//cropButton.Click -= CropPic;
				//cropButton.Click += Finished;

				//cropButton.Enabled = true;
			}
		}

		private void StufTutInit() {
			AlertDialog.Builder helpAlert = new AlertDialog.Builder(this);

			helpAlert.SetTitle("Unknown Mode Help");
			helpAlert.SetMessage("In this mode you will compare one unknown item to a previously made reference set." +
				"\nPressing this button will open a picker were you can find and select a reference file." +
				"\nData is stored on internal storage, if you cannot find internal storage you will need to press the three dots on the upper right hand corner > show internal storage" +
				"\nYou will then need to browse to internal storage > PCA App > Data and select a file that is prefixed with \"Reference\"");

			helpAlert.SetPositiveButton("Ok", (ak, ka) => {
				AppSettings.settings.StuTutInit = false;
				AppSettings.settings.Write();
			});

			Dialog helpDialog = helpAlert.Create();
			helpDialog.Show();

		}
		private void StuTutPicture() {
			AlertDialog.Builder helpAlert = new AlertDialog.Builder(this);

			helpAlert.SetTitle("Taking a picture");
			helpAlert.SetMessage("You will take a picture of an item. Try to take the picture in the same way as every other picture you\'ve taken." +
				"\nThen you will select areas of that item. Do this in the same order each time.");

			helpAlert.SetPositiveButton("Ok", (ak, ka) => {
				Intent intent = new Intent(MediaStore.ActionImageCapture);
				AppSettings._file = new File(AppSettings.dirPhoto, String.Format("PCAPhoto_{0}.jpg", Guid.NewGuid()));
				intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(AppSettings._file));
				intent.PutExtra(MediaStore.ExtraSizeLimit, 256);
				StartActivityForResult(intent, CAMERA_CAPTURE);
				count++;
				captureButton.Enabled = false;

				AppSettings.settings.StuTutPicture = false;
				AppSettings.settings.Write();
			});

			Dialog helpDialog = helpAlert.Create();
			helpDialog.Show();

		}
		private void StuTutImportPhoto() {
			AlertDialog.Builder helpAlert = new AlertDialog.Builder(this);

			helpAlert.SetTitle("Importing a picture");
			helpAlert.SetMessage("You will import a picture from picker were you can find and select a photo." +
				"\nPreviously taken photos will be in Internal Storage > PCA App > Photos" +
				"\nIf you cannot find internal storage you will need to press the three dots on the upper right hand corner > show internal storage" +
				"\nThen you will select areas of that item. Do this in the same order each time.");

			helpAlert.SetPositiveButton("Ok", (ak, ka) => {
				Intent intent = new Intent();
				intent.SetType("image/*");
				intent.SetAction(Intent.ActionGetContent);

				StartActivityForResult(intent, GALLERY_PICK);

				AppSettings.settings.StuTutImportPhoto = false;
				AppSettings.settings.Write();
			});

			Dialog helpDialog = helpAlert.Create();
			helpDialog.Show();
		}
		private void StuTutImportFrag() {
			AlertDialog.Builder helpAlert = new AlertDialog.Builder(this);

			helpAlert.SetTitle("Importing a data fragment");
			helpAlert.SetMessage("You will open a picker were you can find and select a datafragment file." +
				"\nData is stored on internal storage, if you cannot find internal storage you will need to press the three dots on the upper right hand corner > show internal storage" +
				"\nYou will then need to browse to internal storage > PCA App > Data and select a file that is prefixed with \"DataFrag\"" +
				"\nIt will need to be the same number of areas as your current set. Shown on the previous screen. This is said by the file name");

			helpAlert.SetPositiveButton("Ok", (ak, ka) => {
				// This event handler will import created file/fragments
				Intent intent = new Intent();
				intent.SetType("text/comma-separated-values");
				intent.SetAction(Intent.ActionGetContent);

				StartActivityForResult(intent, DATAFRAG_IMPORT);

				AppSettings.settings.StuTutImportFrag = false;
				AppSettings.settings.Write();
			});

			Dialog helpDialog = helpAlert.Create();
			helpDialog.Show();
		}

		private void referenceRead(string filePath) {
			//Read Stuff into pca from reference file
			DataReader datareader = new DataReader(filePath, autoScaling.Checked);
			DataStructure.Labels = datareader.Labels;
			DataStructure.FeatureVectors = datareader.Vectors;
			DataStructure.FinalDataRealigned = datareader.FinalDataRealigned;
			areaLimit = datareader.DimensionCount;

			//create tuple of 0 and get its data point 
			//create pseudo data 0
			List<Double> pseudoDataZero = new List<double>();
			for (int i = 0; i < datareader.DimensionCount; i++) {
				pseudoDataZero.Add(0);
			}

			List<List<Double>> data = new List<List<double>>();
			data.Add(pseudoDataZero);

			//process pseudo data 0
			UserInput.Data = data;
			UserInput.maxDisProcess();
			psData0 = new List<List<double>>(UserInput.FinalDataRealinged);

			//create tuple of 255 and get its data point 
			//create pseudo data 255
			pseudoDataZero = new List<double>();
			for (int i = 0; i < datareader.DimensionCount; i++) {
				pseudoDataZero.Add(255);
			}

			data = new List<List<double>>();
			data.Add(pseudoDataZero);

			//process pseudo data 255
			UserInput.Data = data;
			UserInput.maxDisProcess();
			psData255 = new List<List<double>>(UserInput.FinalDataRealinged);

			maxDist = eDistance(psData0, psData255);

			pictureTitleLabel.Text = "Number of areas " + areaLimit.ToString();
		}
		private void results() {
			areaCounter = 0; // reset so the counter works if we take another picture 
							 //This is where we tie into the PCA core
			List<List<Double>> data = new List<List<double>>();
			data.Add(ImageTuple);

			if (autoScaling.Checked) data = normalizedSet(data);

			UserInput.Data = data;
			UserInput.Process();
			//Show Results
			// This displays an alert box to show the Results 
			AlertDialog.Builder alert = new AlertDialog.Builder(this);
			string alert1 = ("You were closest to " + DataStructure.Labels[UserInput.closestIndex] + "\nWith a distance of: " + UserInput.closestDist + "\n% Distance: " + (UserInput.closestDist / maxDist) * 100);

			alert.SetMessage(alert1);
			alert.SetPositiveButton("Ok", (s, ss) => {
				returns();
			});
			Dialog dialog = alert.Create();
			dialog.Show();

			///Temporarily commented out to test student mode
			///We could leave it like this as to allow for the student to take multiple pictures and get results for each
			//SetContentView(Resource.Layout.Main);
			//Intent intent = new Intent(this, typeof(MainActivity));
			//StartActivity(intent);
			//This needs to be commented out or student mode auto closes the results screen, they can still hit back to exit 
		}

		private double StandardDev(List<double> input, double mean) {
			double output = -1;

			foreach (double number in input) {
				output += Math.Pow((number - mean), 2);
			}

			output /= input.Count;
			output = Math.Sqrt(output);

			return output;
		}

		private List<List<double>> normalizedSet(List<List<double>> input) {
			List<List<double>> output = new List<List<double>>();

			//Do Stuff Here

			foreach (List<double> line in input) {
				List<double> newLine = new List<double>();
				//for each line 
				//find its mean
				double mean = 0;
				foreach (double number in line) mean += number;
				mean /= line.Count;
				//find its standard deviation
				double stdDev = StandardDev(line, mean);
				foreach (double number in line) newLine.Add((number - mean) / stdDev);

				output.Add(newLine);
			}

			//Do Stuff Here

			return output;
		}

		private void returns(){
			DataStructure.clear();
			referenceRead(referencePath);
			SetContentView(Resource.Layout.TakePicture);

			bool norm = autoScaling.Checked;

			captureButton = FindViewById<ImageButton>(Resource.Id.picButton);
			captureButton.SetImageResource(Resource.Drawable.cameraBtnImage);
			captureButton.Click += TakeAPicture;


			importImgButton = FindViewById<Button>(Resource.Id.picImport);
			importImgButton.Visibility = Android.Views.ViewStates.Visible;
			importImgButton.Click += importImage;

			importDataButton = FindViewById<Button>(Resource.Id.dataImport);
			importDataButton.Visibility = Android.Views.ViewStates.Visible;
			importDataButton.Click += importDataFrag;

			autoScaling = FindViewById<CheckBox>(Resource.Id.StuCheckBox);
			autoScaling.Visibility = Android.Views.ViewStates.Visible;
			autoScaling.Click += autoScale;
			autoScaling.Checked = norm;
			
		}
		private void Finished(Object sender, EventArgs e)
        {
			// Should be used to go back a page
		}
	}
}