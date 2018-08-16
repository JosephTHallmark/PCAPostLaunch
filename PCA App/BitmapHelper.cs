using Android.Graphics;
using Android.Media;

namespace PCAapp
{
    public static class BitmapHelper
    {
        public static Bitmap LoadandResizeBitmap(this string fileName, int width, int height)
        {
            // First get teh dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options
            {
                InJustDecodeBounds = true
            };
            BitmapFactory.DecodeFile(fileName, options);

            // Next calculate the ratio that we need to resize the image by
            // in order to fit the requested dimensions
            int outHeight = options.OutHeight;
            int outWidth = options.OutWidth;
            int inSampleSize = 1;

            if (outHeight > height || outWidth > width)
            {
                inSampleSize = outWidth > outHeight 
                               ? outHeight / height 
                               : outWidth / width;
            }

            // Now load the image and have BitmapFactory resize it
            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

            // Images are being saved in landscape, so rotate them back to protrait if they
            // were taken in portrait
            Matrix mtx = new Matrix();
            ExifInterface exif = new ExifInterface(fileName);
            string orientation = exif.GetAttribute(ExifInterface.TagOrientation);

            switch(orientation)
            {
                case "6": // Portrait
                    mtx.PreRotate(90);
                    resizedBitmap = Bitmap.CreateBitmap(resizedBitmap, 0, 0, resizedBitmap.Width,
                                                        resizedBitmap.Height, mtx, false);
                    mtx.Dispose();
                    mtx = null;
                    break;
                case "1": // Landscape
                    break;
                default:
                    mtx.PreRotate(90);
                    resizedBitmap = Bitmap.CreateBitmap(resizedBitmap, 0, 0, resizedBitmap.Width,
                                                        resizedBitmap.Height, mtx, false);
                    mtx.Dispose();
                    mtx = null;
                    break;
            }

            return resizedBitmap;  
            }
        }
    }


