using System;
using System.Collections.Generic;
using System.Text;

namespace SharedModels.UserInfo
{
	public enum Status
	{
		Unknown,
		Online,
		Idle,
		DoNotDisturb,
		Streaming,
		Invisible,
		Offline,
	}
	public class UserStatus
	{
		public Status Status { get; set; }

		public Guid UserID { get; set; }

		public string CustomStatusMsg { get; set; }

		public Guid CurrentSession { get; set; }

		public virtual Guid[] ActiveSessions { get; set; }

		public string ClientVersion { get; set; }
	}
}
