using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MsBox.Avalonia;
using OpenIPC_Config.Models;
using Prism.Events;
using Renci.SshNet;
using Serilog;

namespace OpenIPC_Config.Services;

public class SshClientService : ISshClientService
{
    private IEventSubscriptionService _eventSubscriptionService;

    public SshClientService(IEventSubscriptionService eventSubscriptionService)
    {
        _eventSubscriptionService = eventSubscriptionService;
    }

    public async Task<SshCommand> ExecuteCommandWithResponse(DeviceConfig deviceConfig, string command,
        CancellationToken cancellationToken = default)
    {
        Log.Debug($"Executing command: '{command}' on {deviceConfig.IpAddress}.");

        var connectionInfo = new ConnectionInfo(deviceConfig.IpAddress, deviceConfig.Port, deviceConfig.Username,
            new PasswordAuthenticationMethod(deviceConfig.Username, deviceConfig.Password));

        using (var client = new SshClient(connectionInfo))
        {
            try
            {
                await client.ConnectAsync(cancellationToken);
                //client.Connect();
                var result = client.RunCommand(command);
                Log.Debug($"Command executed successfully. Result: {result.Result}, Exit code: {result.ExitStatus}");
                return result;
            }
            catch (Exception ex)
            {
                Log.Debug($"Error executing command: {ex.Message}");
                return null;
            }
            finally
            {
                client.Disconnect();
            }
        }
    }

    public async Task ExecuteCommandAsync(DeviceConfig deviceConfig, string command)
    {
        Log.Debug($"Executing command: '{command}' on {deviceConfig.IpAddress}.");

        var connectionInfo = new ConnectionInfo(deviceConfig.IpAddress, deviceConfig.Port, deviceConfig.Username,
            new PasswordAuthenticationMethod(deviceConfig.Username, deviceConfig.Password));
        using (var client = new SshClient(connectionInfo))
        {
            try
            {
                client.Connect();
                var result = client.RunCommand(command);
                Log.Information(
                    $"Command executed successfully. Result: {result.Result}, Exit code: {result.ExitStatus}");
            }
            catch (Exception ex)
            {
                Log.Error($"Error executing command: {ex.Message}");
            }
            finally
            {
                client.Disconnect();
            }
        }
    }


    public async Task UploadFileAsync(DeviceConfig deviceConfig, string localFilePath, string remotePath)
    {
        Log.Information($"Uploading file '{localFilePath}' to '{remotePath}' on {deviceConfig.IpAddress}.");

        await Task.Run(() =>
        {
            var connectionInfo = new ConnectionInfo(deviceConfig.IpAddress, deviceConfig.Port, deviceConfig.Username,
                new PasswordAuthenticationMethod(deviceConfig.Username, deviceConfig.Password));
            using (var client = new ScpClient(connectionInfo))
            {
                try
                {
                    client.Connect();

                    using (var fileStream = new FileStream(localFilePath, FileMode.Open))
                    {
                        client.Upload(fileStream, remotePath);
                        Log.Information("File uploaded successfully.");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error uploading file: {ex.Message}");
                }
                finally
                {
                    client.Disconnect();
                }
            }
        });
    }

    public void UploadFile(DeviceConfig deviceConfig, string localFilePath, string remotePath)
    {
        Log.Information($"Uploading file '{localFilePath}' to '{remotePath}' on {deviceConfig.IpAddress}.");

        var connectionInfo = new ConnectionInfo(deviceConfig.IpAddress, deviceConfig.Port, deviceConfig.Username,
            new PasswordAuthenticationMethod(deviceConfig.Username, deviceConfig.Password));
        using (var client = new ScpClient(connectionInfo))
        {
            try
            {
                client.Connect();

                using (var fileStream = new FileStream(localFilePath, FileMode.Open))
                {
                    client.Upload(fileStream, remotePath);
                    Log.Information("File uploaded successfully.");
                }
            }
            catch (Exception ex)
            {
                Log.Information($"Error uploading file: {ex.Message}");
            }
            finally
            {
                client.Disconnect();
            }
        }
    }

    public async Task UploadDirectoryAsync(DeviceConfig deviceConfig, string localDirectory, string remoteDirectory)
    {
        Log.Information($"Uploading directory '{localDirectory}' to '{remoteDirectory}' on {deviceConfig.IpAddress}.");

        await Task.Run(() =>
        {
            var connectionInfo = new ConnectionInfo(deviceConfig.IpAddress, deviceConfig.Port, deviceConfig.Username,
                new PasswordAuthenticationMethod(deviceConfig.Username, deviceConfig.Password));
            using (var client = new ScpClient(connectionInfo))
            {
                try
                {
                    client.Connect();
                    UploadDirectoryRecursively(client, localDirectory, remoteDirectory);
                    Log.Information("Directory uploaded successfully.");
                }
                catch (Exception ex)
                {
                    Log.Error($"Error uploading directory: {ex.Message}");
                }
                finally
                {
                    client.Disconnect();
                }
            }
        });
    }

    public async Task DownloadDirectoryAsync(DeviceConfig deviceConfig, string remoteDirectory, string localDirectory)
    {
        Log.Information(
            $"Downloading directory '{remoteDirectory}' to '{localDirectory}' on {deviceConfig.IpAddress}.");

        await Task.Run(() =>
        {
            var connectionInfo = new ConnectionInfo(deviceConfig.IpAddress, deviceConfig.Port, deviceConfig.Username,
                new PasswordAuthenticationMethod(deviceConfig.Username, deviceConfig.Password));
            using (var client = new ScpClient(connectionInfo))
            {
                try
                {
                    client.Connect();
                    DownloadDirectoryRecursively(client, remoteDirectory, localDirectory);
                    Log.Information("Directory downloaded successfully.");
                }
                catch (Exception ex)
                {
                    Log.Information($"Error downloading directory: {ex.Message}");
                }
                finally
                {
                    client.Disconnect();
                }
            }
        });
    }
    
    public async Task<byte[]> DownloadFileBytesAsync(DeviceConfig deviceConfig, string remotePath)
    {
        Log.Information($"Downloading file from '{remotePath}' on {deviceConfig.IpAddress}.");

        byte[] fileContent = Array.Empty<byte>(); // Initialize an empty byte array

        await Task.Run(() =>
        {
            var connectionInfo = new ConnectionInfo(deviceConfig.IpAddress, deviceConfig.Port, deviceConfig.Username,
                new PasswordAuthenticationMethod(deviceConfig.Username, deviceConfig.Password));
            using (var client = new ScpClient(connectionInfo))
            {
                try
                {
                    client.Connect();
                    using (var memoryStream = new MemoryStream())
                    {
                        // Download the file content into a MemoryStream using ScpClient
                        client.Download(remotePath, memoryStream);
                        fileContent = memoryStream.ToArray(); // Get the file content as a byte array
                        Log.Information("File downloaded successfully.");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error downloading file: {ex.Message}");
                }
                finally
                {
                    client.Disconnect();
                }
            }
        });

        return fileContent; // Return the file content as a byte array
    }


    /// <summary>
    ///     Downloads a file from the device at <paramref name="remotePath" /> and returns its content as a string.
    /// </summary>
    /// <param name="deviceConfig">The device configuration to use for the download.</param>
    /// <param name="remotePath">The path to the file on the device to download.</param>
    /// <returns>The content of the file as a string.</returns>
    public async Task<string> DownloadFileAsync(DeviceConfig deviceConfig, string remotePath)
    {
        Log.Information($"Downloading file from '{remotePath}' on {deviceConfig.IpAddress}.");
    
        var fileContent = string.Empty;
    
        await Task.Run(() =>
        {
            var connectionInfo = new ConnectionInfo(deviceConfig.IpAddress, deviceConfig.Port, deviceConfig.Username,
                new PasswordAuthenticationMethod(deviceConfig.Username, deviceConfig.Password));
            using (var client = new ScpClient(connectionInfo))
            {
                try
                {
                    client.Connect();
                    using (var memoryStream = new MemoryStream())
                    {
                        // Download the file content into a MemoryStream using ScpClient
                        client.Download(remotePath, memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin); // Ensure the stream position is at the beginning
                        fileContent = Encoding.UTF8.GetString(memoryStream.ToArray());
                        Log.Information("File downloaded successfully.");
                    }
                }
                catch (Exception ex)
                {
                    Log.Information($"Error downloading file: {ex.Message}");
                }
                finally
                {
                    client.Disconnect();
                }
            }
        });
    
        return fileContent; // Return the content of the file
    }


    public async Task UploadBinaryAsync(DeviceConfig deviceConfig, string remoteDirectory, string fileName)
    {
        await UploadBinaryAsync(deviceConfig, remoteDirectory, Models.OpenIPC.FileType.Normal, fileName);
    }

    public async Task UploadBinaryAsync(DeviceConfig deviceConfig, string remoteDirectory,
        Models.OpenIPC.FileType fileType, string fileName)
    {
        var binariesFolderPath = OpenIPC.GetBinariesPath();
        Log.Debug($"Binaries folder path: {binariesFolderPath}");
        var filePath = string.Empty;
        var remoteFilePath = string.Empty;

        switch (fileType)
        {
            case Models.OpenIPC.FileType.Normal:
                filePath = Path.Combine(binariesFolderPath, fileName);
                remoteFilePath = Path.Combine(remoteDirectory, fileName);
                break;
            case Models.OpenIPC.FileType.Sensors:
                filePath = Path.Combine(binariesFolderPath, "sensors/", fileName);
                remoteFilePath = Path.Combine(remoteDirectory, fileName);
                break;
            case Models.OpenIPC.FileType.BetaFlightFonts:
                filePath = Path.Combine(binariesFolderPath, "fonts/bf/", fileName);
                remoteFilePath = Path.Combine(remoteDirectory, fileName);
                break;
            case Models.OpenIPC.FileType.iNavFonts:
                filePath = Path.Combine(binariesFolderPath, "fonts/inav/", fileName);
                remoteFilePath = Path.Combine(remoteDirectory, fileName);
                break;
        }

        Log.Debug($"file path: {filePath}");
        if (File.Exists(filePath))
        {
            Log.Information($"Uploading {fileName} to {remoteFilePath}...");
            await UploadFileAsync(deviceConfig, filePath, remoteFilePath);
            Log.Information($"Uploaded {fileName} successfully.");
        }
        else
        {
            var msgBox = MessageBoxManager.GetMessageBoxStandard("Error", $"File {fileName} not found.");
            msgBox.ShowAsync();
        }
    }

    public async Task DownloadFileLocalAsync(DeviceConfig deviceConfig, string remotePath, string localPath)
    {
        try
        {
            var fileContentBytes = await DownloadFileAsync(deviceConfig, remotePath);
            
            if (fileContentBytes.Length != 0) File.WriteAllText(localPath, fileContentBytes);
        }
        catch (Exception ex)
        {
            Log.Information($"Error downloading file: {ex.Message}");
        }
    }

    public async Task UploadFileStringAsync(DeviceConfig deviceConfig, string remotePath, string fileContent)
    {
        Log.Information($"Uploading content to '{remotePath}' on {deviceConfig.IpAddress}.");

        await Task.Run(() =>
        {
            var connectionInfo = new ConnectionInfo(deviceConfig.IpAddress, deviceConfig.Port, deviceConfig.Username,
                new PasswordAuthenticationMethod(deviceConfig.Username, deviceConfig.Password));
            using (var client = new ScpClient(connectionInfo))
            {
                try
                {
                    client.Connect();

                    // Convert the string content to a byte stream
                    using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent)))
                    {
                        // SCP does not directly support uploading from memory streams, so we need to save to a temporary file first
                        var tempFilePath = Path.GetTempFileName();
                        File.WriteAllBytes(tempFilePath, memoryStream.ToArray());

                        // Upload the temporary file
                        client.Upload(new FileInfo(tempFilePath), remotePath);
                        Log.Information("File uploaded successfully.");

                        // Delete the temporary file
                        File.Delete(tempFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Log.Information($"Error uploading file: {ex.Message}");
                }
                finally
                {
                    client.Disconnect();
                }
            }
        });
    }

    public void UploadDirectoryRecursively(ScpClient client, string localDirectory, string remoteDirectory)
    {
        using (var sshClient = new SshClient(client.ConnectionInfo))
        {
            sshClient.Connect();
            sshClient.RunCommand($"mkdir -p {remoteDirectory}");
            sshClient.Disconnect();
        }

        var files = Directory.GetFiles(localDirectory);
        foreach (var file in files)
        {
            var remoteFilePath = Path.Combine(remoteDirectory, Path.GetFileName(file)).Replace("\\", "/");
            Log.Information($"Uploading file {file} to {remoteFilePath}");
            using (var fileStream = new FileStream(file, FileMode.Open))
            {
                client.Upload(fileStream, remoteFilePath);
            }
        }

        var directories = Directory.GetDirectories(localDirectory);
        foreach (var directory in directories)
        {
            var remoteSubDirectory = Path.Combine(remoteDirectory, Path.GetFileName(directory)).Replace("\\", "/");
            UploadDirectoryRecursively(client, directory, remoteSubDirectory);
        }
    }

    public void DownloadDirectoryRecursively(ScpClient client, string remoteDirectory, string localDirectory)
    {
        if (!Directory.Exists(localDirectory)) Directory.CreateDirectory(localDirectory);

        using (var sshClient = new SshClient(client.ConnectionInfo))
        {
            sshClient.Connect();
            var command = sshClient.RunCommand($"ls -F {remoteDirectory}");
            sshClient.Disconnect();

            var files = command.Result.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var file in files)
                if (file.EndsWith("/"))
                {
                    var remoteSubDirectory = $"{remoteDirectory}/{file.TrimEnd('/')}";
                    var localSubDirectory = Path.Combine(localDirectory, file.TrimEnd('/'));
                    DownloadDirectoryRecursively(client, remoteSubDirectory, localSubDirectory);
                }
                else
                {
                    var remoteFilePath = $"{remoteDirectory}/{file}";
                    var localFilePath = Path.Combine(localDirectory, file);
                    Log.Information($"Downloading file {remoteFilePath} to {localFilePath}");

                    using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                    {
                        client.Download(remoteFilePath, fileStream);
                    }
                }
        }
    }
    
    // This is used for sysupgrade commands
    public async Task ExecuteCommandWithProgressAsync(DeviceConfig deviceConfig, string command, Action<string> updateProgress, CancellationToken cancellationToken = default)
    {
        var connectionInfo = new ConnectionInfo(deviceConfig.IpAddress, deviceConfig.Port, deviceConfig.Username,
            new PasswordAuthenticationMethod(deviceConfig.Username, deviceConfig.Password));

        using var client = new SshClient(connectionInfo);
        try
        {
            client.Connect();
            if (!client.IsConnected)
            {
                updateProgress("Unable to connect to the host.");
                return;
            }

            using var shellStream = client.CreateShellStream("commands", 80, 24, 800, 600, 1024);
            shellStream.WriteLine(command);

            var outputBuffer = new StringBuilder();
            bool commandCompleted = false;

            // Set a timeout for the command execution (e.g., 2 minutes)
            var timeout = TimeSpan.FromMinutes(1);
            var startTime = DateTime.UtcNow;

            while (!cancellationToken.IsCancellationRequested && !commandCompleted)
            {
                if (!client.IsConnected)
                {
                    updateProgress("Connection lost.");
                    break;
                }

                Log.Debug("*******************");
                // Check for timeout
                if (DateTime.UtcNow - startTime > timeout)
                {
                    updateProgress("Command execution timed out.");
                    break;
                }

                // Read available data
                while (shellStream.DataAvailable)
                {
                    var output = shellStream.ReadLine();
                    if (output != null)
                    {
                        outputBuffer.AppendLine(output);
                        updateProgress(output);

                        // Check for the end condition
                        if (output.Contains("Unconditional reboot"))
                        {
                            commandCompleted = true;
                            updateProgress("Reboot detected. Command execution completed.");
                            break;
                        }
                        if (output.Contains("Arguments written"))
                        {
                            commandCompleted = true;
                            updateProgress("Reboot detected. Command execution completed.");
                            break;
                        }
                         
                        
                        await Task.Delay(100); // Non-blocking pause
                        Log.Debug("Waiting for command to complete...");
                        
                    }
                }

                // Wait a bit before checking again to avoid a tight loop
                await Task.Delay(100, cancellationToken);
            }

            if (!commandCompleted)
            {
                updateProgress("Command did not complete successfully.");
            }
        }
        catch (OperationCanceledException)
        {
            updateProgress("Operation canceled by user.");
        }
        catch (Exception ex)
        {
            updateProgress($"Error: {ex.Message}");
        }
        finally
        {
            client.Disconnect();
        }
    }


}