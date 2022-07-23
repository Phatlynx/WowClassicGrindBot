﻿using System.Collections.Specialized;

namespace Core
{
    public class ActionBarBits
    {
        private readonly AddonDataProvider reader;
        private readonly int[] cells;

        private readonly BitVector32[] bits;
        private readonly PlayerReader playerReader;

        public ActionBarBits(PlayerReader playerReader, AddonDataProvider reader, params int[] cells)
        {
            this.reader = reader;
            this.playerReader = playerReader;
            this.cells = cells;

            bits = new BitVector32[cells.Length];
            for (int i = 0; i < bits.Length; i++)
            {
                bits[i] = new(reader.GetInt(cells[i]));
            }
        }

        public void Update()
        {
            for (int i = 0; i < bits.Length; i++)
            {
                bits[i] = new(reader.GetInt(cells[i]));
            }
        }

        // https://wowwiki-archive.fandom.com/wiki/ActionSlot
        public bool Is(KeyAction keyAction)
        {
            if (keyAction.Slot == 0) return false;

            int index = Stance.ToSlot(keyAction, playerReader) - 1;
            return bits[index / ActionBar.BIT_PER_CELL][Mask.M[index % ActionBar.BIT_PER_CELL]];
        }
    }
}
