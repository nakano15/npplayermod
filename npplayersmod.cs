using Terraria.ModLoader;
using Terraria;

namespace npplayersmod
{
	public class npplayersmod : Mod
	{
		public static int MyPlayerBackup = 0;
		public static int LastMouseX = 0, LastMouseY = 0;
		public static bool ChangedMousePosition = false;

		public static void ChangeMousePositionTemporarily(float NewPositionX, float NewPositionY)
		{
			LastMouseX = Main.mouseX;
			LastMouseY = Main.mouseY;
			Main.mouseX = (int)(NewPositionX - Main.screenPosition.X);
			Main.mouseY = (int)(NewPositionY - Main.screenPosition.Y);
			ChangedMousePosition = true;
		}

		public static void RevertMousePosition()
        {
            if (ChangedMousePosition)
            {
				ChangedMousePosition = false;
				Main.mouseX = LastMouseX;
				Main.mouseY = LastMouseY;
			}
        }
	}
}