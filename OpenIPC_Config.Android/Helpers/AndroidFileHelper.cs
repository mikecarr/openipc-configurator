using System;
using System.IO;
using Android.Content;
using Android.Content.Res;
using Android.Util;

namespace OpenIPC_Config.Android.Helpers;

public class AndroidFileHelper
{
    public static string ReadAssetFile(string relativePath)
    {
        // var assets = Android.App.Application.Context.Assets;
        var assets = global::Android.App.Application.Context.Assets;
        
        using (var stream = assets.Open(relativePath))
        using (var reader = new StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }

    public static string[] ListAssetFiles(string folderPath)
    {
        var assets = global::Android.App.Application.Context.Assets;
        return assets.List(folderPath); // Lists all files in the folder
    }
    
    public static void CopyAssetsToInternalStorage(Context context)
    {
        AssetManager assets = context.Assets;
        string internalStoragePath = context.FilesDir.AbsolutePath; // Internal storage path

        // Start recursive copy from the root "binaries" folder
        CopyFolder(assets, "binaries", internalStoragePath);
    }

    private static void CopyFolder(AssetManager assets, string sourceFolder, string destinationFolder)
    {
        try
        {
            // Ensure the destination folder exists
            Directory.CreateDirectory(destinationFolder);

            // List all items (files and folders) in the source folder
            string[] items = assets.List(sourceFolder);

            foreach (string item in items)
            {
                string sourcePath = string.IsNullOrEmpty(sourceFolder) ? item : $"{sourceFolder}/{item}";
                string destinationPath = Path.Combine(destinationFolder, item);

                // Check if the item is a directory or file
                if (assets.List(sourcePath).Length > 0)
                {
                    // If it's a folder, recursively copy its contents
                    CopyFolder(assets, sourcePath, destinationPath);
                }
                else
                {
                    // If it's a file, copy it
                    using (Stream input = assets.Open(sourcePath))
                    using (FileStream output = new FileStream(destinationPath, FileMode.Create))
                    {
                        input.CopyTo(output);
                    }

                    Log.Debug("FileHelper", $"Copied file: {sourcePath} to {destinationPath}");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("FileHelper", $"Error copying assets: {ex.Message}");
        }
    }
}