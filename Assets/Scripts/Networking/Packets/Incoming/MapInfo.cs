using Game;
using Models;
using Networking.Packets.Outgoing;

namespace Networking.Packets.Incoming
{
    public class MapInfo : IncomingPacket
    {
        public override PacketId Id => PacketId.MapInfo;
        public override IncomingPacket CreateInstance() => new MapInfo();

        public int Width;
        public int Height;
        public string Name;
        private string _displayName;
        private uint _seed;
        private int _background;
        private bool _showDisplays;
        private bool _allowTeleport;
        private string _music;

        public override void Read(PacketReader rdr)
        {
            Width = rdr.ReadInt32();
            Height = rdr.ReadInt32();
            Name = rdr.ReadString();
            _displayName = rdr.ReadString();
            _seed = rdr.ReadUInt32();
            _background = rdr.ReadInt32();
            _showDisplays = rdr.ReadBoolean();
            _allowTeleport = rdr.ReadBoolean();
            _music = rdr.ReadString();
        }

        public override void Handle(PacketHandler handler, Map map)
        {
            map.Initialize(this);
            handler.Random = new wRandom(_seed);
            
            if (handler.InitData.NewCharacter)
            {
                TcpTicker.Send(new Create(handler.InitData.ClassType, handler.InitData.SkinType));
            }
            else
            {
                TcpTicker.Send(new Load(handler.InitData.CharId)); 
            }
        }
    }
}