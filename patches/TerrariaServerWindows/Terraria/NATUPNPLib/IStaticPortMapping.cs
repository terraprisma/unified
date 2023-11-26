using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NATUPNPLib;

[ComImport]
[CompilerGenerated]
[Guid("6F10711F-729B-41E5-93B8-F21D0F818DF1")]
[TypeIdentifier]
public interface IStaticPortMapping
{
	void _VtblGap1_2();

	[DispId(3)]
	int InternalPort {
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(3)]
		get;
	}

	[DispId(4)]
	string Protocol {
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(4)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(5)]
	string InternalClient {
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(5)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}
}
