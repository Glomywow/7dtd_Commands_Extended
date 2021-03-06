using AllocsFixes.PersistentData;
using System;
using System.Collections.Generic;
using System.IO;

namespace AllocsFixes.CustomCommands
{
	public class ShowInventory : ConsoleCmdAbstract
	{
		public override string GetDescription ()
		{
			return "list inventory of a given player (steam id, entity id or name)";
		}

		public override string GetHelp () {
			return "Usage:\n" +
				   "   showinventory <steam id / player name / entity id>\n" +
				   "Show the inventory of the player given by his SteamID, player name or\n" +
				   "entity id (as given by e.g. \"lpi\")." +
				   "Note: This only shows the player's inventory after it was first sent to\n" +
				   "the server which happens at least every 30 seconds.";
		}
	
		public override string[] GetCommands ()
		{
			return new string[] { "showinventory", "si" };
		}

		public override void Execute (List<string> _params, CommandSenderInfo _senderInfo)
		{
			try {
				if (_params.Count < 1) {
					SdtdConsole.Instance.Output ("Usage: showinventory <steamid|playername|entityid>");
					SdtdConsole.Instance.Output ("   or: showinventory <steamid|playername|entityid> <1>");
					return;
				}
				string steamid = PersistentContainer.Instance.Players.GetSteamID (_params [0], true);
				if (steamid == null) {
					SdtdConsole.Instance.Output ("Playername or entity/steamid id not found or no inventory saved (first saved after a player has been online for 30s).");
					return;
				}

				Player p = PersistentContainer.Instance.Players [steamid, false];
				PersistentData.Inventory inv = p.Inventory;
				if (_params.Count == 2) {
					saveInventory(p, inv, steamid);
					return;
				}
			
				SdtdConsole.Instance.Output ("Belt of player " + p.Name + ":");
				for (int i = 0; i < inv.belt.Count; i++) {
					if (inv.belt [i] != null) {
						SdtdConsole.Instance.Output (string.Format ("    Slot {0}: {1:000} * {2}", i, inv.belt [i].count, inv.belt [i].itemName));
					}
				}
				SdtdConsole.Instance.Output (string.Empty);
				SdtdConsole.Instance.Output ("Bagpack of player " + p.Name + ":");
				for (int i = 0; i < inv.bag.Count; i++) {
					if (inv.bag [i] != null)
						SdtdConsole.Instance.Output (string.Format ("    Slot {0}: {1:000} * {2}", i, inv.bag [i].count, inv.bag [i].itemName));
				}
				SdtdConsole.Instance.Output (string.Empty);
			} catch (Exception e) {
				Log.Out ("Error in ShowInventory.Run: " + e);
			}
		}
		public void saveInventory(Player p, PersistentData.Inventory inv, string steamID) {

			string headere = "";
			string equipement =  "<EQU>";
			Vector3i lastPos = p.LastPosition;
			string ipc = p.IP;
			//SdtdConsole.Instance.Output ("headere1: " + headere);
			try {
				
				if (p.IsOnline == true){
					int health = p.Entity.Health;
					int stamina = p.Entity.Stamina;
					int gassiness = p.Entity.Gassiness;
					EntityPlayer ply = p.Entity;
					int vc = ply.equipment.GetSlotCount();
				
					float wellness = ply.Stats.Wellness.Value;
					//ply.Stats.Debuff//ply.Stats.Buffs.//
					int sickness = ply.Sickness;
					bool sex = ply.IsMale;
					float height = p.Entity.height;
					for (int i = 0; i < vc; i++){
						ItemStack[] ib = ItemStack..list [p.Entity.equipment.GetSlotItem(i).type];
						try {
							string name = ib.GetItemName ();
							if (name != null) {
								//SdtdConsole.Instance.Output ("*    Slot # " + i +"/" + vc + " => " + name);
								equipement += "<ESLT><NAME>" + name + "</NAME><NUM>" + i + "</NUM></ESLT>";
							}
						} catch (Exception e) {
							//Log.Out ("Error in ShowInventory.saveInventory.getitem: " + e);
						}
					}
					//********************************************									
					headere = string.Concat (new object[] {
						"<HEALTH>" , health , "</HEALTH>",
						"<STAM>", stamina , "</STAM>",
						"<GAS>", gassiness , "</GAS>",
						"<WEL>", wellness , "</WEL>",
						"<MAL>", sex, "</MAL>",
						"<SIC>", sickness, "</SIC>",
						"<H8>", height, "</H8>",
						"<IP>", ipc, "</IP>",
						"<ZKILL>", p.Entity.KilledZombies, "</ZKILL>",
						"<PKILL>",p.Entity.KilledPlayers, "</PKILL>",
						"<DIED>", p.Entity.Died, "</DIED>"					
					});
				}
				//SdtdConsole.Instance.Output ("headere2: " + headere);
				try {

					long played = p.TotalPlayTime;
					string lastO = p.LastOnline.ToString("G");

					headere = string.Concat (new object[] {
						"<STATS>",
						"<NAME>", p.Name,"</NAME>",
						"<TTP>", (p.TotalPlayTime / 60).ToString(), "</TTP>",
						"<IOL>", p.IsOnline, "</IOL>",
						"<LCNX>", p.LastOnline.ToString(), "</LCNX>",
						headere,
						"<LASTP>", 
						"<X>", p.LastPosition.x, "</X>",
						"<Y>", p.LastPosition.y, "</Y>",
						"<Z>", p.LastPosition.z, "</Z>",
						"</LASTP>",
						"</STATS>"
						});
				} catch (Exception e) {
					Log.Out ("Error in ShowInventory.saveInventory.header: " + e);
				}
				equipement += "</EQU>";
				//SdtdConsole.Instance.Output ("headere3: " + headere);
				string beltSlots = "<BELT>";
				for (int i = 0; i < inv.belt.Count; i++) {
					if (inv.belt [i] != null)
						beltSlots += string.Format ("<SLOT><NUM>{0}</NUM><COUNT>{1:000}</COUNT><NAME>{2}</NAME></SLOT>", i, inv.belt [i].count, inv.belt [i].itemName);
				}
				beltSlots += "</BELT>";
				//SdtdConsole.Instance.Output ("beltSlots: " + beltSlots);
				string bagSlots = "<BAG>";
				for (int i = 0; i < inv.bag.Count; i++) {
					if (inv.bag [i] != null)
						bagSlots += string.Format ("<SLOT><NUM>{0}</NUM><COUNT>{1:000}</COUNT><NAME>{2}</NAME></SLOT>", i, inv.bag [i].count, inv.bag [i].itemName);
				}
				bagSlots += "</BAG>";
				//SdtdConsole.Instance.Output ("bagSlots: " + bagSlots);
				string inventory = string.Concat (new object[] {
					"<INVENTORY>",
					beltSlots, bagSlots, "</INVENTORY>"
				});
				//SdtdConsole.Instance.Output ("inventory: " + inventory);
				string kdata = string.Concat (new object[] {
					"<PLAYER>", headere, inventory, equipement, "</PLAYER>"
				});
				try {
					SdtdConsole.Instance.Output ("SteamID: " + steamID + ".xml saved..");
					StreamWriter sw = new StreamWriter("D:\\Inventories\\" + steamID + ".xml");

					sw.Write(kdata);
					sw.Close();
					kdata = null;
				} catch (Exception e){
					SdtdConsole.Instance.Output ("Error in ShowInventory.saveInventory.write: " + e + " " + steamID);
					Log.Out ("Error in ShowInventory.saveInventory.StreamWriter: " + e);
				}
			} catch (Exception e){
				SdtdConsole.Instance.Output ("Error in ShowInventory.saveInventory: " + e);
				Log.Out ("Error in ShowInventory.saveInventory: " + e);
			}
		}	
	}
}
