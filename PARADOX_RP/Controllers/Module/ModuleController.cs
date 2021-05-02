﻿using AltV.Net;
using PARADOX_RP.Core.Module;
using System;
using System.Collections.Generic;
using PARADOX_RP.Core.Extensions;
using AltV.Net.Async;
using PARADOX_RP.Core.Factories;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using PARADOX_RP.Game.Misc.Progressbar.Extensions;
using PARADOX_RP.Utils;
using PARADOX_RP.Controllers.Interval.Interface;
using PARADOX_RP.Controllers.Login.Interface;
using PARADOX_RP.Controllers.Event.Interface;

namespace PARADOX_RP.Controllers
{
    class ModuleController : IModuleController
    {
        private readonly IEnumerable<IModuleBase> _modules;
        public ModuleController(IEnumerable<IModuleBase> modules, IEventController eventController, ILoginController loginController, IIntervalController intervalController)
        {
            _modules = modules;

            eventController.OnClient<PXPlayer>("Pressed_E", PressedE);
            eventController.OnClient<PXPlayer>("Pressed_F9", PressedF9);
            eventController.OnClient<PXPlayer>("PlayerReady", OnPlayerConnect);

            AltAsync.OnPlayerDead += OnPlayerDead;
            AltAsync.OnPlayerDisconnect += OnPlayerDisconnect;
            AltAsync.OnColShape += OnColShape;
            AltAsync.OnPlayerEnterVehicle += OnPlayerEnterVehicle;
            AltAsync.OnPlayerLeaveVehicle += OnPlayerLeaveVehicle;

            //TODO: add module base cons
            intervalController.SetInterval(1000 * 60, async (s, e) =>
            {
                await loginController.SavePlayers();

                await _modules.ForEach(async e =>
                {
                    if (e.Enabled)
                        await e.OnEveryMinute();
                });
            });
        }

        private async Task OnPlayerDead(IPlayer client, IEntity killerEntity, uint weapon)
        {
            PXPlayer player = (PXPlayer)client;
            PXPlayer killer = null;

            if (killer is IPlayer)
            {
                killer = (PXPlayer)killerEntity;
            }
            else if (killer is IVehicle)
            {
                killer = (PXPlayer)await ((IVehicle)killerEntity).GetDriverAsync();
            }

            await _modules.ForEach(e =>
             {
                 if (e.Enabled)
                     e.OnPlayerDeath(player, killer, weapon);
             });
        }

        public void Load()
        {
            _modules.ForEach(e =>
            {
                if (e.Enabled)
                    e.OnModuleLoad();
            });
        }

        private async void PressedE(PXPlayer player)
        {
            if (!player.LoggedIn) return;
            if (player.CancelProgressBar()) return;

            await _modules.ForEach(async e =>
            {
                if (e.Enabled)
                    if (await e.OnKeyPress(player, Utils.Enums.KeyEnumeration.E)) return;
            });
        }

        private async void PressedF9(PXPlayer player)
        {
            if (!player.LoggedIn) return;

            await _modules.ForEach(async e =>
            {
                if (e.Enabled)
                    if (await e.OnKeyPress(player, Utils.Enums.KeyEnumeration.F9)) return;
            });
        }

        private async void OnPlayerConnect(PXPlayer pxPlayer)
        {
            await _modules.ForEach(e =>
            {
                if (e.Enabled)
                    e.OnPlayerConnect(pxPlayer);
            });
        }

        private async Task OnPlayerDisconnect(IPlayer player, string reason)
        {
            PXPlayer pxPlayer = (PXPlayer)player;

            if (pxPlayer.LoggedIn)
                Pools.Instance.Remove(pxPlayer.SqlId, pxPlayer);

            await _modules.ForEach(e =>
            {
                if (e.Enabled)
                    e.OnPlayerDisconnect(pxPlayer);
            });

            pxPlayer.LoggedIn = false;
        }

        private async Task OnColShape(IColShape colShape, IEntity targetEntity, bool state)
        {

            PXPlayer pxPlayer = (PXPlayer)targetEntity;
            await _modules.ForEach(e =>
            {
                if (e.Enabled && state)
                    e.OnColShapeEntered(pxPlayer, colShape);
            });
        }


        private async Task OnPlayerLeaveVehicle(IVehicle vehicle, IPlayer player, byte seat)
        {
            await player.EmitAsync("playerLeaveVehicle", vehicle, seat);

            await _modules.ForEach(async e =>
            {
                if (e.Enabled)
                    await e.OnPlayerLeaveVehicle(vehicle, player, seat);
            });
        }

        private async Task OnPlayerEnterVehicle(IVehicle vehicle, IPlayer player, byte seat)
        {
            await player.EmitAsync("playerEnterVehicle", vehicle, seat);

            await _modules.ForEach(async e =>
            {
                if (e.Enabled)
                    await e.OnPlayerEnterVehicle(vehicle, player, seat);
            });
        }
    }
}
