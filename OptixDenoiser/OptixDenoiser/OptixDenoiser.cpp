#include <optix.h>
#include <optix_function_table_definition.h>
#include <optix_stubs.h>
#include <cuda_runtime.h>

#include "OptixDenoiser.h"

OPTIXDENOISER_API OptixDeviceContext createContext(OptixLogCallback logCallbackFunction, int logLevel)
{
	OptixDeviceContext context = nullptr;
	{
		cudaError_t error;
		if ((error = cudaFree(0)) != cudaSuccess)
		{
			if (logCallbackFunction != nullptr)
				logCallbackFunction(1, "CUDA Error", cudaGetErrorString(error), nullptr);
			return nullptr;
		}

		CUcontext cuCtx = 0;  // zero means take the current context
		OptixResult result = optixInit();
		if (result != OPTIX_SUCCESS)
		{
			if (logCallbackFunction != nullptr)
				logCallbackFunction(1, optixGetErrorName(result), optixGetErrorString(result), nullptr);
			return nullptr;
		}
		OptixDeviceContextOptions options = {};
		options.logCallbackFunction = logCallbackFunction;
		options.logCallbackLevel = logLevel;
		result = optixDeviceContextCreate(cuCtx, &options, &context);
		if (result != OPTIX_SUCCESS)
		{
			if (logCallbackFunction != nullptr)
				logCallbackFunction(1, optixGetErrorName(result), optixGetErrorString(result), nullptr);
			return nullptr;
		}
	}
	return context;
}

OPTIXDENOISER_API OptixResult destroyContext(OptixDeviceContext context)
{
	return optixDeviceContextDestroy(context);
}

OPTIXDENOISER_API cudaError_t createCudaStream(cudaStream_t* stream)
{
	return cudaStreamCreate(stream);
}

OPTIXDENOISER_API cudaError_t destroyCudaStream(cudaStream_t stream)
{
	return cudaStreamDestroy(stream);
}

OPTIXDENOISER_API OptixResult createDenoiser(OptixDeviceContext context, const OptixDenoiserOptions* options, OptixDenoiser* denoiser)
{
	return optixDenoiserCreate(context, options, denoiser);
}

OPTIXDENOISER_API OptixResult destroyDenoiser(OptixDenoiser denoiser)
{
	return optixDenoiserDestroy(denoiser);
}

OPTIXDENOISER_API OptixResult setDenoiserModel(OptixDenoiser denoiser, OptixDenoiserModelKind kind, void* data, size_t sizeInBytes)
{
	return optixDenoiserSetModel(denoiser, kind, data, sizeInBytes);
}

OPTIXDENOISER_API OptixResult computeIntensity(OptixDenoiser denoiser, CUstream stream, const OptixImage2D* inputImage, CUdeviceptr outputIntensity, 
	CUdeviceptr scratch, size_t scratchSizeInBytes)
{
	return optixDenoiserComputeIntensity(denoiser, stream, inputImage, outputIntensity, scratch, scratchSizeInBytes);
}

OPTIXDENOISER_API OptixResult invokeDenoiser(
	OptixDenoiser denoiser, CUstream stream, const OptixDenoiserParams* params, CUdeviceptr denoiserState, size_t denoiserStateSizeInBytes,
	const OptixImage2D* inputLayers, unsigned int numInputLayers, unsigned int inputOffsetX, unsigned int inputOffsetY, const OptixImage2D* outputLayer, 
	CUdeviceptr scratch, size_t scratchSizeInBytes)
{
	return optixDenoiserInvoke(denoiser, stream, params, denoiserState, denoiserStateSizeInBytes, inputLayers, numInputLayers, inputOffsetX, inputOffsetY,
		outputLayer, scratch, scratchSizeInBytes);
}

OPTIXDENOISER_API OptixResult computeMemoryResources(OptixDenoiser denoiser, unsigned int outputWidth, unsigned int outputHeight, 
	OptixDenoiserSizes* returnSizes)
{
	return optixDenoiserComputeMemoryResources(denoiser, outputWidth, outputHeight, returnSizes);
}