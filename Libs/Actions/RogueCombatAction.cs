﻿using Libs.GOAP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Libs.Actions
{
    public class RogueCombatAction : CombatActionBase
    {
        public RogueCombatAction(WowProcess wowProcess, PlayerReader playerReader, StopMoving stopMoving) : base(wowProcess, playerReader, stopMoving)
        {
        }

        private ConsoleKey SinisterStrike => actionBar.HotKey2 ? ConsoleKey.D2 : ConsoleKey.Escape;
        private ConsoleKey SliceAndDice => actionBar.HotKey3 && !IsOnCooldown(ConsoleKey.D3, 20) ? ConsoleKey.D3 : ConsoleKey.Escape;
        private ConsoleKey Eviscerate => actionBar.HotKey4&& !IsOnCooldown(ConsoleKey.D4, 5)? ConsoleKey.D4 : ConsoleKey.Escape;
        private ConsoleKey Evasion => ConsoleKey.D5;
        private ConsoleKey Vanish => ConsoleKey.D8;

        protected override async Task Fight()
        {
            this.actionBar = playerReader.ActionBarUseable_1To24;

            if (playerReader.HealthPercent < 50 && !IsOnCooldown(Evasion, 600))
            {
                await PressKey(Evasion);
            }

            if (playerReader.HealthPercent < 10 && !IsOnCooldown(Vanish, 600))
            {
                await DoVanish();
                return;
            }

            var key = new List<ConsoleKey> { SliceAndDice, Eviscerate, SinisterStrike }
                .Where(key => key != ConsoleKey.Escape)
                .ToList()
                .FirstOrDefault();

            if (key != 0)
            {
                await PressKey(key);
                RaiseEvent(new ActionEvent(GoapKey.shouldloot, true));
            }
        }

        private async Task DoVanish()
        {
            await PressKey(Vanish);

            await new WowProcess().KeyPress(ConsoleKey.F3, 400); //clear target
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(1000);
                if (playerReader.PlayerBitValues.PlayerInCombat || playerReader.HealthPercent > 60)
                {
                    return;
                }
            }
            return;
        }

        public override void OnActionEvent(object sender, ActionEvent e)
        {
            if (e.Key == GoapKey.newtarget)
            {
                Debug.WriteLine("Rend reset");
                LastClicked.Remove(ConsoleKey.D3);
                LastClicked.Remove(ConsoleKey.D4);

                LastClicked[ConsoleKey.D3] = DateTime.Now.AddSeconds(-15);
                LastClicked[ConsoleKey.D4] = DateTime.Now;
            }
        }
    }
}