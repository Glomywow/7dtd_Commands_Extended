using System;
using System.Collections.Generic;
using System.Reflection;

namespace AllocsFixes.CustomCommands
{
	public class TeleportPlayer : ConsoleCmdAbstract
	{
		public override string GetDescription ()
		{
			return "teleport a player to a given location";
		}

		public override string GetHelp () {
			return "Usage:\n" +
				   "  1. teleportplayer <steam id / player name / entity id> <x> <y> <z>\n" +
				   "  2. teleportplayer <steam id / player name / entity id> <target steam id / player name / entity id>\n" +
				   "1. Teleports the player given by his SteamID, player name or entity id (as given by e.g. \"lpi\")\n" +
				   "   to the specified location\n" +
				   "2. As 1, but destination given by another player which has to be online";
		}

		public override string[] GetCommands ()
		{
			return new string[] { "teleportplayer", "tele" };
		}

		public override void Execute (List<string> _params, CommandSenderInfo _senderInfo)
		{
			try {
				if (_params.Count != 4 && _params.Count != 2) {
					SdtdConsole.Instance.Output ("Usage: teleportplayer <entityid|playername|steamid> <x> <y> <z>");
					SdtdConsole.Instance.Output ("   or: teleportplayer <entityid|playername|steamid> <target entityid|playername|steamid>");
				} else {
					ClientInfo ci1 = ConsoleHelper.ParseParamIdOrName (_params [0]);//ci1.SendPackage (pkg);
					if (ci1 == null) {
						SdtdConsole.Instance.Output ("Playername or entity/steamid id not found.");
						return;
					}
					EntityPlayer ep1 = GameManager.Instance.World.Players.dict [ci1.entityId];

					if (_params.Count == 4) {
						int x = int.MinValue;
						int y = int.MinValue;
						int z = int.MinValue;

						int.TryParse (_params [1], out x);
						int.TryParse (_params [2], out y);
						int.TryParse (_params [3], out z);

						if (x == int.MinValue || y == int.MinValue || z == int.MinValue) {
							SdtdConsole.Instance.Output ("At least one of the given coordinates is not a valid integer");
							return;
						}

						ep1.position.x = x;
						ep1.position.y = y;
						ep1.position.z = z;
					} else {
						ClientInfo ci2 = ConsoleHelper.ParseParamIdOrName (_params [1]);
						if (ci2 == null) {
							SdtdConsole.Instance.Output ("Target playername or entity/steamid id not found.");
							return;
						}
						EntityPlayer ep2 = GameManager.Instance.World.Players.dict [ci2.entityId];

						ep1.position = ep2.GetPosition();
						ep1.position.y += 1;
						ep1.position.z += 1;
					
					}

					NetPackageEntityTeleport pkg = new NetPackageEntityTeleport (ep1);
					//NetPackagePlayerInventory pkh = new NetPackagePlayerInventory(ep2
					ci1.SendPackage (pkg);
				}
			} catch (Exception e) {
				Log.Out ("Error in TeleportPlayer.Run: " + e);
			}
		}

	}
}
