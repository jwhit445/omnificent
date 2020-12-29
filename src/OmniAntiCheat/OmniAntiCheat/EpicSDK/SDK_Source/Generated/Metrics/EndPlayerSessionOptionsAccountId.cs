// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Metrics
{
	public class EndPlayerSessionOptionsAccountId : ISettable
	{
		private MetricsAccountIdType m_AccountIdType;
		private EpicAccountId m_Epic;
		private string m_External;

		/// <summary>
		/// The Account ID type that is set in the union.
		/// </summary>
		public MetricsAccountIdType AccountIdType
		{
			get
			{
				return m_AccountIdType;
			}

			private set
			{
				m_AccountIdType = value;
			}
		}

		/// <summary>
		/// An Epic Online Services Account ID. Set this field when AccountIdType is set to <see cref="MetricsAccountIdType.Epic" />.
		/// </summary>
		public EpicAccountId Epic
		{
			get
			{
				EpicAccountId value;
				Helper.TryMarshalGet(m_Epic, out value, m_AccountIdType, MetricsAccountIdType.Epic);
				return value;
			}

			set
			{
				Helper.TryMarshalSet(ref m_Epic, value, ref m_AccountIdType, MetricsAccountIdType.Epic);
			}
		}

		/// <summary>
		/// An Account ID for another service. Set this field when AccountIdType is set to <see cref="MetricsAccountIdType.External" />.
		/// </summary>
		public string External
		{
			get
			{
				string value;
				Helper.TryMarshalGet(m_External, out value, m_AccountIdType, MetricsAccountIdType.External);
				return value;
			}

			set
			{
				Helper.TryMarshalSet(ref m_External, value, ref m_AccountIdType, MetricsAccountIdType.External);
			}
		}

		public static implicit operator EndPlayerSessionOptionsAccountId(EpicAccountId value)
		{
			return new EndPlayerSessionOptionsAccountId() { Epic = value };
		}

		public static implicit operator EndPlayerSessionOptionsAccountId(string value)
		{
			return new EndPlayerSessionOptionsAccountId() { External = value };
		}

		internal void Set(EndPlayerSessionOptionsAccountIdInternal? other)
		{
			if (other != null)
			{
				Epic = other.Value.Epic;
				External = other.Value.External;
			}
		}

		public void Set(object other)
		{
			Set(other as EndPlayerSessionOptionsAccountIdInternal?);
		}
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Pack = 4)]
	internal struct EndPlayerSessionOptionsAccountIdInternal : ISettable, System.IDisposable
	{
		[System.Runtime.InteropServices.FieldOffset(0)]
		private MetricsAccountIdType m_AccountIdType;
		[System.Runtime.InteropServices.FieldOffset(4)]
		private System.IntPtr m_Epic;
		[System.Runtime.InteropServices.FieldOffset(4)]
		private System.IntPtr m_External;

		public EpicAccountId Epic
		{
			get
			{
				EpicAccountId value;
				Helper.TryMarshalGet(m_Epic, out value, m_AccountIdType, MetricsAccountIdType.Epic);
				return value;
			}

			set
			{
				Helper.TryMarshalSet(ref m_Epic, value, ref m_AccountIdType, MetricsAccountIdType.Epic, this);
			}
		}

		public string External
		{
			get
			{
				string value;
				Helper.TryMarshalGet(m_External, out value, m_AccountIdType, MetricsAccountIdType.External);
				return value;
			}

			set
			{
				Helper.TryMarshalSet(ref m_External, value, ref m_AccountIdType, MetricsAccountIdType.External, this);
			}
		}

		public void Set(EndPlayerSessionOptionsAccountId other)
		{
			if (other != null)
			{
				Epic = other.Epic;
				External = other.External;
			}
		}

		public void Set(object other)
		{
			Set(other as EndPlayerSessionOptionsAccountId);
		}

		public void Dispose()
		{
			Helper.TryMarshalDispose(ref m_External, m_AccountIdType, MetricsAccountIdType.External);
		}
	}
}