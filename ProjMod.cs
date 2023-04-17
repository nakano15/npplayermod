using Terraria;
using Terraria.ModLoader;

namespace npplayersmod
{
    public class ProjMod : GlobalProjectile
    {
        private static int MainPlayerBackup = -1;

        public override bool PreAI(Projectile projectile)
        {
            if(MainPlayerBackup != -1)
            {
                Main.myPlayer = MainPlayerBackup;
            }
            if(!projectile.npcProj && Npplayer.IsPlayerLocalNpp(Main.player[projectile.owner]))
            {
                MainPlayerBackup = Main.myPlayer;
                Main.myPlayer = projectile.owner;
                Npplayer.ChangeAimDirectionBasedOnNpplayer(Main.player[projectile.owner]);
            }
            return base.PreAI(projectile);
        }

        public override void PostAI(Projectile projectile)
        {
            if(!projectile.npcProj && Npplayer.IsPlayerLocalNpp(Main.player[projectile.owner]))
            {
                try{
                    projectile.Damage();
                }catch{}
                Main.myPlayer = MainPlayerBackup;
                MainPlayerBackup = -1;
                npplayersmod.RevertMousePosition();
            }
        }
    }
}
