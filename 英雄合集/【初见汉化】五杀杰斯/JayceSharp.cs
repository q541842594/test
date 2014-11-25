﻿using System;

using LeagueSharp;
using LeagueSharp.Common;
/*
 * ToDo:
 * Q doesnt shoot much
 * Full combo burst
 * Useles gate <-- fixed
 * 
 * Add Fulldmg combo starting from hamer
 * 
 * kOCK ANY ENEMY UNDER TOWER
 * */
using SharpDX;
using Color = System.Drawing.Color;


namespace JayceSharp
{
    internal class JayceSharp
    {
        public const string CharName = "Jayce";

        public static Menu Config;

        public JayceSharp()
        {
            /* CallBAcks */
            CustomEvents.Game.OnGameLoad += onLoad;

        }

        private static void onLoad(EventArgs args)
        {

            Game.PrintChat("Jayce - Sharp by DeTuKs DOnate if you love my assams :)");
            Jayce.setSkillShots();
            try
            {

                Config = new Menu("鏉版柉", "Jayce", true);
                //Orbwalker
                Config.AddSubMenu(new Menu("璧扮爫", "Orbwalker"));
                Jayce.orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));
                //TS
                Menu targetSelectorMenu = new Menu("鐩爣閫夋嫨", "Target Selector");
                SimpleTs.AddToMenu(targetSelectorMenu);
                Config.AddSubMenu(targetSelectorMenu);
                //Combo
                Config.AddSubMenu(new Menu("杩炴嫑璁剧疆", "combo"));
                Config.SubMenu("combo").AddItem(new MenuItem("comboItems", "浣跨敤閫夐」")).SetValue(true);
                Config.SubMenu("combo").AddItem(new MenuItem("fullDMG", "杩炴嫑")).SetValue(new KeyBind('A', KeyBindType.Press));
                Config.SubMenu("combo").AddItem(new MenuItem("injTarget", "棰勫垽EQ")).SetValue(new KeyBind('G', KeyBindType.Press));


                //Extra
                Config.AddSubMenu(new Menu("鎶€鑳借寖鍥撮€夐」", "drawing"));
                Config.SubMenu("drawing").AddItem(new MenuItem("drawStuff", "Draw on/off")).SetValue(true);
               
                //Extra
                Config.AddSubMenu(new Menu("瀹氬悜EQ", "extra"));
                Config.SubMenu("extra").AddItem(new MenuItem("shoot", "Shoot manual Q")).SetValue(new KeyBind('T', KeyBindType.Press));

                //Debug
                Config.AddSubMenu(new Menu("Debug", "debug"));
                Config.SubMenu("debug").AddItem(new MenuItem("db_targ", "Debug Target")).SetValue(new KeyBind('N', KeyBindType.Press));
                //Donate
                Config.AddSubMenu(new Menu("Donate", "Donate"));
                Config.SubMenu("debug").AddItem(new MenuItem("domateMe", "PayPal:")).SetValue(true);
                Config.SubMenu("debug").AddItem(new MenuItem("domateMe2", "dtk600@gmail.com")).SetValue(true);
                Config.SubMenu("debug").AddItem(new MenuItem("domateMe3", "Tnx ^.^")).SetValue(true);
Config.AddSubMenu(new Menu("鍒濊姹夊寲", "by chujian"));

Config.SubMenu("by chujian").AddItem(new MenuItem("qunhao", "姹夊寲缇わ細386289593"));
Config.SubMenu("by chujian").AddItem(new MenuItem("qunhao2", "濞冨▋缇わ細13497795"));

                Config.AddToMainMenu();
                Drawing.OnDraw += onDraw;
                Game.OnGameUpdate += OnGameUpdate;

                GameObject.OnCreate += OnCreateObject;
                GameObject.OnDelete += OnDeleteObject;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;

            }
            catch
            {
                Game.PrintChat("Oops. Something went wrong with Jayce - Sharp");
            }

        }

        private static void OnGameUpdate(EventArgs args)
        {
            Jayce.checkForm();
            Jayce.processCDs();
            if (Config.Item("shoot").GetValue<KeyBind>().Active)//fullDMG
            {
                Jayce.shootQE(Game.CursorPos);
            }

            if(!Jayce.E1.IsReady())
                Jayce.castQon = new Vector3(0,0,0);
            else if (Jayce.castQon.X != 0)
                Jayce.shootQE(Jayce.castQon);

            if (Config.Item("fullDMG").GetValue<KeyBind>().Active)//fullDMG
            {
                Obj_AI_Hero target = SimpleTs.GetTarget(Jayce.getBestRange(), SimpleTs.DamageType.Physical);
                if (Jayce.lockedTarg == null)
                    Jayce.lockedTarg = target;
                Jayce.doFullDmg(Jayce.lockedTarg);
            }
            else
            {
                Jayce.lockedTarg = null;
            }

            if (Config.Item("injTarget").GetValue<KeyBind>().Active)//fullDMG
            {
                Obj_AI_Hero target = SimpleTs.GetTarget(Jayce.getBestRange(), SimpleTs.DamageType.Physical);
                if (Jayce.lockedTarg == null)
                    Jayce.lockedTarg = target;
                Jayce.doJayceInj(Jayce.lockedTarg);
            }
            else
            {
                Jayce.lockedTarg = null;
            }
           // Console.Clear();
           // Console.WriteLine(Jayce.isHammer +" "+Jayce.Qdata.SData.Name);

            if (Jayce.castEonQ != null && (Jayce.castEonQ.TimeSpellEnd-2) > Game.Time)
                Jayce.castEonQ = null;

            if (Jayce.orbwalker.ActiveMode.ToString() == "Combo")
            {
                
                Obj_AI_Hero target = SimpleTs.GetTarget(Jayce.getBestRange(), SimpleTs.DamageType.Physical);
                Jayce.doCombo(target);
            }

            if (Jayce.orbwalker.ActiveMode.ToString() == "Mixed")
            {

            }

            if (Jayce.orbwalker.ActiveMode.ToString() == "LaneClear")
            {

            }

          
        }

        private static void onDraw(EventArgs args)
        {
            if (!Config.Item("drawStuff").GetValue<bool>())
                return;
            //Obj_AI_Hero target = SimpleTs.GetTarget(1500, SimpleTs.DamageType.Physical);

          //  Utility.DrawCircle(Jayce.getBestPosToHammer(target), 70, Color.LawnGreen);
           // Utility.DrawCircle(Jayce.Player.Position, 400, Color.Violet);
            if (!Jayce.isHammer)
            {
                Utility.DrawCircle(Jayce.Player.Position, 1550, Color.Violet);
                Utility.DrawCircle(Jayce.Player.Position, 1100, Color.Red);
            }
            else
            {
                Utility.DrawCircle(Jayce.Player.Position, 600, Color.Red);
            }


            //Draw CD
            Jayce.drawCD();
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            Obj_SpellMissile missile = sender as Obj_SpellMissile;
            if (missile != null)
            {
                Obj_SpellMissile missle = missile;

                if (missle.SpellCaster.IsMe && missle.SData.Name == "JayceShockBlastMis")
                {
                   // Console.WriteLine("Created " +  missle.SData.Name );
                    Jayce.myCastedQ = missle;
                }
            }
        }

        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {
            if (Jayce.myCastedQ != null && Jayce.myCastedQ.NetworkId == sender.NetworkId)
            {
                Jayce.myCastedQ = null;
                Jayce.castEonQ = null;
            }
        }

        public static void OnProcessSpell(Obj_AI_Base obj, GameObjectProcessSpellCastEventArgs arg)
        {
            if (obj.IsMe)
            {

                if (arg.SData.Name == "jayceshockblast")
                {
                    Jayce.castEonQ = arg;
                }
                else if (arg.SData.Name == "jayceaccelerationgate")
                {
                    Jayce.castEonQ = null;
                   // Console.WriteLine("Cast dat E on: " + arg.SData.Name);
                }

                Jayce.getCDs(arg);
            }
        }

    }
}
