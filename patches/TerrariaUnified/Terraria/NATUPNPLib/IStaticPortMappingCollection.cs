using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NATUPNPLib;

[ComImport]
[CompilerGenerated]
[Guid("CD1F3E77-66D6-4664-82C7-36DBB641D0F1")]
[DefaultMember("Item")]
[TypeIdentifier]
public interface IStaticPortMappingCollection : IEnumerable
{
	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(-4)]
	[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType = "System.Runtime.InteropServices.CustomMarshalers.EnumeratorToEnumVariantMarshaler, CustomMarshalers, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	new IEnumerator GetEnumerator();

	void _VtblGap1_2();

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(2)]
	void Remove([In] int lExternalPort, [In][MarshalAs(UnmanagedType.BStr)] string bstrProtocol);

	[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(3)]
	[return: MarshalAs(UnmanagedType.Interface)]
	IStaticPortMapping Add([In] int lExternalPort, [In][MarshalAs(UnmanagedType.BStr)] string bstrProtocol, [In] int lInternalPort, [In][MarshalAs(UnmanagedType.BStr)] string bstrInternalClient, [In] bool bEnabled, [In][MarshalAs(UnmanagedType.BStr)] string bstrDescription);
}
