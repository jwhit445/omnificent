// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Auth
{
	/// <summary>
	/// Flags used to describe how the account linking operation is to be performed.
	/// <seealso cref="AuthInterface.LinkAccount" />
	/// </summary>
	[System.Flags]
	public enum LinkAccountFlags : ulong
	{
		/// <summary>
		/// Default flag used for a standard account linking operation.
		/// 
		/// This flag is set when using a continuance token received from a previous call to the <see cref="AuthInterface.Login" /> API,
		/// when the local user has not yet been successfully logged in to an Epic Account yet.
		/// </summary>
		NoFlags = 0x0,
		/// <summary>
		/// Specified when the <see cref="ContinuanceToken" /> describes a Nintendo NSA ID account type.
		/// 
		/// This flag is used only with, and must be set, when the continuance token was received from a previous call
		/// to the <see cref="AuthInterface.Login" /> API using the <see cref="Connect.ExternalCredentialType" />::<see cref="Connect.ExternalCredentialType.NintendoNsaIdToken" /> login type.
		/// </summary>
		NintendoNsaId = 0x1
	}
}