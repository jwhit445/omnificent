// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Ecom
{
	/// <summary>
	/// Input parameters for the <see cref="EcomInterface.GetItemImageInfoCount" /> function.
	/// </summary>
	public class GetItemImageInfoCountOptions
	{
		/// <summary>
		/// The Epic Online Services Account ID of the local user whose item image is being accessed
		/// </summary>
		public EpicAccountId LocalUserId { get; set; }

		/// <summary>
		/// The ID of the item to get the images for.
		/// </summary>
		public string ItemId { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct GetItemImageInfoCountOptionsInternal : ISettable, System.IDisposable
	{
		private int m_ApiVersion;
		private System.IntPtr m_LocalUserId;
		private System.IntPtr m_ItemId;

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string ItemId
		{
			set
			{
				Helper.TryMarshalSet(ref m_ItemId, value);
			}
		}

		public void Set(GetItemImageInfoCountOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = EcomInterface.GetitemimageinfocountApiLatest;
				LocalUserId = other.LocalUserId;
				ItemId = other.ItemId;
			}
		}

		public void Set(object other)
		{
			Set(other as GetItemImageInfoCountOptions);
		}

		public void Dispose()
		{
		}
	}
}