﻿using Content.Client.GameObjects;
using Content.Client.GameObjects.Components.Actor;
using Content.Client.GameObjects.Components.Clothing;
using Content.Client.GameObjects.Components.Construction;
using Content.Client.GameObjects.Components.Power;
using Content.Client.GameObjects.Components.IconSmoothing;
using Content.Client.GameObjects.Components.Storage;
using Content.Client.GameObjects.Components.Weapons.Ranged;
using Content.Client.GameTicking;
using Content.Client.Input;
using Content.Client.Interfaces;
using Content.Client.Interfaces.GameObjects;
using Content.Client.Interfaces.Parallax;
using Content.Client.Parallax;
using Content.Shared.GameObjects.Components.Weapons.Ranged;
using Content.Shared.Interfaces;
using Robust.Client;
using Robust.Client.Interfaces;
using Robust.Client.Interfaces.Graphics.Overlays;
using Robust.Client.Interfaces.Input;
using Robust.Client.Player;
using Robust.Client.Utility;
using Robust.Shared.ContentPack;
using Robust.Shared.Interfaces.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;
using System;
using Content.Client.Chat;
using Content.Client.GameObjects.Components;
using Content.Client.GameObjects.Components.Mobs;
using Content.Client.GameObjects.Components.Research;
using Content.Client.GameObjects.Components.Sound;
using Content.Client.Interfaces.Chat;
using Content.Client.Research;
using Content.Client.UserInterface;
using Content.Shared.GameObjects.Components.Markers;
using Content.Shared.GameObjects.Components.Materials;
using Content.Shared.GameObjects.Components.Mobs;
using Content.Shared.GameObjects.Components.Research;
using Robust.Client.Interfaces.UserInterface;
using Robust.Shared.Log;

namespace Content.Client
{
    public class EntryPoint : GameClient
    {
        public override void Init()
        {
#if DEBUG
            GodotResourceCopy.DoDirCopy("../../Resources", "Content");
#endif
            var factory = IoCManager.Resolve<IComponentFactory>();
            var prototypes = IoCManager.Resolve<IPrototypeManager>();

            factory.RegisterIgnore("Interactable");
            factory.RegisterIgnore("Destructible");
            factory.RegisterIgnore("Temperature");
            factory.RegisterIgnore("PowerTransfer");
            factory.RegisterIgnore("PowerNode");
            factory.RegisterIgnore("PowerProvider");
            factory.RegisterIgnore("PowerDevice");
            factory.RegisterIgnore("PowerStorage");
            factory.RegisterIgnore("PowerGenerator");

            factory.RegisterIgnore("Wirecutter");
            factory.RegisterIgnore("Screwdriver");
            factory.RegisterIgnore("Multitool");
            factory.RegisterIgnore("Welder");
            factory.RegisterIgnore("Wrench");
            factory.RegisterIgnore("Crowbar");
            factory.Register<ClientRangedWeaponComponent>();
            factory.RegisterIgnore("HitscanWeapon");
            factory.RegisterIgnore("ProjectileWeapon");
            factory.RegisterIgnore("Projectile");
            factory.RegisterIgnore("MeleeWeapon");

            factory.RegisterIgnore("Storeable");

            factory.RegisterIgnore("Stack");

            factory.Register<HandsComponent>();
            factory.RegisterReference<HandsComponent, IHandsComponent>();
            factory.Register<ClientStorageComponent>();
            factory.Register<ClientInventoryComponent>();
            factory.Register<PowerDebugTool>();
            factory.Register<ConstructorComponent>();
            factory.Register<ConstructionGhostComponent>();
            factory.Register<IconSmoothComponent>();
            factory.Register<DamageableComponent>();
            factory.Register<ClothingComponent>();
            factory.Register<ItemComponent>();
            factory.Register<MaterialComponent>();
            factory.Register<SoundComponent>();
            factory.Register<MaterialStorageComponent>();
            factory.RegisterReference<MaterialStorageComponent, SharedMaterialStorageComponent>();

            factory.RegisterReference<ClothingComponent, ItemComponent>();

            factory.Register<SpeciesUI>();
            factory.Register<CharacterInterface>();

            factory.RegisterIgnore("Construction");
            factory.RegisterIgnore("Apc");
            factory.RegisterIgnore("Door");
            factory.RegisterIgnore("PoweredLight");
            factory.RegisterIgnore("Smes");
            factory.RegisterIgnore("Powercell");
            factory.RegisterIgnore("HandheldLight");
            factory.RegisterIgnore("LightBulb");
            factory.RegisterIgnore("Healing");
            factory.RegisterIgnore("Catwalk");
            factory.RegisterIgnore("BallisticMagazine");
            factory.RegisterIgnore("BallisticMagazineWeapon");
            factory.RegisterIgnore("BallisticBullet");
            factory.RegisterIgnore("HitscanWeaponCapacitor");

            prototypes.RegisterIgnore("material");

            factory.RegisterIgnore("PowerCell");

            factory.Register<SharedSpawnPointComponent>();

            factory.Register<SharedLatheComponent>();
            factory.Register<LatheDatabaseComponent>();

            factory.RegisterReference<LatheDatabaseComponent, SharedLatheDatabaseComponent>();

            factory.Register<CameraRecoilComponent>();
            factory.RegisterReference<CameraRecoilComponent, SharedCameraRecoilComponent>();

            factory.Register<SubFloorHideComponent>();

            factory.RegisterIgnore("AiController");
            factory.RegisterIgnore("PlayerInputMover");

            IoCManager.Register<IClientNotifyManager, ClientNotifyManager>();
            IoCManager.Register<ISharedNotifyManager, ClientNotifyManager>();
            IoCManager.Register<IClientGameTicker, ClientGameTicker>();
            IoCManager.Register<IParallaxManager, ParallaxManager>();
            IoCManager.Register<IChatManager, ChatManager>();
            IoCManager.BuildGraph();

            IoCManager.Resolve<IParallaxManager>().LoadParallax();
            IoCManager.Resolve<IBaseClient>().PlayerJoinedServer += SubscribePlayerAttachmentEvents;

            var stylesheet = new NanoStyle();

            IoCManager.Resolve<IUserInterfaceManager>().Stylesheet = stylesheet.Stylesheet;
        }

        /// <summary>
        /// Subscribe events to the player manager after the player manager is set up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void SubscribePlayerAttachmentEvents(object sender, EventArgs args)
        {
            IoCManager.Resolve<IPlayerManager>().LocalPlayer.EntityAttached += AttachPlayerToEntity;
            IoCManager.Resolve<IPlayerManager>().LocalPlayer.EntityDetached += DetachPlayerFromEntity;
            AttachPlayerToEntity(IoCManager.Resolve<IPlayerManager>().LocalPlayer, EventArgs.Empty);
        }

        /// <summary>
        /// Add the character interface master which combines all character interfaces into one window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void AttachPlayerToEntity(object sender, EventArgs args)
        {
            var localplayer = (LocalPlayer)sender;

            localplayer.ControlledEntity?.AddComponent<CharacterInterface>();
        }

        /// <summary>
        /// Remove the character interface master from this entity now that we have detached ourselves from it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void DetachPlayerFromEntity(object sender, EventArgs args)
        {
            var localplayer = (LocalPlayer)sender;
            //Wont work atm, controlled entity gets nulled before this event fires
            localplayer.ControlledEntity?.RemoveComponent<CharacterInterface>();
        }

        public override void PostInit()
        {
            base.PostInit();

            // Setup key contexts
            var inputMan = IoCManager.Resolve<IInputManager>();
            ContentContexts.SetupContexts(inputMan.Contexts);

            IoCManager.Resolve<IClientNotifyManager>().Initialize();
            IoCManager.Resolve<IClientGameTicker>().Initialize();
            IoCManager.Resolve<IOverlayManager>().AddOverlay(new ParallaxOverlay());
            IoCManager.Resolve<IChatManager>().Initialize();
        }

        public override void Update(AssemblyLoader.UpdateLevel level, float frameTime)
        {
            base.Update(level, frameTime);

            switch (level)
            {
                case AssemblyLoader.UpdateLevel.FramePreEngine:
                    var renderFrameEventArgs = new RenderFrameEventArgs(frameTime);
                    IoCManager.Resolve<IClientNotifyManager>().FrameUpdate(renderFrameEventArgs);
                    IoCManager.Resolve<IClientGameTicker>().FrameUpdate(renderFrameEventArgs);
                    break;
            }
        }
    }
}
