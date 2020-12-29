// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Platform
{
	/// <summary>
	/// Function prototype type definition for functions that reallocate memory.
	/// 
	/// Functions passed to <see cref="PlatformInterface.Initialize" /> to serve as memory reallocators should return a pointer to the reallocated memory.
	/// The returned pointer should have at least SizeInBytes available capacity and the memory address should be a multiple of alignment.
	/// The SDK will always call the provided function with an Alignment that is a power of 2.
	/// Reallocation failures should return a null pointer.
	/// </summary>
	public delegate System.IntPtr ReallocateMemoryFunc(System.IntPtr pointer, System.UIntPtr sizeInBytes, System.UIntPtr alignment);

	internal delegate System.IntPtr ReallocateMemoryFuncInternal(System.IntPtr pointer, System.UIntPtr sizeInBytes, System.UIntPtr alignment);
}