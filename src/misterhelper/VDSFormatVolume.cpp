#include "VDSFormatVolume.h"

#define SAFE_RELEASE(x)     if (x) { x->Release(); x = NULL; }
#define SAFE_FREE(x)        if (x) { CoTaskMemFree(x); }

VDSFormatVolume::VDSFormatVolume()
{
}

VDSFormatVolume::~VDSFormatVolume()
{
}

BOOL VDSFormatVolume::formatVolume(
	LPCWSTR letter,
	LPWSTR pwszFileSystemTypeName,
	UINT32 ulDesiredUnitAllocationSize,
	LPWSTR pwszLabel)
{
	BOOL result = FALSE;

	HRESULT hr;
	HRESULT hrAsync;

	IVdsServiceLoader *pLoader;
	IVdsService *pService;
	IUnknown *pUnk;
	IVdsVolume *pVolume;
	IVdsVolumeMF3 *pVolumeMF3;
	IVdsAsync *pAsync;

	VDS_ASYNC_OUTPUT AsyncOut;

	hr = CoInitialize(NULL); // Will fail when called from C# since thread was already initialized from there

	if (true)
	{
		// Create a loader instance
		hr = CoCreateInstance(CLSID_VdsLoader,
			NULL,
			CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER,
			IID_IVdsServiceLoader,
			(void **)&pLoader
		);

		if (SUCCEEDED(hr))
		{
			// Load the service on the machine.
			hr = pLoader->LoadService(NULL, &pService);
			SAFE_RELEASE(pLoader);
			pLoader = NULL;

			if (SUCCEEDED(hr))
			{
				// Access to volume interface via drive letter
				VDS_DRIVE_LETTER_PROP mDriveLetterPropArray[1];
				hr = pService->QueryDriveLetters(letter[0], 1, mDriveLetterPropArray);

				if (SUCCEEDED(hr))
				{
					hr = pService->GetObject(mDriveLetterPropArray->volumeId, VDS_OT_VOLUME, &pUnk);

					if (SUCCEEDED(hr))
					{
						hr = pUnk->QueryInterface(IID_IVdsVolume, (void **)&pVolume);

						if (SUCCEEDED(hr))
						{
							// Access volume format interface
							hr = pVolume->QueryInterface(IID_IVdsVolumeMF3, (void **)&pVolumeMF3);

							if (SUCCEEDED(hr))
							{
								// Execute format operation
								hr = pVolumeMF3->FormatEx2(
									pwszFileSystemTypeName,
									1,
									ulDesiredUnitAllocationSize,
									pwszLabel,
									VDS_FSOF_QUICK | VDS_FSOF_FORCE,
									&pAsync);
								hr = pAsync->Wait(&hrAsync, &AsyncOut);

#pragma region Error handling
								if (FAILED(hr))
								{
									OutputDebugString("MiSTerHelper: Failed to wait for asynchronous volume format completion");
								}
								else if (FAILED(hrAsync))
								{
									switch (hrAsync)
									{
									case VDS_E_NOT_SUPPORTED:
										OutputDebugString("MiSTerHelper: The operation is not supported by the object");
										break;
									case VDS_E_ACCESS_DENIED:
										OutputDebugString("MiSTerHelper: Access denied");
										break;
									case VDS_E_ACTIVE_PARTITION:
										break;
									default:
										OutputDebugString("MiSTerHelper: Error occurred FormatEx2");
										break;
									}
								}

								if (SUCCEEDED(hr))
								{
									result = TRUE;
								}
#pragma endregion

								SAFE_RELEASE(pVolumeMF3);
							}

							SAFE_RELEASE(pVolume);
						}

						SAFE_RELEASE(pUnk);
					}
				}

				SAFE_RELEASE(pService);
			}
		}

		CoUninitialize();
	}

	return result;
}
