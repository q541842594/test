﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SkarnerHugeStinger
{
    class Program
    {
        public const string ChampionName = "Skarner";

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        //Menu
        public static Menu menu;

        private static Obj_AI_Hero Player;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            //Thanks to Esk0r
            Player = ObjectManager.Player;

            //check to see if correct champ
            if (Player.BaseSkinName != ChampionName) return;

            //intalize spell
            Q = new Spell(SpellSlot.Q, 350);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 1000);
            R = new Spell(SpellSlot.R, 350);


            E.SetSkillshot(0.50f, 60, 1200, false, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            //Create the menu
            menu = new Menu(ChampionName, ChampionName, true);

            //Orbwalker submenu
            menu.AddSubMenu(new Menu("璧扮爫", "Orbwalking"));

            //Target selector
            var targetSelectorMenu = new Menu("鐩爣閫夋嫨", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            menu.AddSubMenu(targetSelectorMenu);

            //Orbwalk
            Orbwalker = new Orbwalking.Orbwalker(menu.SubMenu("Orbwalking"));

            //Combo menu:
            menu.AddSubMenu(new Menu("杩炴嫑", "Combo"));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "浣跨敤 Q").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "浣跨敤 W").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "浣跨敤 E").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "浣跨敤 R").SetValue(true));
            menu.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "杩炴嫑!").SetValue(new KeyBind(menu.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            //Harass menu:
            menu.AddSubMenu(new Menu("楠氭壈", "Harass"));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "浣跨敤 Q").SetValue(true));
            menu.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "浣跨敤 E").SetValue(true));
            menu.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "楠氭壈!").SetValue(new KeyBind(menu.Item("Farm").GetValue<KeyBind>().Key, KeyBindType.Press)));
            menu.SubMenu("Harass").AddItem(new MenuItem("HarassActiveT", "楠氭壈 (鍒囨崲)!").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle)));

            //Farming menu:
            menu.AddSubMenu(new Menu("娓呴噹", "Farm"));
            menu.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "浣跨敤 Q").SetValue(false));
            menu.SubMenu("Farm").AddItem(new MenuItem("UseEFarm", "浣跨敤 E").SetValue(false));
            menu.SubMenu("Farm").AddItem(new MenuItem("LaneClearActive", "娓呴噹!").SetValue(new KeyBind(menu.Item("LaneClear").GetValue<KeyBind>().Key, KeyBindType.Press)));

            //Misc Menu:
            menu.AddSubMenu(new Menu("鏉傞」", "Misc"));
            menu.SubMenu("Misc").AddItem(new MenuItem("UseInt", "浣跨敤R鎵撴柇").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("UseGap", "浣跨敤W鎺ヨ繎").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("packet", "浣跨敤灏佸寘").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("towerR", "濉斾笅鑷姩R鏁屼汉").SetValue(true));
            menu.SubMenu("Misc").AddItem(new MenuItem("toggleR", "寮鸿R").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
            menu.SubMenu("Misc").AddItem(new MenuItem("autoW", "W 濡傛灉 HP < %").SetValue(new Slider(40, 0, 100)));

            //Damage after combo:
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "鏄剧ず缁勫悎浼ゅ").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            //Drawings menu:
            menu.AddSubMenu(new Menu("鎶€鑳借寖鍥撮€夐」", "Drawings"));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q 鑼冨洿").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("WRange", "W 鑼冨洿").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("ERange", "E 鑼冨洿").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(new MenuItem("RRange", "R 鑼冨洿").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            menu.SubMenu("Drawings")
                .AddItem(dmgAfterComboItem);
            menu.AddToMainMenu();
menu.AddSubMenu(new Menu("鍒濊姹夊寲", "by chujian"));

menu.SubMenu("by chujian").AddItem(new MenuItem("qunhao", "姹夊寲缇わ細386289593"));
menu.SubMenu("by chujian").AddItem(new MenuItem("qunhao2", "濞冨▋缇わ細13497795"));
            //Events
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
            //Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.PrintChat(ChampionName + " 鍔犺浇鎴愬姛! --- by 鍒濊姹夊寲QQ 5011477");
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q) * 2;

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);

            return (float)damage;
        }

        private static void Combo()
        {
            UseSpells(menu.Item("UseQCombo").GetValue<bool>(), menu.Item("UseWCombo").GetValue<bool>(),
                menu.Item("UseECombo").GetValue<bool>(), menu.Item("UseRCombo").GetValue<bool>());
        }

        private static void UseSpells(bool useQ, bool useW, bool useE, bool useR)
        {
            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var wTarget = SimpleTs.GetTarget(1200, SimpleTs.DamageType.Magical);
            var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            var rTarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);

            if (useW && wTarget != null && W.IsReady())
            {
                castW(wTarget);
            }

            if (useQ && Q.IsReady() && Player.Distance(qTarget) <= Q.Range && qTarget != null)
            {
                Q.Cast();
            }

            if (useE && eTarget != null && E.IsReady() && Player.Distance(eTarget) <= E.Range && E.GetPrediction(eTarget).Hitchance >= HitChance.High)
            {
                E.Cast(E.GetPrediction(eTarget).CastPosition, packets());
            }

            if (useR && rTarget != null && R.IsReady() && Player.Distance(rTarget) < R.Range)
            {
                castR(rTarget);
                return;
            }

        }

        public static bool packets()
        {
            return menu.Item("packet").GetValue<bool>();
        }

        public static void castW(Obj_AI_Hero wTarget)
        {
            //hp sheild
            var hp = menu.Item("autoW").GetValue<Slider>().Value;
            var hpPercent = Player.Health / Player.MaxHealth * 100;

            if (Player.Distance(wTarget) >= W.Range && menu.Item("UseGap").GetValue<bool>() && Player.Distance(wTarget) <= 800 && W.IsReady())
            {
                W.Cast();
            }

            if (hpPercent <= hp && Player.Distance(wTarget) <= 1200 && W.IsReady())
                W.Cast();

            if(Player.Distance(wTarget) <= W.Range && W.IsReady())
                W.Cast();

            
        }

        public static void castR(Obj_AI_Hero target)
        {
            if (GetComboDamage(target) >= target.Health - 100 && R.IsReady())
            {
                R.CastOnUnit(target, packets());
            }

            if (menu.Item("forceR").GetValue<KeyBind>().Active && R.IsReady())
            {
                R.CastOnUnit(target, packets());
            }
        }
        private static void Harass()
        {
            UseSpells(menu.Item("UseQHarass").GetValue<bool>(), false,
                menu.Item("UseEHarass").GetValue<bool>(), false);
        }

        public static void checkUnderTower(){
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (enemy.IsEnemy && Player.Distance(enemy.ServerPosition) <= R.Range && enemy != null)
                {
                    foreach (var turret in ObjectManager.Get<Obj_AI_Turret>())
                    {
                        if (turret != null && turret.IsValid && turret.IsAlly && turret.Health > 0)
                        {
                            if (Vector2.Distance(enemy.Position.To2D(), turret.Position.To2D()) < 950 && R.IsReady())
                            {
                                R.CastOnUnit(enemy, packets());
                            }
                        }
                    }
                }
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            //check if player is dead
            if (Player.IsDead) return;

            Orbwalker.SetAttack(true);

            if (menu.Item("towerR").GetValue<bool>())
                checkUnderTower();

            if (menu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (menu.Item("LaneClearActive").GetValue<KeyBind>().Active)
                {
                    Farm();
                }

                if (menu.Item("HarassActive").GetValue<KeyBind>().Active || menu.Item("HarassActiveT").GetValue<KeyBind>().Active)
                    Harass();
            }
        }

        private static void Farm()
        {
            //if (!Orbwalking.CanMove(40)) return;

            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
            var allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range + E.Width, MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm").GetValue<bool>();
            var useE = menu.Item("UseEFarm").GetValue<bool>();

            if (useE && E.IsReady())
            {
                var ePos = E.GetLineFarmLocation(allMinionsE);
                if (ePos.MinionsHit >= 3)
                    E.Cast(ePos.Position, true);
            }

            if (useQ && Q.IsReady())
            {
                var qPos = Q.GetCircularFarmLocation(allMinionsQ);
                if (qPos.MinionsHit >= 2)
                    Q.Cast(qPos.Position, true);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = menu.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }

        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!menu.Item("UseInt").GetValue<bool>()) return;

            if (Player.Distance(unit) <= R.Range && unit != null && R.IsReady())
            {
                R.CastOnUnit(unit);
            }
        }


    }
}
