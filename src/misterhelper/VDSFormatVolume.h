#pragma once
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
};

