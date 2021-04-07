﻿using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Enums;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PARADOX_RP.Core.Database;
using PARADOX_RP.Core.Database.Models;
using PARADOX_RP.Core.Factories;
using PARADOX_RP.Core.Module;
using PARADOX_RP.Game.Char;
using PARADOX_RP.Game.Misc.Progressbar;
using PARADOX_RP.Game.Misc.Progressbar.Extensions;
using PARADOX_RP.Game.Moderation;
using PARADOX_RP.Handlers.Login;
using PARADOX_RP.Handlers.Login.Interface;
using PARADOX_RP.UI;
using PARADOX_RP.UI.Windows;
using PARADOX_RP.Utils;
using PARADOX_RP.Utils.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PARADOX_RP.Game.Login
{
    class LoginModule : ModuleBase<LoginModule>
    {
        private readonly ILoginController _loginHandler;

        public LoginModule(ILoginController loginHandler) : base("Login")
        {
            _loginHandler = loginHandler;

            AltAsync.OnClient<PXPlayer, string, string>("RequestLoginResponse", RequestLoginResponse);
        }

        public override void OnModuleLoad()
        {

        }

        public override async void OnPlayerConnect(PXPlayer player)
        {
            player.Model = (uint)PedModel.FreemodeMale01;
            await player.SpawnAsync(new Position(0, 0, 72));

            if (Configuration.Instance.DevMode) 
            {
                //LoadPlayerResponse loadPlayerResponse = await _loginHandler.LoadPlayer(player, player.Name);
                //if (loadPlayerResponse == LoadPlayerResponse.ABORT) return;
                //else
                //{
                //    WindowManager.Instance.Get<LoginWindow>().Hide(player);
                //    return;
                //  }
            }

            WindowManager.Instance.Get<LoginWindow>().Show(player, JsonConvert.SerializeObject(new LoginWindowObject() { name = player.Name }));
        }

        public async void RequestLoginResponse(PXPlayer player, string username, string hashedPassword)
        {
            if (player.LoggedIn) return;

            if (await _loginHandler.CheckLogin(player, hashedPassword))
            {
                LoadPlayerResponse loadPlayerResponse = await _loginHandler.LoadPlayer(player, player.Name);
                if (loadPlayerResponse == LoadPlayerResponse.ABORT) return;
                else
                {
                    WindowManager.Instance.Get<LoginWindow>().Hide(player);

                    // HANDLE EVERYTHING AFTER LOAD PLAYER
                    if(loadPlayerResponse == LoadPlayerResponse.NEW_PLAYER)
                    {
                        CharModule.Instance.CreatePlayerCharacter(player, CharCreationType.NEW);
                        return;
                    }
                }
            }
        }
    }
}
