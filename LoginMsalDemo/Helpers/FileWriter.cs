using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;

namespace LoginMsalDemo.Helpers;

public class FileWriter
{
    public static async Task<string> WriteBitmapAsync(Stream stream, string nameWithExt)
    {
        var storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            nameWithExt,
            CreationCollisionOption.ReplaceExisting);

        // ref: https://codedocu.com/Details?d=1592&a=9&f=181&l=0&v=d
        using var s = stream.AsRandomAccessStream();
        // Create the decoder from the stream
        var decoder = await BitmapDecoder.CreateAsync(s);

        // Get the SoftwareBitmap representation of the file
        var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId,
            await storageFile.OpenAsync(FileAccessMode.ReadWrite));

        encoder.SetSoftwareBitmap(softwareBitmap);

        await encoder.FlushAsync();

        return storageFile.Path;
    }
}