#pragma once

#include <stdlib.h>
#include <Windows.h>
#include <vds.h>

class VDSFormatVolume
{
public:
	VDSFormatVolume();
	virtual ~VDSFormatVolume();

	BOOL formatVolume(
		LPCWSTR letter,
		LPWSTR pwszFileSystemTypeName,
		UINT32 ulDesiredUnitAllocationSize,
		LPWSTR pwszLabel);

	BOOL formatByVolumeID(
		LPWSTR pwszVolumeID,
		LPWSTR pwszFileSystemTypeName,
		UINT32 ulDesiredUnitAllocationSize,
		LPWSTR pwszLabel);
};

