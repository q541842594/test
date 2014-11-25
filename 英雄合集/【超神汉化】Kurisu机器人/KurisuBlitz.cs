using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace KurisuBlitz
{
    /*   _____       _    _____           _ 
     *  |   __|___ _| |  |  |  |___ ___ _| |
     *  |  |  | . | . |  |     | .'|   | . |
     *  |_____|___|___|  |__|__|__,|_|_|___|
     *                               
     * Blitz - The God Hand
     * 
     * Revision: 104 30/10/2014
     * + Fixed powerfist (E)
     * + Lag free drawings
     * 
     * Revision: 103 - 13/10/2014
     * + Added Q,E Killsteal
     * + New KS Menu
     * + New smart anti gapcloser should pull gapclosers away from allies
     * 
     * Revision: 101 - 10/10/2014
     * + added target selector
     * + should focus selected target
     * + fixed interruptable spell printing in game
     * + will only power fist if Q is not avaiable and target is close enough
     * */
                                                         
    internal class KurisuBlitz
    {
        
        private static Menu _menu;
        private static Obj_AI_Hero _target;
        private static Orbwalking.Orbwalker _orbwalker;
        private static readonly Obj_AI_Hero _player = ObjectManager.Player;

        private static readonly Spell Q = new Spell(SpellSlot.Q, 925f);
        private static readonly Spell E = new Spell(SpellSlot.E, _player.AttackRange);
        private static readonly Spell R = new Spell(SpellSlot.R, 550f);

        private static readonly List<Spell> blitzDrawingList = new List<Spell>();
        //private static List<InterruptableSpell> blitzInterruptList = new List<InterruptableSpell>();

        public KurisuBlitz()
        {
            
            Console.WriteLine("Blitzcrank assembly is loading...");
            CustomEvents.Game.OnGameLoad += BlitzOnLoad;
        }

        private void BlitzOnLoad(EventArgs args)
        {
            if (_player.BaseSkinName != "Blitzcrank") return;

            // Set Q Prediction
            Q.SetSkillshot(0.25f, 70f, 1800f, true, SkillshotType.SkillshotLine);
            

            // Drawing List
            blitzDrawingList.Add(Q);
            blitzDrawingList.Add(R);

            // Load Menu
            BlitzMenu();

            // Load Drawings
            Drawing.OnDraw += BlitzOnDraw;

            // OnTick
            Game.OnGameUpdate += BlitzOnUpdate;

            // Interrupter
            Interrupter.OnPossibleToInterrupt += BlitzOnInterrupt;

            // OnGapCloser
            AntiGapcloser.OnEnemyGapcloser += BlitzOnGapcloser;

        }

        private void BlitzOnGapcloser(ActiveGapcloser gapcloser)
        {
            if (!_menu.Item("gapcloser").GetValue<bool>()) return;

            foreach (
                var a in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(a => a.IsValid && a.IsVisible && !a.IsDead && a.Team == _player.Team))
            {

                var senderPos = gapcloser.End;
                var validPos = senderPos - Vector3.Normalize(_player.Position - senderPos)*Q.Range;

                if (_player.Distance(validPos) > a.Distance(a.Position))
                {
                    if (_player.Distance(validPos) > 200f)
                        Q.Cast(senderPos);
                }
            }
        }

        // not tested
        private void BlitzOnInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (_menu.Item("interrupt").GetValue<bool>())
            {   if (unit.Distance(_player.Position) < Q.Range)
                    Q.Cast(unit);        
                else if (unit.Distance(_player.Position) < R.Range)
                    R.Cast();
            }
        }

        private void BlitzOnDraw(EventArgs args)
        {
            foreach (var spell in blitzDrawingList)
            {
                var circle = _menu.SubMenu("drawings").Item("draw" + spell.Slot).GetValue<Circle>();
                if (circle.Active)
                    Utility.DrawCircle(_player.Position, spell.Range, circle.Color, 1, 1);
            }

            if (_target != null)
            {
                Utility.DrawCircle(_target.Position, _target.BoundingRadius, Color.Red, 10, 1);
                
            }           
        }


        private void BlitzOnUpdate(EventArgs args)
        {
            try
            {
                _target = SimpleTs.GetSelectedTarget() ?? SimpleTs.GetTarget(1000, SimpleTs.DamageType.Physical);

                // do KS
                GodKS(Q);
                GodKS(R);
                GodKS(E);

                int actualHealthSetting = _menu.Item("hneeded").GetValue<Slider>().Value;
                int actualHealthPercent = (int) (_player.Health/_player.MaxHealth*100);

                if (actualHealthPercent < actualHealthSetting) return;

                // use the god hand

                if (SimpleTs.GetSelectedTarget() == null || !(_target.Distance(_player.Position) > 750))
                {
                    TheGodHand(_target);
                }

                // powerfist that hoe
                    foreach (
                        var e in
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(
                                    e =>
                                        e.Team != _player.Team && e.IsValid && !e.IsDead &&
                                        e.Distance(_player.Position) <= _player.AttackRange))
                    {
                        if (_menu.Item("useE").GetValue<bool>() && !Q.IsReady())
                            E.CastOnUnit(_player);
                    }
                
            }
            catch (Exception ex)
            {
                //Game.PrintChat(ex.Message);
                Console.WriteLine(ex);
            }

        }

        private void TheGodHand(Obj_AI_Base target)
        {
            bool keydown = _menu.Item("combokey").GetValue<KeyBind>().Active;
            if (SimpleTs.GetSelectedTarget() != null && _target.Distance(_player.Position) > 1000)
                return;

            if (target != null && Q.IsReady())
            {
                PredictionOutput prediction = Q.GetPrediction(target);
                if (keydown)
                {
                    if ((target.Distance(_player.Position) > _menu.Item("dneeded").GetValue<Slider>().Value)
                        && (target.Distance(_player.Position) < _menu.Item("dneeded2").GetValue<Slider>().Value))
                    if (_menu.Item("dograb" + target.SkinName).GetValue<StringList>().SelectedIndex == 0) return;
                    if (prediction.Hitchance == HitChance.High && _menu.Item("hitchance").GetValue<StringList>().SelectedIndex == 2)
                            Q.Cast(prediction.CastPosition);
                    else if (prediction.Hitchance == HitChance.Medium && _menu.Item("hitchance").GetValue<StringList>().SelectedIndex == 1)
                        Q.Cast(prediction.CastPosition);
                    else if (prediction.Hitchance == HitChance.Low && _menu.Item("hitchance").GetValue<StringList>().SelectedIndex == 0)
                        Q.Cast(prediction.CastPosition);      
                    
                }
            }

            foreach (
                   var e in
                       ObjectManager.Get<Obj_AI_Hero>()
                           .Where(
                               e =>
                                   e.Team != _player.Team && !e.IsDead && e.IsValid &&
                                   Vector2.DistanceSquared(_player.Position.To2D(), e.ServerPosition.To2D()) <
                                   Q.Range * Q.Range && _menu.Item("dograb" + e.SkinName).GetValue<StringList>().SelectedIndex == 2))
            {
                if (e.Distance(_player.Position) > _menu.Item("dneeded").GetValue<Slider>().Value)
                {
                    PredictionOutput prediction = Q.GetPrediction(e);
                    if (prediction.Hitchance == HitChance.Immobile && _menu.Item("immobile").GetValue<bool>())
                        Q.Cast(prediction.CastPosition);
                    if (prediction.Hitchance == HitChance.Dashing && _menu.Item("dashing").GetValue<bool>())
                        Q.Cast(prediction.CastPosition);
                }
            }
        }

                        
        private void GodKS(Spell spell)
        {
            if (_menu.Item("killsteal" + spell.Slot).GetValue<bool>() && spell.IsReady())
            {
                foreach (
                    var enemy in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(e => e.Team != _player.Team && e.Distance(_player.Position) < spell.Range))
                {
                    var ksDmg = _player.GetSpellDamage(enemy, spell.Slot);
                    if (ksDmg > enemy.Health)
                    {
                        PredictionOutput po = spell.GetPrediction(enemy);
                        if (po.Hitchance >= HitChance.Medium)
                            spell.Cast(po.CastPosition);
                    }

                }
            }
        }

        private void BlitzMenu()
        {
            _menu = new Menu("Kurisu: Blitz", "blitz", true);

            Menu blitzOrb = new Menu("璧扮爫", "orbwalker");
            _orbwalker = new Orbwalking.Orbwalker(blitzOrb);
            _menu.AddSubMenu(blitzOrb);

            Menu blitzTS = new Menu("鐩爣閫夋嫨", "tselect");
            SimpleTs.AddToMenu(blitzTS);
            _menu.AddSubMenu(blitzTS);
            
            Menu menuD = new Menu("鏄剧ず", "drawings");
            menuD.AddItem(new MenuItem("drawQ", "Q鑼冨洿")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            menuD.AddItem(new MenuItem("drawR", "R鑼冨洿")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            _menu.AddSubMenu(menuD);
            
            Menu menuG = new Menu("Q", "autograb");
            menuG.AddItem(new MenuItem("hitchance", "鍛戒腑鏈轰細"))
                .SetValue(new StringList(new[] {"Low", "Medium", "High"}, 2));
            menuG.AddItem(new MenuItem("dneeded", "Min璺濈")).SetValue(new Slider(255, 0, (int)Q.Range));
            menuG.AddItem(new MenuItem("dneeded2", "Max璺濈")).SetValue(new Slider((int)Q.Range, 0, (int)Q.Range));
            menuG.AddItem(new MenuItem("dashing", "鑷姩Q绐佽繘")).SetValue(true);
            menuG.AddItem(new MenuItem("immobile", "鑷姩Q涓嶅姩")).SetValue(true);
            menuG.AddItem(new MenuItem("hneeded", "鐢熷懡< %涓峇")).SetValue(new Slider(0));
            menuG.AddItem(new MenuItem("sep", ""));
            
            foreach (
                var e in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            e =>
                                e.Team != _player.Team))
            {
                menuG.AddItem(new MenuItem("dograb" + e.SkinName, e.SkinName))
                    .SetValue(new StringList(new[] {"Dont Grab ", "Normal Grab ", "Auto Grab "}, 1));
            }
            _menu.AddSubMenu(menuG);
            Menu menuK = new Menu("鎶㈠ご", "blitzks");
            menuK.AddItem(new MenuItem("killstealQ", "浣跨敤Q")).SetValue(false);
            menuK.AddItem(new MenuItem("killstealE", "浣跨敤E")).SetValue(false);
            menuK.AddItem(new MenuItem("killstealR", "浣跨敤R")).SetValue(false);
            _menu.AddSubMenu(menuK);
            _menu.AddItem(new MenuItem("gapcloser", "闃茬獊")).SetValue(true);
            _menu.AddItem(new MenuItem("interrupt", "鎵撴柇")).SetValue(true);
            _menu.AddItem(new MenuItem("useE", "鑷姩E")).SetValue(true);
            _menu.AddItem(new MenuItem("combokey", "杩炴嫑")).SetValue(new KeyBind(32, KeyBindType.Press));
            _menu.AddToMainMenu();

			
			Menu menuQun = new Menu("瓒呯姹夊寲", "by weilai");
            menuQun.AddItem(new MenuItem("qunhao", "姹夊寲缇わ細386289593"));
            menuQun.AddItem(new MenuItem("qunhao2", "濞冨▋缇わ細13497795"));
			

        }
    }
}
