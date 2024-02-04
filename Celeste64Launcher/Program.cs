using System.Diagnostics;
using System.Globalization;
using System.Text;

using Celeste64;
using Foster.Framework;

class Program
{
	// Copied from Celeste64 project
	public static void Main(string[] args)
	{
		Version loaderVersion = typeof(Program).Assembly.GetName().Version!;
		Game.LoaderVersion = $"Fuji: v.{loaderVersion.Major}.{loaderVersion.Minor}.{loaderVersion.Build}";
		if (!string.IsNullOrEmpty(BuildProperties.ModVersion()))
		{
			Game.LoaderVersion += "-" + BuildProperties.ModVersion();
		}
		Log.Info($"Celeste 64 v.{Game.GameVersion.Major}.{Game.GameVersion.Minor}.{Game.GameVersion.Build}");
		Log.Info(Game.LoaderVersion);

		AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
		{
			HandleError((Exception)e.ExceptionObject);
		};

		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
		CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
		CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

		try
		{
			App.Run<Game>(Game.GamePath, 1280, 720);
		}
		catch (Exception e)
		{
			HandleError(e);
		}
	}

	private static void HandleError(Exception e)
	{
		// write error to console in case they can see stdout
		Console.WriteLine(e?.ToString() ?? string.Empty);

		// construct a log message
		const string ErrorFileName = "ErrorLog.txt";
		StringBuilder error = new();
		error.AppendLine($"Celeste 64 v.{Game.GameVersion.Major}.{Game.GameVersion.Minor}.{Game.GameVersion.Build}");
		error.AppendLine(Game.LoaderVersion);
		error.AppendLine($"Error Log ({DateTime.Now})");
		error.AppendLine($"Call Stack:");
		error.AppendLine(e?.ToString() ?? string.Empty);
		error.AppendLine($"Game Output:");
		lock (Log.Logs)
			error.AppendLine(Log.Logs.ToString());

		// write to file
		string path = ErrorFileName;
		{
			if (App.Running)
			{
				try
				{
					path = Path.Join(App.UserPath, ErrorFileName);
				}
				catch
				{
					path = ErrorFileName;
				}
			}

			File.WriteAllText(path, error.ToString());
		}

		// open the file
		if (File.Exists(path))
		{
			new Process { StartInfo = new ProcessStartInfo(path) { UseShellExecute = true } }.Start();
		}
	}
}