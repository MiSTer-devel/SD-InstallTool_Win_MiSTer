#include "Export.h"
#include "VDSFormatVolume.h"

BOOL Format(
	PCWSTR letter,
	LPWSTR pwszFileSystemTypeName,
	UINT32 ulDesiredUnitAllocationSize,
	LPWSTR pwszLabel)
{
	BOOL result = FALSE;

	try
	{
		VDSFormatVolume* pVDSFV = new VDSFormatVolume();
		result = pVDSFV->formatVolume(
			letter,
			pwszFileSystemTypeName,
			ulDesiredUnitAllocationSize,
			pwszLabel);
	}
	catch (...)
	{
		OutputDebugString("MiSTerHelper: Global exception caught");
	}

	return result;
}