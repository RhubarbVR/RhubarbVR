﻿using System;
using System.Collections.Generic;
using System.Text;

using DataModel.Enums;

namespace RhubarbCloudClient.Model
{
	public sealed class UserDM
	{
		public sealed class MSG
		{
			public Guid Id { get; set; }
			public Guid DMId { get; set; }

			public Guid FromUser { get; set; }

			public bool IsDeleted { get; set; }

			public bool IsEdited { get; set; }

			public MessageType Type { get; set; }

			public DateTimeOffset CreatedDate { get; set; }

			public DateTimeOffset LastUpdated { get; set; }

			public string MessageData { get; set; }
		}

		public Guid Id { get; set; }
		public Guid LastMsgId { get; set; }

		public string DMName { get; set; }
		public Guid Thumbnail { get; set; }
		public bool IsGorupDM { get; set; }
		public Guid[] Users { get; set; }
		public MSG[] Msgs { get; set; }

	}
}
