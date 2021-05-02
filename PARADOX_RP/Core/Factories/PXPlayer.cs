﻿using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using PARADOX_RP.Core.Database;
using PARADOX_RP.Core.Database.Models;
using PARADOX_RP.Game.Administration.Models;
using PARADOX_RP.Game.Commands.Extensions;
using PARADOX_RP.Game.Login;
using PARADOX_RP.Game.MiniGames.Models;
using PARADOX_RP.Game.Team;
using PARADOX_RP.Models;
using PARADOX_RP.Utils.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PARADOX_RP.Core.Factories
{
    public enum DimensionTypes
    {
        WORLD,
        TEAMHOUSE
    }

    public enum NotificationTypes
    {
        SUCCESS,
        ERROR
    }

    public enum DutyTypes
    {
        OFFDUTY,
        ONDUTY,
        ADMINDUTY
    }

    public enum MoneyTypes
    {
        MONEY,
        BANKMONEY
    }

    public class PXPlayer : Player
    {
        public int SqlId { get; set; }
        public bool LoggedIn { get; set; }
        public string Username { get; set; }

        private int _money { get; set; }
        public int Money
        {
            get => _money;
            set
            {
                this.EmitLocked("UpdateMoney", value);
                _money = value;
            }
        }


        public int BankMoney { get; set; }

        private bool _injured;
        public bool Injured
        {
            get => _injured;
            set
            {
                this.EmitLocked("UpdateInjured", value);
                _injured = value;
            }
        }

        private bool _cuffed;
        public bool Cuffed
        {
            get => _cuffed;
            set
            {
                this.EmitLocked("UpdateCuff", value);
                _cuffed = value;
            }
        }

        public SupportRankModel SupportRank { get; set; }
        public Teams Team { get; set; }
        public PlayerCustomization PlayerCustomization { get; set; }
        public PlayerTeamData PlayerTeamData { get; set; }
        public Invitation Invitation { get; set; }
        public string CurrentWindow { get; set; }
        public DimensionTypes DimensionType { get; set; }
        public DutyTypes DutyType { get; set; }
        public CancellationTokenSource CancellationToken { get; set; }
        public Dictionary<ComponentVariation, Clothes> Clothes { get; set; }
        public MinigameTypes Minigame { get; set; }

        internal PXPlayer(IntPtr nativePointer, ushort id) : base(nativePointer, id)
        {
            SqlId = -1;
            LoggedIn = false;
            Username = "";
            SupportRank = new SupportRankModel();
            Team = null;
            PlayerCustomization = null;
            PlayerTeamData = null;
            Invitation = null;
            DimensionType = DimensionTypes.WORLD;
            DutyType = DutyTypes.OFFDUTY;
            CancellationToken = null;
            Clothes = new Dictionary<ComponentVariation, Clothes>();
        }

        public async Task<bool> TakeMoney(int moneyAmount)
        {
            if (moneyAmount < 0) return false;

            if (moneyAmount > Money) return false;
            Money -= moneyAmount;
            await using (var px = new PXContext())
            {
                (await px.Players.FindAsync(SqlId)).Money = Money;
                await px.SaveChangesAsync();
            }
            return true;
        }

        public async Task<bool> AddMoney(int moneyAmount)
        {
            if (moneyAmount < 0) return false;
            Money += moneyAmount;
            await using (var px = new PXContext())
            {
                (await px.Players.FindAsync(SqlId)).Money = Money;
                await px.SaveChangesAsync();
            }
            return true;
        }

        public void SendNotification(string Title, string Message, NotificationTypes notificationType)
        {
            this.EmitLocked("PushNotification", Title, Message, 5000);
        }

        public Task SetClothes(int component, int drawable, int texture) => this.EmitAsync("SetClothes", component, drawable, texture);

        public Task StartEffect(string EffectName, int Duration) => this.EmitAsync("StartEffect", EffectName, Duration);

        public void Freeze(bool state)
        {
            this.EmitLocked("Freeze", state);
        }

        public void AddBlips(string label, Position pos, int number, int color, int scale, bool shortRange)
        {
            this.EmitLocked("AddBlips", label, pos, number, color, scale, shortRange);
        }

        public bool IsValid()
        {
            if (!LoggedIn) return false;
            if (SqlId < 1) return false;

            return true;
        }

        public bool CanInteract()
        {
            if (!LoggedIn) return false;
            if (CancellationToken != null) return false;
            if (Cuffed || Injured) return false;

            return true;
        }
    }

    internal class PXPlayerFactory : IEntityFactory<IPlayer>
    {
        public IPlayer Create(IntPtr entityPointer, ushort id)
        {
            return new PXPlayer(entityPointer, id);
        }
    }
}
