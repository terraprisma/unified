using System;
using System.Runtime.InteropServices;

namespace ReLogic.OS.Windows;

partial class NativeMethods
{
	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

	[DllImport("user32.dll")]
	public static extern IntPtr GetDC(IntPtr hWnd);
}
