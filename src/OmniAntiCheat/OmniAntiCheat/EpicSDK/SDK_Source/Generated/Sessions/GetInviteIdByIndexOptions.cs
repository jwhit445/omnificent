// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Sessions
{
	/// <summary>
	/// Input parameters for the <see cref="SessionsInterface.GetInviteIdByIndex" /> function.
	/// </summary>
	public class GetInviteIdByIndexOptions
	{
		/// <summary>
		/// The Product User ID of the local user who has an invitation in the cache
		/// </summary>
		public ProductUserId LocalUserId { get; set; }

		/// <summary>
		/// Index of the invite ID to retrieve
		/// </summary>
		public uint Index { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct GetInviteIdByIndexOptionsInternal : ISettable, System.IDisposable
	{
		private int m_ApiVersion;
		private System.IntPtr m_LocalUserId;
		private uint m_Index;

		public ProductUserId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public uint Index
		{
			set
			{
				m_Index = value;
			}
		}

		public void Set(GetInviteIdByIndexOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = SessionsInterface.GetinviteidbyindexApiLatest;
				LocalUserId = other.LocalUserId;
				Index = other.Index;
			}
		}

		public void Set(object other)
		{
			Set(other as GetInviteIdByIndexOptions);
		}

		public void Dispose()
		{
		}
	}
}