// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Ecom
{
	/// <summary>
	/// Input parameters for the <see cref="EcomInterface.CopyTransactionById" /> function.
	/// </summary>
	public class CopyTransactionByIdOptions
	{
		/// <summary>
		/// The Epic Online Services Account ID of the local user who is associated with the transaction
		/// </summary>
		public EpicAccountId LocalUserId { get; set; }

		/// <summary>
		/// The ID of the transaction to get
		/// </summary>
		public string TransactionId { get; set; }
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct CopyTransactionByIdOptionsInternal : ISettable, System.IDisposable
	{
		private int m_ApiVersion;
		private System.IntPtr m_LocalUserId;
		private System.IntPtr m_TransactionId;

		public EpicAccountId LocalUserId
		{
			set
			{
				Helper.TryMarshalSet(ref m_LocalUserId, value);
			}
		}

		public string TransactionId
		{
			set
			{
				Helper.TryMarshalSet(ref m_TransactionId, value);
			}
		}

		public void Set(CopyTransactionByIdOptions other)
		{
			if (other != null)
			{
				m_ApiVersion = EcomInterface.CopytransactionbyidApiLatest;
				LocalUserId = other.LocalUserId;
				TransactionId = other.TransactionId;
			}
		}

		public void Set(object other)
		{
			Set(other as CopyTransactionByIdOptions);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_TransactionId);
		}
	}
}