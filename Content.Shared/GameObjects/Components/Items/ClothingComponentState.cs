﻿using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;
using System;

namespace Content.Shared.GameObjects.Components.Items
{
    [Serializable, NetSerializable]
    public class ClothingComponentState : ItemComponentState
    {
        public string ClothingEquippedPrefix { get; set; }

        public ClothingComponentState(string clothingEquippedPrefix, string equippedPrefix) : base(equippedPrefix, ContentNetIDs.CLOTHING)
        {
            ClothingEquippedPrefix = clothingEquippedPrefix;
        }
    }
}
