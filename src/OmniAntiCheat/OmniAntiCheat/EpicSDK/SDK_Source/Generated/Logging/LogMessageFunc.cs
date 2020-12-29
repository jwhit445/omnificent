// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Logging
{
	/// <summary>
	/// Function prototype definition for functions that receive log messages.
	/// <seealso cref="LogMessage" />
	/// </summary>
	/// <param name="message">A <see cref="LogMessage" /> containing the log category, log level, and message.</param>
	public delegate void LogMessageFunc(LogMessage message);

	internal delegate void LogMessageFuncInternal(System.IntPtr message);
}