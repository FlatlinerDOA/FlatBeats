namespace FlatBeats.Framework.Controls
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    using FlatBeats.DataModel;

    using Microsoft.Phone.Reactive;

    using ObservableExtensions = Microsoft.Phone.Reactive.ObservableExtensions;

    public class CensorBitmapConverter : IValueConverter
    {
        public int PixelWidth { get; set; }

        public int PixelHeight { get; set; }

        public int PixelSize { get; set; }

        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        /// <param name="value">The source data being passed to the target.</param><param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param><param name="parameter">An optional parameter to be used in the converter logic.</param><param name="culture">The culture of the conversion.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var url = GetUrl(value);
            if (url == null)
            {
                return null;
            }

            if (!url.OriginalString.EndsWith("nsfw"))
            {
                return new BitmapImage(url) { CreateOptions = BitmapCreateOptions.BackgroundCreation | BitmapCreateOptions.DelayCreation };
            }

            var safeUrl = new Uri(url.OriginalString.Remove(url.OriginalString.Length - 4, 4).TrimEnd('?'));
            var pixelated = new WriteableBitmap(this.PixelWidth, this.PixelHeight);
            Downloader.Instance.GetStreamAsync(safeUrl, false).ObserveOnDispatcher().Subscribe(b => this.Pixelate(pixelated, b, 0, 0, this.PixelWidth, this.PixelHeight, this.PixelSize));
            return pixelated;
        }

        private void Pixelate(WriteableBitmap target, Stream source, int startX, int startY, int width, int height, int pixelateSize)
        {
            target.SetSource(source);

            // look at every pixel in the rectangle while making sure we're within the image bounds
            var endY = Math.Min(startY + height, target.PixelHeight);
            var endX = Math.Min(startX + width, target.PixelWidth);
            var halfStep = pixelateSize / 2;

            for (var sourceY = startY + halfStep; sourceY < endY; sourceY += pixelateSize)
            {
                for (var sourceX = startX + halfStep; sourceX < endX; sourceX += pixelateSize)
                {
                    // get the pixel color in the center of the soon to be pixelated area
                    var pixelIndex = ((sourceY - 1) * target.PixelWidth) + sourceX;
                    int pixel = target.Pixels[pixelIndex];

                    // for each pixel in the pixelate size, set it to the center color
                    for (int setY = sourceY - halfStep; setY < Math.Min(sourceY + halfStep, endY); setY++)
                    {
                        for (int setX = sourceX - halfStep; setX < Math.Min(sourceX + halfStep, endX); setX++)
                        {
                            var setPixelIndex = (setY * target.PixelWidth) + setX;
                            target.Pixels[setPixelIndex] = pixel;
                        }
                    }
                }
            }
        }

        private Uri GetUrl(object value)
        {
            var uriValue = value as Uri;
            if (uriValue != null)
            {
                return uriValue;
                ///return new BitmapImage(uriValue) { CreateOptions = BitmapCreateOptions.BackgroundCreation };
            }

            var textValue = value.ToString();
            if (Uri.TryCreate(textValue, UriKind.Absolute, out uriValue))
            {
                return uriValue;
                ////return new BitmapImage(uriValue) { CreateOptions = BitmapCreateOptions.BackgroundCreation };
            }
            
            return null;
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        /// <param name="value">The target data being passed to the source.</param><param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param><param name="parameter">An optional parameter to be used in the converter logic.</param><param name="culture">The culture of the conversion.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}