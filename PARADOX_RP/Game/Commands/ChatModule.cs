﻿using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Resources.Chat.Api;
using PARADOX_RP.Core.Factories;
using PARADOX_RP.Core.Module;
using PARADOX_RP.Game.Administration;
using PARADOX_RP.Game.MiniGames;
using PARADOX_RP.Game.MiniGames.Models;
using AltV.Net.Enums;
using PARADOX_RP.Utils.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net.Data;

namespace PARADOX_RP.Game.Commands
{
    class ChatModule : ModuleBase<ChatModule>, IScript
    {
        public ChatModule() : base("Chat") { }

        [ClientEvent("chat:message")]
        public void OnChatMessage(IPlayer player, string msg)
        {
            if (msg.Length == 0 || msg[0] == '/') return;
            Alt.Log(msg);
            player.SendChatMessage(msg);

        }

        [CommandEvent(CommandEventType.CommandNotFound)]
        public void OnCommandNotFound(IPlayer player, string cmd)
        {
            player.SendChatMessage("{FF0000}[Server] {FFFFFF}Befehl nicht gefunden.");
        }


        [Command("minigame")]
        public void enterMinigameCommand(PXPlayer player, string minigameModule)
        {
            MinigameTypes _minigameType = Enum.Parse<MinigameTypes>(minigameModule);

            MinigameModule.Instance.ChooseMinigame(player, _minigameType);
        }

        [Command("veh")]
        public async void veh(PXPlayer player, string vehicleModel)
        {
            try
            {
                await AltAsync.CreateVehicle(Alt.Hash(vehicleModel), player.Position, new Rotation(0, 0, 0));
            }
            catch
            {
                AltAsync.Log("Vehicle-Hash not found.");
            }
        }

        [Command("pos")]
        public void pos(PXPlayer player, string positionName)
        {
            AltAsync.Log($"{positionName} | {player.Position.X.ToString().Replace(",", ".")}, {player.Position.Y.ToString().Replace(",", ".")}, {player.Position.Z.ToString().Replace(",", ".")}");
        }

        [Command("aduty")]
        public async void aduty(PXPlayer player)
        {
            await AdministrationModule.Instance.OnKeyPress(player, KeyEnumeration.F9);
        }
    }
}
