using System.Runtime.InteropServices;

namespace Celeste64.Launcher;

public static class ConsoleHelper
{
	// P/Invoke required:
	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern IntPtr GetStdHandle(int nStdHandle);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);
	[DllImport("kernel32")]
	static extern bool AllocConsole();

	public const int STD_OUTPUT_HANDLE = -11;

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	public static extern IntPtr CreateFile([MarshalAs(UnmanagedType.LPTStr)] string filename,
										   [MarshalAs(UnmanagedType.U4)] uint access,
										   [MarshalAs(UnmanagedType.U4)] FileShare share,
																			 IntPtr securityAttributes,
										   [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
										   [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
																			 IntPtr templateFile);

	public const uint GENERIC_WRITE = 0x40000000;
	public const uint GENERIC_READ = 0x80000000;

	public static void CreateConsole()
	{
		AllocConsole();
		var hOut = GetStdHandle(STD_OUTPUT_HANDLE);
		var hRealOut = CreateFile("CONOUT$", GENERIC_READ | GENERIC_WRITE, FileShare.Write, IntPtr.Zero, FileMode.OpenOrCreate, 0, IntPtr.Zero);
		if (hRealOut != hOut)
		{
			SetStdHandle(STD_OUTPUT_HANDLE, hRealOut);
			Console.SetOut(new StreamWriter(Console.OpenStandardOutput(), Console.OutputEncoding) { AutoFlush = true });
		}
	}
}