﻿using Core.Goals;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Core
{
    public class MountHandler
    {
        private readonly ILogger logger;
        private readonly ConfigurableInput input;
        private readonly ClassConfiguration classConfig;
        private readonly Wait wait;
        private readonly PlayerReader playerReader;
        private readonly StopMoving stopMoving;

        private readonly int minLevelToMount = 30;
        private readonly int mountCastTimeMs = 3000;

        public MountHandler(ILogger logger, ConfigurableInput input, ClassConfiguration classConfig, Wait wait, PlayerReader playerReader, StopMoving stopMoving)
        {
            this.logger = logger;
            this.classConfig = classConfig;
            this.input = input;
            this.wait = wait;
            this.playerReader = playerReader;
            this.stopMoving = stopMoving;
        }

        public async Task MountUp()
        {
            if (playerReader.PlayerLevel >= minLevelToMount)
            {
                if (playerReader.PlayerClass == PlayerClassEnum.Druid)
                {
                    classConfig.Form
                      .Where(s => s.FormEnum == Form.Druid_Travel)
                      .ToList()
                      .ForEach(async k => await input.KeyPress(k.ConsoleKey, 50));
                }
                else
                {
                    await stopMoving.Stop();
                    await wait.Update(1);

                    if (playerReader.PlayerBitValues.IsFalling)
                    {
                        (bool notfalling, double fallingElapsedMs) = await wait.InterruptTask(10000, () => !playerReader.PlayerBitValues.IsFalling);
                        if (!notfalling)
                        {
                            logger.LogInformation($"{GetType().Name}: waited for landing {fallingElapsedMs}ms");
                        }
                    }

                    int beforeCastEventValue = playerReader.CastEvent.Value;
                    await input.TapMount();
                    await wait.Interrupt(mountCastTimeMs, () => playerReader.PlayerBitValues.IsMounted || beforeCastEventValue != playerReader.CastEvent.Value);
                }
            }
        }

        private void Log(string text)
        {
            logger.LogInformation(text);
        }
    }
}
