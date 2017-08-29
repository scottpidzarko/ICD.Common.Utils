﻿using ICD.Common.Properties;

namespace ICD.Common.Utils.EventArguments
{
	[PublicAPI]
	public sealed class UShortEventArgs : GenericEventArgs<ushort>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="data"></param>
		public UShortEventArgs(ushort data) : base(data)
		{
		}
	}
}
