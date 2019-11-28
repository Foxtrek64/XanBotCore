using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace XanBotCore.Utility.DiscordObjects {

	/// <summary>
	/// Represents an object that can be turned into a <see cref="DiscordEmbed"/>.
	/// </summary>
	public interface IEmbeddable {

		/// <summary>
		/// Represent this object in a <see cref="DiscordEmbed"/>
		/// </summary>
		/// <returns></returns>
		DiscordEmbed ToEmbed();

	}
}
