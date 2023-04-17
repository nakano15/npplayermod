using Terraria;
using System;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace npplayersmod
{
    public class Npplayer : ModPlayer
    {
        public override bool IsCloneable => false;
        private static PlayerDeathReason deathReason = new PlayerDeathReason();

        public bool IsNPPlayer = false;
        public byte SpawnerID = 0;
        public bool AIWait = false;
        public byte AITime = 0;

        public Vector2 AimPosition = Vector2.Zero;

        public int TargetID = -1;

        public bool IsLocalNpp { get { return IsNPPlayer && SpawnerID == npplayersmod.MyPlayerBackup; } }

        public static bool IsPlayerLocalNpp(Player player)
        {
            return player.GetModPlayer<Npplayer>().IsLocalNpp;
        }

        public override void OnEnterWorld(Player player)
        {
            AimPosition = player.Center;
            if (player.whoAmI == Main.myPlayer)
            {
                npplayersmod.MyPlayerBackup = player.whoAmI;
                foreach(Terraria.IO.PlayerFileData p in Main.PlayerList)
                {
                    if (p.Player != player)
                    {
                        Player np = SpawnNPPlayer(p.Player);
                    }
                }
                /*for (byte i = 0; i < 10; i++)
                {
                    Player p = SpawnNPPlayer();
                    if(p != null)
                    {
                        //p.inventory[0].SetDefaults(Terraria.ID.ItemID.SolarEruption);
                        p.inventory[1].SetDefaults(Terraria.ID.ItemID.WoodenBow);
                        //p.inventory[2].SetDefaults(Terraria.ID.ItemID.StardustCellStaff);
                        //p.inventory[3].SetDefaults(Terraria.ID.ItemID.RazorbladeTyphoon);
                        p.inventory[54].SetDefaults(Terraria.ID.ItemID.HolyArrow);
                        p.inventory[54].stack = 250;
                        p.selectedItem = 1;
                        p.armor[0].SetDefaults(Terraria.ID.ItemID.CrimsonHelmet);
                        p.armor[1].SetDefaults(Terraria.ID.ItemID.CrimsonScalemail);
                        p.armor[2].SetDefaults(Terraria.ID.ItemID.CrimsonGreaves);
                    }
                    p.statManaMax = p.statMana = 200;
                    p.statLifeMax = p.statLife = 500;
                }*/
            }
        }

        public static Player SpawnNPPlayer(Player player = null)
        {
            for(int i = 254; i >= 0; i--)
            {
                if (!Main.player[i].active)
                {
                    if (player == null)
                    {
                        player = new Player();
                        Main.player[i] = player;
                        player.name = "Npplayer #" + i;
                    }
                    Main.player[i] = player;
                    player.whoAmI = i;
                    player.direction = Main.rand.Next(2) == 0 ? -1 : 1;
                    player.Spawn(PlayerSpawnContext.SpawningIntoWorld);
                    Npplayer npp = player.GetModPlayer<Npplayer>();
                    npp.IsNPPlayer = true;
                    npp.SpawnerID = (byte)npplayersmod.MyPlayerBackup;
                    PlayerLoader.OnEnterWorld(i);
                    return player;
                }
            }
            return null;
        }

        public override void OnRespawn(Player player)
        {
            AimPosition = player.Center;
        }

        public override void PostUpdateRunSpeeds()
        {
            if (IsLocalNpp)
            {
                Player.Update_NPCCollision();
            }
        }

        public override void Unload()
        {
            deathReason = null;
        }

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter)
        {
            Npplayer.deathReason = damageSource;
            if(IsLocalNpp)
                Main.myPlayer = Player.whoAmI;
            return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter);
        }

        public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter)
        {
            Main.myPlayer = npplayersmod.MyPlayerBackup;
            if (IsLocalNpp && Player.statLife - damage <= 0)
                Player.KillMe(Npplayer.deathReason, damage, hitDirection, pvp);
        }

        public override void PreUpdate()
        {
            Main.myPlayer = npplayersmod.MyPlayerBackup;
            if (IsLocalNpp)
            {
                if(Main.dontStarveWorld)
                {
                    Terraria.GameContent.DontStarveDarknessDamageDealer.Update(Player);
                }
                if (Player.ghost)
                {
                    Player.Ghost();
                }
                if (Player.dead)
                {
                    Player.UpdateDead();
                    if (Player.respawnTimer <= 0)
                        Player.Spawn(PlayerSpawnContext.ReviveFromDeath);
                }
            }
        }

        public static void ChangeAimDirectionBasedOnNpplayer(Player player)
        {
            Npplayer npp = player.GetModPlayer<Npplayer>();
            npplayersmod.ChangeMousePositionTemporarily(npp.AimPosition.X, npp.AimPosition.Y);
        }

        public override bool CanConsumeAmmo(Item weapon, Item ammo)
        {
            return !IsNPPlayer;
        }

        public override bool PreItemCheck()
        {
            if (IsLocalNpp)
            {
                Main.myPlayer = Player.whoAmI;
                npplayersmod.ChangeMousePositionTemporarily(AimPosition.X, AimPosition.Y);
            }
            return base.PreItemCheck();
        }

        public override void PostItemCheck()
        {
            Main.myPlayer = npplayersmod.MyPlayerBackup;
            npplayersmod.RevertMousePosition();
            base.PostItemCheck();
        }

        public override void PreUpdateMovement()
        {
            if (IsLocalNpp)
                UpdateNPP();
        }

        public override void SetControls()
        {

        }

        private void ResetControls()
        {
            Player.releaseUseItem = !Player.controlUseItem;
            Player.releaseDown = !Player.controlDown;
            Player.releaseHook = !Player.controlHook;
            Player.releaseJump = !Player.controlJump;
            Player.releaseLeft = !Player.controlLeft;
            Player.releaseMount = !Player.controlMount;
            Player.releaseQuickHeal = !Player.controlQuickHeal;
            Player.releaseQuickMana = !Player.controlQuickMana;
            Player.releaseRight = !Player.controlRight;
            Player.releaseSmart = !Player.controlSmart;
            Player.releaseThrow = !Player.controlThrow;
            Player.releaseUp = !Player.controlUp;
            Player.releaseUseTile = !Player.controlUseTile;
            Player.controlDown = Player.controlHook = Player.controlJump = Player.controlLeft = Player.controlMount = Player.controlQuickHeal =
                Player.controlQuickMana = Player.controlRight = Player.controlSmart = Player.controlThrow = Player.controlTorch = Player.controlUp =
                Player.controlUseTile = Player.controlUseItem = false;
        }

        private void UpdateNPP()
        {
            ResetControls();
            UpdateAI();
        }

        private void UpdateAI()
        {
            LookForTargets();
            UpdateMousePositionByMovement();
            ResetCommands();
            if (TargetID == -1){
                //UpdateIdle();
                AvoidNpplayerStacking();
                ReturnToSpawnPointArea();
            }
            else
                UpdateCombat();
        }

        private void AvoidNpplayerStacking()
        {
            for (int i = Player.whoAmI + 1; i < 255; i++)
            {
                if (Main.player[i].active && Main.player[i].Hitbox.Intersects(Player.Hitbox) && Main.player[i].velocity.X == 0)
                {
                    if (Player.direction == 1)
                        Player.controlRight = true;
                    else
                        Player.controlLeft = true;
                    break;
                }
            }
        }

        private void ReturnToSpawnPointArea(){
            if(MathF.Abs(Player.Center.X - Main.spawnTileX * 16) >= 128)
            {
                if(Player.Center.X < Main.spawnTileX * 16)
                    Player.controlRight = true;
                else
                    Player.controlLeft = true;
            }
        }

        private void ResetCommands(){
            Player.controlLeft = Player.controlRight = Player.controlUp = Player.controlDown = 
            Player.controlJump = Player.controlUseItem = false;
        }

        private void UpdateMousePositionByMovement()
        {
            if (Math.Abs(AimPosition.X - Player.Center.X) > 2000)
                AimPosition = Player.Center;
            AimPosition += Player.velocity;
        }

        private void AimAt(Vector2 Position)
        {
            Vector2 AimDirection = Position - AimPosition;
            AimDirection.Normalize();
            AimPosition += AimDirection * 4;
        }

        private void LookForTargets()
        {
            TargetID = -1;
            float NearestDistance = 600;
            Vector2 MyPosition = Player.Center;
            for(byte i = 0; i < 200; i++)
            {
                float Distance;
                if(Main.npc[i].active && !Main.npc[i].friendly && !Main.npc[i].townNPC && Main.npc[i].CanBeChasedBy(null) && 
                (Distance = (Main.npc[i].position - MyPosition).Length()) < NearestDistance)
                {
                    TargetID = i;
                    NearestDistance = Distance;
                }
            }
        }

        private void UpdateIdle()
        {
            if (AITime == 0)
            {
                AIWait = !AIWait;
                AITime = (byte)(Main.rand.Next(10, 21));
                if (!AIWait && Main.rand.Next(2) == 0)
                    Player.direction *= -1;
            }
            if (!AIWait)
            {
                if (Player.direction < 0)
                    Player.controlLeft = true;
                else
                    Player.controlRight = true;
            }
            AimAt(Player.Center + new Vector2(16 * Player.direction, 0));
            AITime--;
        }

        private bool IsDamageType(DamageClass ToCompare, DamageClass ToLookFor)
        {
            return ToCompare == ToLookFor || ToCompare.CountsAsClass(ToLookFor);
        }

        private void UpdateCombat()
        {
            Vector2 TargetCenter, TargetPosition, TargetVelocity;
            int TargetDimX, TargetDimY;
            //Npc
            {
                TargetPosition = Main.npc[TargetID].position;
                TargetCenter = Main.npc[TargetID].Center;
                TargetVelocity = Main.npc[TargetID].velocity;
                TargetDimX = Main.npc[TargetID].width;
                TargetDimY = Main.npc[TargetID].height;
            }
            Vector2 AimDirection = TargetCenter + TargetVelocity + (TargetCenter - Player.Center) * (1f / Player.inventory[Player.selectedItem].shootSpeed * 2);
            AimAt(TargetCenter + TargetVelocity);
            bool MouseOnTarget = Math.Abs(TargetCenter.X + TargetVelocity.X - AimPosition.X) < TargetDimX * 0.5f && Math.Abs(TargetCenter.Y + TargetVelocity.Y - AimPosition.Y) < TargetDimY * 0.5f;
            Vector2 MyPosition = Player.Bottom;
            float DistanceX = Math.Abs(MyPosition.X - TargetCenter.X) - (TargetDimX * 0.5f + Player.width * 0.5f);
            float DistanceY = Math.Abs(MyPosition.Y - TargetCenter.Y) - (TargetDimY * 0.5f + Player.height * 0.5f);
            if(Player.itemAnimation <= 0)
            {
                byte BestItem = 0;
                int BestDamage = 0;
                for(byte i = 0; i < 10; i++){
                    Item thisitem = Player.inventory[i];
                    if(thisitem.type > 0 && thisitem.damage > 0)
                    {
                        int DamageValue = 0;
                        if(thisitem.DamageType.CountsAsClass(DamageClass.Summon) && Player.numMinions < Player.maxMinions && DistanceX >= 100)
                        {
                            DamageValue = Player.GetWeaponDamage(thisitem, false) * 10;
                        }
                        if(thisitem.DamageType.CountsAsClass(DamageClass.Magic) && Player.statMana >= Player.manaCost * thisitem.mana)
                        {
                            DamageValue = Player.GetWeaponDamage(thisitem, false);
                        }
                        if(thisitem.DamageType.CountsAsClass(DamageClass.Ranged))
                        {
                            DamageValue = Player.GetWeaponDamage(thisitem, false);
                        }
                        if(thisitem.DamageType.CountsAsClass(DamageClass.Melee))
                        {
                            if(DistanceX < 100)
                            {
                                DamageValue = Player.GetWeaponDamage(thisitem, false);
                            }
                        }
                        if(DamageValue > BestDamage)
                        {
                            BestItem = i;
                            BestDamage = DamageValue;
                        }
                    }
                }
                Player.selectedItem = BestItem;
            }
            Item item = Player.inventory[Player.selectedItem];
            bool IsMonsterAbove = MyPosition.Y > TargetCenter.Y;
            bool Attack = false, Jump = false, Approach = false, Retreat = false;
            if(IsDamageType(item.DamageType, DamageClass.Summon))
            {
                Attack = true;
                if (DistanceX < 80)
                {
                    if (MyPosition.X < TargetCenter.X)
                        Player.controlLeft = true;
                    else
                        Player.controlRight = true;
                }
            }
            if (IsDamageType(item.DamageType, DamageClass.Melee))
            {
                if (IsMonsterAbove && DistanceY > item.height * item.scale * 0.9f)
                {
                    Jump = true;
                }
                if (DistanceX < item.width * item.scale * 0.9f)
                {
                    Attack = true;
                    if (MyPosition.X < TargetCenter.X)
                        Player.controlLeft = true;
                    else
                        Player.controlRight = true;
                }
                else if (DistanceX > item.width * item.scale)
                {
                    if (MyPosition.X > TargetCenter.X)
                        Player.controlLeft = true;
                    else
                        Player.controlRight = true;
                }
                else
                {
                    Attack = true;
                    if (MyPosition.X > TargetCenter.X)
                        Player.controlLeft = true;
                    else
                        Player.controlRight = true;
                    Player.ChangeDir(Player.controlLeft ? -1 : 1);
                }
            }
            else if (IsDamageType(item.DamageType, DamageClass.Ranged) || IsDamageType(item.DamageType, DamageClass.Magic))
            {
                if (DistanceX < 180)
                {
                    Retreat = true;
                    if(Collision.CanHitLine(Player.position, Player.width, Player.height, TargetPosition, TargetDimX, TargetDimY)) Attack = true;
                }
                else if (DistanceX > 350)
                {
                    Approach = true;
                }
                else if(Collision.CanHitLine(Player.position, Player.width, Player.height, TargetPosition, TargetDimX, TargetDimY))
                {
                    Attack = true;
                }
            }
            if (Approach)
            {
                if (MyPosition.X > TargetCenter.X)
                    Player.controlLeft = true;
                else
                    Player.controlRight = true;
            }
            if (Retreat)
            {
                if (MyPosition.X < TargetCenter.X)
                    Player.controlLeft = true;
                else
                    Player.controlRight = true;
            }
            if (Attack && MouseOnTarget)
            {
                if ((Player.itemAnimation <= 0 && Main.rand.NextDouble() < 0.3f))
                {
                    Player.controlUseItem = true;
                }
            }
            if (Jump)
            {
                if (Player.jump > 0 || (Player.jump == 0 && Player.velocity.Y == 0))
                {
                    Player.controlJump = true;
                }
            }
        }
    }
}
