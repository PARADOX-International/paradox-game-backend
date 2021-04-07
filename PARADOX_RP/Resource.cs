﻿using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using PARADOX_RP.Core.Factories;
using PARADOX_RP.Core.Interface;
using PARADOX_RP.Handlers;
using System.Threading.Tasks;

namespace PARADOX_RP
{
    public class Resource : AsyncResource
    {
        private IApplication _application;
        public override void OnStart()
        {
            using var autofac = new PXContainer();
            autofac.RegisterTypes();
            autofac.ResolveTypes();

            _application = autofac.Resolve<IApplication>();
            _application.Start();
        }

        public override void OnStop()
        {
            throw new System.NotImplementedException();
        }

        public override IEntityFactory<IPlayer> GetPlayerFactory()
        {
            return new PXPlayerFactory();
        }

        public override IEntityFactory<IVehicle> GetVehicleFactory()
        {
            return new PXVehicleFactory();
        }
    }
}
