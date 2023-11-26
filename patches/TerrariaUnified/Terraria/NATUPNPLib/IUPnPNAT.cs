using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NATUPNPLib;

[ComImport]
[CompilerGenerated]
[Guid("B171C812-CC76-485A-94D8-B6B3A2794E99")]
[TypeIdentifier]
public interface IUPnPNAT
{
	[DispId(1)]
	IStaticPortMappingCollection StaticPortMappingCollection {
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		[return: MarshalAs(UnmanagedType.Interface)]
		get;
	}
}
