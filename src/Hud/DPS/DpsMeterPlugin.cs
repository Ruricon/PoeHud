﻿using System;
using System.Collections.Generic;
using System.Linq;

using PoeHUD.Controllers;
using PoeHUD.Framework.Helpers;
using PoeHUD.Hud.UI;
using PoeHUD.Models;
using PoeHUD.Poe.Components;

using SharpDX;
using SharpDX.Direct3D9;

namespace PoeHUD.Hud.Dps
{
    public class DpsMeterPlugin : SizedPlugin<DpsMeterSettings>
    {
        private const double DPS_PERIOD = 0.2;

        private DateTime lastTime;

        private Dictionary<int, int> lastMonsters = new Dictionary<int, int>();

        private double[] damageMemory = new double[10];

        private int damageMemoryIndex;

        private int maxDps;

        public DpsMeterPlugin(GameController gameController, Graphics graphics, DpsMeterSettings settings)
            : base(gameController, graphics, settings)
        {
            lastTime = DateTime.Now;
            GameController.Area.OnAreaChange += area =>
            {
                lastTime = DateTime.Now;
                maxDps = 0;
                damageMemory = new double[10];
                lastMonsters.Clear();
            };
        }

        public override void Render()
        {
            base.Render();
            if (!Settings.Enable)
            {
                return;
            }

            DateTime nowTime = DateTime.Now;
            TimeSpan elapsedTime = nowTime - lastTime;
            if (elapsedTime.TotalSeconds > DPS_PERIOD)
            {
                damageMemoryIndex++;
                if (damageMemoryIndex >= damageMemory.Length)
                {
                    damageMemoryIndex = 0;
                }
                damageMemory[damageMemoryIndex] = CalculateDps(elapsedTime);
                lastTime = nowTime;
            }

            Vector2 position = StartDrawPointFunc();
            var dps = (int)damageMemory.Average();
            maxDps = Math.Max(dps, maxDps);

            string dpsText = dps + " DPS";
            string peakText = maxDps + " peak DPS";
            Size2 dpsSize = Graphics.DrawText(dpsText, Settings.DpsTextSize, position, FontDrawFlags.Right);
            Size2 peakSize = Graphics.DrawText(peakText, Settings.PeakDpsTextSize, position.Translate(0, dpsSize.Height),
                FontDrawFlags.Right);

            int width = Math.Max(peakSize.Width, dpsSize.Width);
            int height = dpsSize.Height + peakSize.Height;
            var bounds = new RectangleF(position.X - 5 - width, position.Y - 5, width + 10, height + 10);
            Graphics.DrawBox(bounds, Settings.BackgroundColor);

            Size = bounds.Size;
            Margin = new Vector2(5, 0);
        }

        private double CalculateDps(TimeSpan elapsedTime)
        {
            int totalDamage = 0;
            var monsters = new Dictionary<int, int>();
            foreach (EntityWrapper monster in GameController.Entities.Where(x => x.HasComponent<Monster>() && x.IsHostile))
            {
                int hp = monster.IsAlive ? monster.GetComponent<Life>().CurHP + monster.GetComponent<Life>().CurES : 0;
                if (hp > -1000000 && hp < 10000000)
                {
                    int lastHP;
                    if (lastMonsters.TryGetValue(monster.Id, out lastHP))
                    {
                        // make this a separte if statement to prevent dictionary already containing item
                        if (lastHP > hp) 
                        {
                            totalDamage += lastHP - hp;
                        }
                    }
                    monsters.Add(monster.Id, hp);
                }
            }
            lastMonsters = monsters;
            return totalDamage / elapsedTime.TotalSeconds;
        }
    }
}