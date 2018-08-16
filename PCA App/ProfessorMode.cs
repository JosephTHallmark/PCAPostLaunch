using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Widget;
using Uri = Android.Net.Uri;

using Com.Theartofdev.Edmodo.Cropper;

using System.Text.RegularExpressions;


namespace PCAapp {

	//public static class AppSettings
 //   {
 //       public static Java.IO.File _file;
 //       public static Java.IO.File dirPhoto;
 //       public static Java.IO.File dirData;
 //       public static Bitmap bitmap;
 //   }

    [Activity(Label = "Reference Mode")]
    public class ReferenceMode : Activity
    {		
		// Image picking service
		//IImagePickerService imagePickerService;

		// Count of how many times picture taken.
		int picCounter = 0;

        // Count of areas
        int areaCounter = 0;

        //Restrict area selection count to that of the first picture
        int areaUpperLimit = 0;

        // Count of data sets
        int datasetCounter = 0;

        // Keep track of capture intent
        const int CAMERA_CAPTURE = 1;
		const int GALLERY_PICK = 2;
		const int DATA_IMPORT = 3;
		const int DATA_IMPORT_RAW = 4;

		// Captured picture uri
		private Uri picUri;

        // Button declaration
        ImageView selectAreaImageView;
        ImageButton captureButton, helpBtn;
        Button cropButton, labelButton, finishButton, importImageButton, importDataButton;
        // TextView declaration
        TextView selectAreaCounterLbl, numberOfPicturesLbl, pictureTitleLbl;

        // Current view
        string currentView;

        // Lists for dataset 
        List<String> Labels;
        /// <summary>
        /// Contains the areas that have been gray-scaled
        /// </summary>
        List<double> ImageTuple;
        /// <summary>
        /// Contains the lists of each image
        /// </summary>
        List<List<double>> DataSet;

		#region Android Actions
		protected override void OnCreate(Bundle bundle)
        {
            // attempting to fix file uri exposed exception
            StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
            StrictMode.SetVmPolicy(builder.Build());
            //
            base.OnCreate(bundle);

            if (IsThereAnAppToTakePictures())
            {
                // Set our view from the "main" layout resource
                SetContentView(Resource.Layout.TakePicture);
                currentView = "areaSelect";

                helpBtn = FindViewById<ImageButton>(Resource.Id.helpButton);
                helpBtn.Click += Help;

                pictureTitleLbl = FindViewById<TextView>(Resource.Id.pictureTitleLbl);
                pictureTitleLbl.Text = "Take a Picture";

				// Take a picture button
				captureButton = FindViewById<ImageButton>(Resource.Id.picButton);
				captureButton.SetImageResource(Resource.Drawable.cameraBtnImage);
                captureButton.Click += TakeAPicture;

				importImageButton = FindViewById<Button>(Resource.Id.picImport);
				importImageButton.Click += importImage;

				importDataButton = FindViewById<Button>(Resource.Id.dataImport);
				importDataButton.Click += importData;

				finishButton = FindViewById<Button>(Resource.Id.finishButton);
				finishButton.Visibility = Android.Views.ViewStates.Visible;
				finishButton.Text = "Import Raw Data Set";
				finishButton.Click += importRawSet;

				//Initialize The lists
				Labels = new List<String>();
                ImageTuple = new List<double>();
                DataSet = new List<List<double>>();

				//CreateDirectoryForPictures();
				//AppSettings.settings = new Settings();

				if (AppSettings.settings.ProfTutInit) ProfTutInit();
            }
        }

		// Override the default behavior of the nav bar back button to return to the main menu.
		public override void OnBackPressed() {
            SetContentView(Resource.Layout.Main);
            Intent intent = new Intent(this, typeof(MainActivity));
            StartActivity(intent);
        }

		private void Finished(Object sender, EventArgs e) {
			AlertDialog.Builder builder = new AlertDialog.Builder(this);
			builder.SetTitle("Please select desired reference file name(this file can be opened in excel");

			EditText input = new EditText(this);
			input.InputType = Android.Text.InputTypes.ClassText;
			builder.SetView(input);

			//only the positive button should do anything 
			builder.SetPositiveButton("OK", (see, ess) => {
				exportDataSets(input);
			});

			//this should just cancel
			builder.SetNegativeButton("Cancel", (afk, kfa) => {

			});

			//show dialog 
			Dialog diaglog = builder.Create();
			diaglog.Show();
		}
		#endregion

		#region Helper Functions
		// Try to decode a URI as a bitmap
		private Bitmap decodeUriAsBitmap(Uri uri)
        {
            Bitmap bitmap = null;
            try {
                //bitmap = BitmapFactory.DecodeFile(uri.Path);
                bitmap = BitmapFactory.DecodeStream(this.ContentResolver.OpenInputStream(uri));
            }
            catch (Java.IO.FileNotFoundException e) {
                e.PrintStackTrace();
                return null;
            }

            return bitmap;
        }

        // Convert RGB values to grayscale and put into list. 
        public int Grayscale(Color avgCol)
        {
            var red = Color.GetRedComponent(avgCol);
            var green = Color.GetGreenComponent(avgCol);
            var blue = Color.GetBlueComponent(avgCol);

            return (int)(red * .3 + green * .59 + blue * .11);
        }

        // Pretty self explanatory. Checks if the device has a camera for picture taking
        private bool IsThereAnAppToTakePictures()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
                PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        // Creates a directory to store images if it doesn't already exist.
        /*** THIS FUNCTION MAY NOT BE NEEDED LATER ON AS IMAGES MIGHT NOT NEED TO BE SAVED TO THE DEVICE ***/
		// It was needed
        private void CreateDirectoryForPictures()
        {
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
		private static Color CalculateAverageColor(Bitmap bm)
        {
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

			int total = bitmapHeight * bitmapWidth;

            for (int x = 0; x < bitmapWidth; x++) {
                for (int y = 0; y < bitmapHeight; y++) {
                    // Temporary variable to store the color of each pixel
                    int tmpColor = bm.GetPixel(x, y);

                    // Add RGB values for each pixel
                    red += Color.GetRedComponent(tmpColor);
                    green += Color.GetGreenComponent(tmpColor);
                    blue += Color.GetBlueComponent(tmpColor);

                    // Keep track of how many pixels processed
                    pixCount++;
                }
            }

            // Calculate Averages
            red /= pixCount;
            green /= pixCount;
            blue /= pixCount;

            return Android.Graphics.Color.Rgb(red, green, blue);
        }

        private void LabelData(Object s, EventArgs e)
        {
            var builder = new AlertDialog.Builder(this);
            builder.SetTitle("Label Data");

            EditText label = new EditText(this);

            builder.SetView(label);
            // Create empty event handlers, we will override them manually instead of letting the builder handling the clicks.
            builder.SetPositiveButton("Done", (EventHandler<DialogClickEventArgs>)null);
            builder.SetNegativeButton("Back", (EventHandler<DialogClickEventArgs>)null);
            var dialog = builder.Create();

            // Show the dialog. This is important to do before accessing the buttons.
            dialog.Show();

            // Get the buttons.
            var yesBtn = dialog.GetButton((int)DialogButtonType.Positive);
            var noBtn = dialog.GetButton((int)DialogButtonType.Negative);

            // Assign our handlers.
            yesBtn.Click += (sender, args) => {
                //labelButton.Visibility = Android.Views.ViewStates.Gone;
                //cropButton.Text = "Finished";
                //cropButton.Click += Finished;
                SetContentView(Resource.Layout.TakePicture);

                captureButton = FindViewById<ImageButton>(Resource.Id.picButton);
                captureButton.Click += TakeAPicture;

				importImageButton = FindViewById<Button>(Resource.Id.picImport);
				importImageButton.Click += importImage;

				importDataButton = FindViewById<Button>(Resource.Id.dataImport);
				importDataButton.Click += importData;

				numberOfPicturesLbl = FindViewById<TextView>(Resource.Id.numberOfPicturesLbl);
                numberOfPicturesLbl.Text = "Number of pictures: " + picCounter.ToString() + "\nTarget # of areas " + areaUpperLimit;

                if (picCounter >= 3) {
                    finishButton = FindViewById<Button>(Resource.Id.finishButton);
                    finishButton.Visibility = Android.Views.ViewStates.Visible;
                    finishButton.Click += Finished;
                }

                areaCounter = 0;

				//Add Image Tuple to data set
				////adder is needed as adding to a list is adding a reference and so when image tuple would be cleared the data in dataset would be cleared
				//List<double> adder = new List<double>(ImageTuple);
				//DataSet.Add(adder);
				//This actually wasnt needed 

				DataSet.Add(new List<double>(ImageTuple));
				Labels.Add(label.Text.Clone().ToString());

				exportData(label.Text,ImageTuple);

                //Reset Image Tuple
                ImageTuple.Clear();

				// Finish button enabled if 3 or more pictures have been taken and 
				// 3 or more datasets have been collected
				if (picCounter >= 3 /*&& datasetCounter >= 3*/) {
					finishButton.Enabled = true;
				}
				dialog.Dismiss();
            };
            noBtn.Click += (sender, args) => {
                // Dismiss dialog.
                dialog.Dismiss();
            };
        }

        private void Help(Object sender, EventArgs e)
        {
            AlertDialog.Builder helpAlert = new AlertDialog.Builder(this);

            helpAlert.SetTitle("Reference Mode Help");
            helpAlert.SetMessage(
            "Constraints:\n" +
            "\t1. At least 3 pictures must be taken.\n" +
            "\t2. At least 3 areas must be selected from each picture taken.\n" +
            "\nTaking a Picture:\n" +
            "\t1. Ensure protein tray is well lit.\n" +
            "\t2. Try to take the picture from a 90 degree angle above the tray. Avoid odd angles.\n" +
            "\t3. Avoid shadows on the tray.\n" +
            "\nArea Selection:\n" +
            "\t1. You must select the correct number of areas specified by the reference file. The number " +
            "of necessary areas will be shown by the area selection button.\n" +
            "\t2. All areas should be as close to the same size as possible.\n" +
            "\t3. Select areas in order starting from the upper left and moving to the right row by row.\n"+
			"\t1 If you cannot find your content in the content picker enable show internal storage by pressing the upper right hand corner of the picker"
                        );

            Dialog helpDialog = helpAlert.Create();
            helpDialog.Show();

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
				foreach (double number in line)	newLine.Add((number - mean) / stdDev);
				
				output.Add(newLine);
			}

			//Do Stuff Here

			return output;
		}

		#endregion

		#region Intents
		// Take a picture intent
		private void TakeAPicture(object sender, EventArgs eventArgs)
        {
			if(AppSettings.settings.ProfTutPicture) ProfTutPicture();
			else {
				Intent intent = new Intent(MediaStore.ActionImageCapture);
				AppSettings._file = new Java.IO.File(AppSettings.dirPhoto, String.Format("PCAPhoto_{0}.jpg", Guid.NewGuid()));
				intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(AppSettings._file));
				intent.PutExtra(MediaStore.ExtraSizeLimit, 256);
				StartActivityForResult(intent, CAMERA_CAPTURE);
			}
        }
		private void importImage(object sender, EventArgs e) {
			if(AppSettings.settings.ProfTutImportPhoto) ProfTutImportPhoto();
			else {
				Intent intent = new Intent();
				intent.SetType("image/*");
				intent.SetAction(Intent.ActionGetContent);

				StartActivityForResult(intent, GALLERY_PICK);
			}	
		}
		private void exportData(string label, List<double> areas) {
			// This will create a file/fragment 
			string path = string.Format("{1}DataFrag_{0}.csv", "Areas :" + areaUpperLimit + "_" + label,AppSettings.dirData.Path +"/");
			// write to file
			System.IO.File.WriteAllText(path, label + "\n");
			for (int i = 0; i < areas.Count - 1; i++) System.IO.File.AppendAllText(path, areas[i].ToString() + ",");
			System.IO.File.AppendAllText(path, areas[areas.Count - 1].ToString());
		}
		private void exportDataSets(EditText input) {
			#region nonnormalized
			//This is where we will tie in the PCA core
			///Its undecided if we want to create a new class that is just the PCA core output to file output, probably best way to do it 
			DataStructure.Data = DataSet;
			DataStructure.Labels = Labels;
			DataStructure.Process();
			DataWriter dataWriter = new DataWriter();

			//User Prompt
			string path = AppSettings.dirData.Path;
			string filePath;

			//get file path
			//get file path
			string name = "RefernceFile.txt";//Default value
			if (input.Text != "") name = "Reference_areas" + areaUpperLimit.ToString() + "_" + input.Text + ".csv";

			filePath = System.IO.Path.Combine(path, name);

			dataWriter.Filepath = filePath;
			//Dimension size 
			dataWriter.Dimensionsize = areaUpperLimit;
			dataWriter.NumberOfPics = Labels.Count;
			dataWriter.writeDimension();
			//Labels
			dataWriter.Labels = Labels;
			dataWriter.writeLabels();
			//using the final data from PCA forms the vectors. 
			dataWriter.FinalData = DataStructure.FinalDataRealigned;
			dataWriter.writeFinalDataRealigned();
			//Grab the vectors
			dataWriter.Vectors = DataStructure.FeatureVectors;
			dataWriter.writeVectors();

			//Clear the data structure afterwords 
			DataStructure.clear();
			#endregion

			#region normalized

			DataStructure.Data = normalizedSet(DataSet);
			DataStructure.Process();
			dataWriter.FinalData = DataStructure.FinalDataRealigned;
			dataWriter.writeFinalDataRealigned();
			//Grab the vectors
			dataWriter.Vectors = DataStructure.FeatureVectors;
			dataWriter.writeVectors();


			#endregion

			// Export a raw file which can then be added to later
			name = "DataSet_areas" + areaUpperLimit.ToString() + "_" + input.Text + ".csv";
			filePath = System.IO.Path.Combine(path, name);
			if (System.IO.File.Exists(path)) System.IO.File.Create(path);

			for (int i = 0; i < Labels.Count; i++) {
				System.IO.File.AppendAllText(filePath, Labels[i] + "\n");
			}
			foreach (List<double> item in DataSet) {
				for (int i = 0; i < item.Count - 1; i++) {
					System.IO.File.AppendAllText(filePath, item[i].ToString() + ",");
				}
				System.IO.File.AppendAllText(filePath, item[item.Count - 1].ToString() + "\n");
			}

			SetContentView(Resource.Layout.Main);
			Intent intent = new Intent(this, typeof(MainActivity));
			StartActivity(intent);
		}
		private void importData(object sender, EventArgs e) {
			if (AppSettings.settings.ProfTutImportFrag) ProfTutImportFrag();
			else {
				// This event handler will import created file/fragments
				Intent intent = new Intent();
				intent.SetType("text/comma-separated-values");
				intent.SetAction(Intent.ActionGetContent);

				StartActivityForResult(intent, DATA_IMPORT);
			}
		}
		private void importRawSet(object sender, EventArgs e) {
			if (AppSettings.settings.ProfTutImportRaw) ProfTutImportRaw();
			else {
				// This event handler will import created file/fragments
				Intent intent = new Intent();
				intent.SetType("text/comma-separated-values");
				intent.SetAction(Intent.ActionGetContent);

				StartActivityForResult(intent, DATA_IMPORT_RAW);
			}
		}
		// Crop, aka area selection, intent
		private void CropPic(object sender, EventArgs eventArgs)
        {
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
		#endregion

		#region OnActivity Result
		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {

            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == Result.Ok)
            {
				// User is returning from capturing an image using the camera
				if (requestCode == CAMERA_CAPTURE) resultCameraCapture(data);
				// This should be all that needs done to bind a selected image to the uri we use in crops. 
				else if (requestCode == GALLERY_PICK) resultGalleryPick(data);
				// Import a data fragment and integrate it into the current data set
				// Data check it first to make sure it conforms to the same number of areas
				else if (requestCode == DATA_IMPORT) resultDataImport(data);
				else if (requestCode == DATA_IMPORT_RAW) resultDataImportRaw(data);
				// User is returning from cropping the image
				else if (requestCode == CropImage.CropImageActivityRequestCode) resultCrop(data);
            }
        }
		
		private void resultCameraCapture (Intent data) {
			picCounter++;
			if (currentView == "takepicture") {
				numberOfPicturesLbl.Text = "Number of pictures: " + picCounter.ToString() + "\nTarget # of areas " + areaUpperLimit;
			}
			captureButton.Enabled = false;

			picUri = Uri.FromFile(AppSettings._file);

			// Make picture available in the gallery
			Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
			Uri contentUri = Uri.FromFile(AppSettings._file);
			mediaScanIntent.SetData(contentUri);
			SendBroadcast(mediaScanIntent);


			captureButton.Enabled = false;
			SetContentView(Resource.Layout.AreaSelection);

			// Enable area selection button

			helpBtn = FindViewById<ImageButton>(Resource.Id.helpButton);
			helpBtn.Click += Help;
			cropButton = FindViewById<Button>(Resource.Id.cropButton);
			selectAreaCounterLbl = FindViewById<TextView>(Resource.Id.selectAreaCounterLbl);
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

			// Dispose of Java side bitmap
			GC.Collect();
		}
		private void resultGalleryPick(Intent data) {
			picUri = data.Data;
			picCounter++;

			// Show picture on image view

			captureButton.Enabled = false;
			SetContentView(Resource.Layout.AreaSelection);

			// Enable area selection button

			helpBtn = FindViewById<ImageButton>(Resource.Id.helpButton);
			helpBtn.Click += Help;
			cropButton = FindViewById<Button>(Resource.Id.cropButton);
			selectAreaCounterLbl = FindViewById<TextView>(Resource.Id.selectAreaCounterLbl);
			cropButton.Click += CropPic;
			cropButton.Enabled = true;
			ImageView picView = FindViewById<ImageView>(Resource.Id.imageView1);

			picView.SetImageURI(picUri);
		}
		private void resultDataImport(Intent data) {
			string dataStr = data.Data.ToString();
			string path = data.Data.Path;
			path = getPath(path);

			path = "storage/emulated/0/" + path;

			if (path != "storage/emulated/0/") {
				string[] lines = System.IO.File.ReadAllLines(path);
				// Read in the 2nd line for number of areas recorded
				string[] numbers = lines[1].Split(',');
				// If it matches or if its the first one 
				if (numbers.Length == areaUpperLimit || areaUpperLimit == 0) {
					// Read the first line and put it in labels
					Labels.Add(lines[0]);
					// Put the already read 2nd line as a new image tuple 
					ImageTuple = new List<double>();
					foreach (string item in numbers) {
						ImageTuple.Add(double.Parse(item));
					}
					DataSet.Add(new List<double>(ImageTuple));
					ImageTuple.Clear();
					// Increment needed counters (picture count) 
					picCounter++;

					// If its the first image of a new set, set the area limit counter to the number of areas recorded
					if (areaUpperLimit == 0) areaUpperLimit = numbers.Length;

					// Finish button enabled if 3 or more pictures have been taken and 
					SetContentView(Resource.Layout.TakePicture);

					captureButton = FindViewById<ImageButton>(Resource.Id.picButton);
					captureButton.Click += TakeAPicture;

					importImageButton = FindViewById<Button>(Resource.Id.picImport);
					importImageButton.Click += importImage;

					importDataButton = FindViewById<Button>(Resource.Id.dataImport);
					importDataButton.Click += importData;

					numberOfPicturesLbl = FindViewById<TextView>(Resource.Id.numberOfPicturesLbl);
					numberOfPicturesLbl.Text = "Number of pictures: " + picCounter.ToString() + "\nTarget # of areas " + areaUpperLimit;

					if (picCounter >= 3) {
						finishButton = FindViewById<Button>(Resource.Id.finishButton);
						finishButton.Visibility = Android.Views.ViewStates.Visible;
						finishButton.Click += Finished;
					}
				}
				// If it doesnt 
				else if (numbers.Length != areaUpperLimit) {
					Toast.MakeText(Application.Context, "File does not match the number of areas cropped", ToastLength.Long).Show();
				}
				else {
					// Return everything to the state it was to start with and reset the buttons to the correct state
					Toast.MakeText(Application.Context, "File cannot be found", ToastLength.Long).Show();
				}
			}
			else {
				Toast.MakeText(Application.Context, "File does not match data fragment format or cannot be found", ToastLength.Long).Show();
			}
		}
		private string getPath(string fakePath) {
			//This could cause issues if the naming scheme changes later
			string output = "";

			Regex regex = new Regex(@"((PCA App/Data/DataFrag).*csv)");
			Match match = regex.Match(fakePath);
			output = match.Value;

			return output;
		}
		private void resultDataImportRaw(Intent data) {
			string dataStr = data.Data.ToString();
			string path = data.Data.Path;
			path = getPathRaw(path);

			path = "storage/emulated/0/" + path;

			if (path != "storage/emulated/0/") {
				string[] lines = System.IO.File.ReadAllLines(path);
				int half = lines.Length / 2; // This represents the change from labels to data
											 //get # of areas
				picCounter = half;
				string[] numbers = lines[half].Split(',');
				int areaLength = numbers.Length;
				if (areaUpperLimit == 0) areaUpperLimit = areaLength;

				for (int i = 0; i < half; i++) {
					Labels.Add(lines[i]);
				}

				for (int i = half; i < lines.Length; i++) {
					numbers = lines[i].Split(',');
					ImageTuple = new List<double>();
					foreach (string item in numbers) {
						ImageTuple.Add(double.Parse(item));
					}
					DataSet.Add(new List<double>(ImageTuple));
				}

				// ATTENTION 
				finishButton.Text = "Finished";
				finishButton.Visibility = Android.Views.ViewStates.Gone;
				finishButton.Click -= importRawSet;

				numberOfPicturesLbl = FindViewById<TextView>(Resource.Id.numberOfPicturesLbl);
				numberOfPicturesLbl.Text = "Number of pictures: " + picCounter.ToString() + "\nTarget # of areas " + areaUpperLimit;

				if (picCounter >= 3) {
					finishButton = FindViewById<Button>(Resource.Id.finishButton);
					finishButton.Visibility = Android.Views.ViewStates.Visible;
					finishButton.Click += Finished;
				}

			}
			else if (System.IO.File.Exists(path)) Toast.MakeText(Application.Context, "File does not Exist", ToastLength.Long).Show();
			else Toast.MakeText(Application.Context, "File does not match raw data set format", ToastLength.Long).Show();
		}
		private string getPathRaw(string fakePath) {
			//This could cause issues if the naming scheme changes later
			string output = "";

			Regex regex = new Regex(@"((PCA App/Data/DataSet).*csv)");
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

			// Send GrayScale values to the Data Set
			int gray = new int();
			gray = Grayscale(avgColorSmall);
			ImageTuple.Add(gray);

			// Update area selection counter
			selectAreaCounterLbl.Text = "Number of crops: " + areaCounter.ToString();

			// If first picture operate normally
			if (picCounter == 1) {
				// Update area selection counter
				//cropButton.Text = "Select Area (" + areaCounter + ")";
				if (areaCounter == 3) {
					selectAreaImageView = FindViewById<ImageView>(Resource.Id.imageView1);
					selectAreaImageView.LayoutParameters.Height = 500;
					selectAreaImageView.LayoutParameters.Width = 350;
					labelButton = FindViewById<Button>(Resource.Id.labelButton);
					labelButton.Click += LabelData;
					labelButton.Visibility = Android.Views.ViewStates.Visible;
					labelButton.Enabled = true;
				}
				areaUpperLimit = areaCounter;
			}
			// For every subsequent picture restrict number of areas to select
			else if (picCounter > 0) {
				// Update area selection counter with upper limit
				selectAreaCounterLbl.Text = "Number of crops: " + areaCounter + " / " + areaUpperLimit;
				if (areaCounter == areaUpperLimit) {

					selectAreaImageView = FindViewById<ImageView>(Resource.Id.imageView1);
					selectAreaImageView.LayoutParameters.Height = 500;
					selectAreaImageView.LayoutParameters.Width = 350;
					labelButton = FindViewById<Button>(Resource.Id.labelButton);
					labelButton.Click += LabelData;
					labelButton.Visibility = Android.Views.ViewStates.Visible;
					labelButton.Enabled = true;
					cropButton.Enabled = false;
				}
			}
		}
		#endregion

		#region TutorialMessages
		private void ProfTutInit() {
			AlertDialog.Builder helpAlert = new AlertDialog.Builder(this);

			helpAlert.SetTitle("Reference Mode Help");
			helpAlert.SetMessage("In this mode you will create the reference set by taking multiple pictures of different known items. \n" +
				"Make sure you select your areas in the same order each time, and try to take the pictures in the same way each time.");

			helpAlert.SetPositiveButton("Ok", (ak, ka) => {
				AppSettings.settings.ProfTutInit = false;
				AppSettings.settings.Write();
			});

			Dialog helpDialog = helpAlert.Create();
			helpDialog.Show();
		}
		private void ProfTutPicture() {
			AlertDialog.Builder helpAlert = new AlertDialog.Builder(this);

			helpAlert.SetTitle("Taking a picture");
			helpAlert.SetMessage("You will take a picture of an item. Try to take the picture in the same way as every other picture you\'ve taken." +
				"\nThen you will select areas of that item. Do this in the same order each time.");

			helpAlert.SetPositiveButton("Ok", (ak, ka) => {
				Intent intent = new Intent(MediaStore.ActionImageCapture);
				AppSettings._file = new Java.IO.File(AppSettings.dirPhoto, String.Format("PCAPhoto_{0}.jpg", Guid.NewGuid()));
				intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(AppSettings._file));
				intent.PutExtra(MediaStore.ExtraSizeLimit, 256);
				StartActivityForResult(intent, CAMERA_CAPTURE);

				AppSettings.settings.ProfTutPicture = false;
				AppSettings.settings.Write();
			});

			Dialog helpDialog = helpAlert.Create();
			helpDialog.Show();
		}
		private void ProfTutImportPhoto() {
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

				AppSettings.settings.ProfTutImportPhoto = false;
				AppSettings.settings.Write();
			});

			Dialog helpDialog = helpAlert.Create();
			helpDialog.Show();
		}
		private void ProfTutImportFrag() {
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

				StartActivityForResult(intent, DATA_IMPORT);
				AppSettings.settings.ProfTutImportFrag = false;
				AppSettings.settings.Write();
			});

			Dialog helpDialog = helpAlert.Create();
			helpDialog.Show();
		}
		private void ProfTutImportRaw() {
			AlertDialog.Builder helpAlert = new AlertDialog.Builder(this);

			helpAlert.SetTitle("Importing a data set");
			helpAlert.SetMessage("This will allow you to open up a previously completed data set so you can add to it." +
				"\nData is stored on internal storage, if you cannot find internal storage you will need to press the three dots on the upper right hand corner > show internal storage" +
				"\nYou will then need to browse to internal storage > PCA App > Data and select a file that is prefixed with \"DataSet\"");

			helpAlert.SetPositiveButton("Ok", (ak, ka) => {
				// This event handler will import created file/fragments
				Intent intent = new Intent();
				intent.SetType("text/comma-separated-values");
				intent.SetAction(Intent.ActionGetContent);

				StartActivityForResult(intent, DATA_IMPORT_RAW);
				AppSettings.settings.ProfTutImportRaw = false;
				AppSettings.settings.Write();
			});

			Dialog helpDialog = helpAlert.Create();
			helpDialog.Show();
		}
		#endregion
	}
}