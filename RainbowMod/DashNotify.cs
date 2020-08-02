using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.Mod.CelesteNet;
using Celeste.Mod.CelesteNet.Client;
using Celeste.Mod.CelesteNet.DataTypes;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.Steve
{
    class DashNotify : CelesteNetGameComponent
    {

        public DashNotify(CelesteNetClientContext context, Game game) : base(context, game)
        {
            UpdateOrder = 20300;
            Visible = false;
            Logger.Log(LogLevel.Debug.ToString(), "Initialized!");

        }

        public override void Initialize()
        {
            MainThreadHelper.Do(() =>
            {
                On.Celeste.Player.DashEnd += NotifyOnDash;
            });
        }

        private void NotifyOnDash(On.Celeste.Player.orig_DashEnd orig, Player self)
        {
            Logger.Log(LogLevel.Debug.ToString(), "NotifyOnDash");
            Client.SendAndHandle(new DataDashInfo()
            {
                PlayerName = Client.PlayerInfo.DisplayName
            });
        }

        protected override void Dispose(bool disposing)
        {
            MainThreadHelper.Do(() =>
            {
                On.Celeste.Player.DashEnd -= NotifyOnDash;
            });
        }

        public void Handle(CelesteNetConnection con, DataDashInfo dash)
        {
            DataChat msg = new DataChat
            {
                Player = Client.PlayerInfo,
                Text = dash.PlayerName + " has dashed."
            };
            Logger.Log(LogLevel.Debug.ToString(), "Sending Chat: " + msg.Text);

            Client.Send(msg);
        }
    }

    public class DataDashInfo : DataType<DataDashInfo>
    {
        public override DataFlags DataFlags => DataFlags.Update | DataFlags.Taskable;
        public string PlayerName;

        public override void Read(DataContext ctx, BinaryReader reader)
        {
            PlayerName = reader.ReadString();
        }

        public override void Write(DataContext ctx, BinaryWriter writer)
        {
            writer.Write(PlayerName);
        }
    }
}
