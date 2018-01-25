#include "Export.h"
#include "VDSFormatVolume.h"

#pragma comment(lib, "rpcrt4.lib")

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

BOOL FormatVolume(
	LPWSTR pwszVolumeID,
	LPWSTR pwszFileSystemTypeName,
	UINT32 ulDesiredUnitAllocationSize,
	LPWSTR pwszLabel)
{
	BOOL result = FALSE;

	try
	{
		VDSFormatVolume* pVDSFV = new VDSFormatVolume();
		result = pVDSFV->formatByVolumeID(
			pwszVolumeID,
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