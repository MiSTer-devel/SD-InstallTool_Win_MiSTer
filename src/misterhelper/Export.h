#pragma once

#include <Windows.h>

/// Needs to be included. Otherwise LNK2001 error: "LNK2001 unresolved external symbol _CLSID_VdsLoader"
/// <remarks>StackOverflow: https://stackoverflow.com/questions/19766686/unresolved-external-symbol-clsid-vdsloader </remarks>
/// <remarks>Microsoft support: https://support.microsoft.com/en-us/help/130869/how-to-avoid-error-lnk2001-unresolved-external-by-using-define-guid </remarks>
#include <InitGuid.h>

extern "C"
{
	__declspec(dllexport) BOOL Format(
		PCWSTR letter,
		LPWSTR pwszFileSystemName,
		UINT32 nAllocationUnitSize,
		LPWSTR pwszLabel
	);
}
